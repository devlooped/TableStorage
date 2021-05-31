using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Azure.Cosmos.Table;
using ProtoBuf;
using Xunit;

namespace Devlooped
{
    public class DocumentRepositoryTests
    {
        public static IEnumerable<object[]> Serializers => new object[][]
        {
            new object[] { JsonDocumentSerializer.Default },
            new object[] { BsonDocumentSerializer.Default },
            new object[] { MessagePackDocumentSerializer.Default },
            new object[] { ProtobufDocumentSerializer.Default },
        };

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task DocumentEndToEnd(IDocumentSerializer serializer)
        {
            var table = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient()
                .GetTableReference(serializer.GetType().Name);
            await table.DeleteIfExistsAsync();
            await table.CreateAsync();

            try
            {
                var repo = DocumentRepository.Create<DocumentEntity>(CloudStorageAccount.DevelopmentStorageAccount,
                    table.Name, serializer: serializer);

                var partitionKey = "P" + Guid.NewGuid().ToString("N");
                var rowKey = "R" + Guid.NewGuid().ToString("N");

                var entity = await repo.PutAsync(new DocumentEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    Title = "Foo",
                });

                entity.Title = "Bar";

                await repo.PutAsync(entity);

                var saved = await repo.GetAsync(partitionKey, rowKey);

                Assert.NotNull(saved);
                Assert.Equal("Bar", saved!.Title);

                var entities = new List<DocumentEntity>();

                await foreach (var e in repo.EnumerateAsync(partitionKey))
                    entities.Add(e);

                Assert.Single(entities);

                await repo.DeleteAsync(saved);

                Assert.Null(await repo.GetAsync(partitionKey, rowKey));

                await foreach (var _ in repo.EnumerateAsync(partitionKey))
                    Assert.False(true, "Did not expect to find any entities");
            }
            finally
            {
                await table.DeleteIfExistsAsync();
            }
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task DocumentPartitionEndToEnd(IDocumentSerializer serializer)
        {
            var table = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient()
                .GetTableReference(serializer.GetType().Name);
            await table.DeleteIfExistsAsync();
            await table.CreateAsync();

            try
            {
                var repo = DocumentPartition.Create<DocumentEntity>(CloudStorageAccount.DevelopmentStorageAccount,
                    table.Name, serializer: serializer);

                var partitionKey = "P" + Guid.NewGuid().ToString("N");
                var rowKey = "R" + Guid.NewGuid().ToString("N");

                var entity = await repo.PutAsync(new DocumentEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    Title = "Foo",
                });

                entity.Title = "Bar";

                await repo.PutAsync(entity);

                var saved = await repo.GetAsync(rowKey);

                Assert.NotNull(saved);
                Assert.Equal("Bar", saved!.Title);

                var entities = new List<DocumentEntity>();

                await foreach (var e in repo.EnumerateAsync())
                    entities.Add(e);

                Assert.Single(entities);

                await repo.DeleteAsync(saved);

                Assert.Null(await repo.GetAsync(rowKey));

                await foreach (var _ in repo.EnumerateAsync())
                    Assert.False(true, "Did not expect to find any entities");
            }
            finally
            {
                await table.DeleteAsync();
            }
        }

        [ProtoContract]
        [MessagePackObject]
        public class DocumentEntity
        {
            [PartitionKey]
            [Key(0)]
            [ProtoMember(1)]
            public string? PartitionKey { get; set; }
            [RowKey]
            [Key(1)]
            [ProtoMember(2)]
            public string? RowKey { get; set; }
            [Key(2)]
            [ProtoMember(3)]
            public string? Title { get; set; }
        }
    }
}
