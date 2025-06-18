using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using ProtoBuf;
using Xunit;

namespace Devlooped
{
    public class BlobStorageTests : TableStorageTests
    {
        protected override void Delete(string table)
        {
            var service = CloudStorageAccount.DevelopmentStorageAccount.CreateBlobServiceClient();
            var container = service.GetBlobContainerClient(table);
            container.DeleteIfExists();
        }

        protected override ITableStorage<DocumentEntity> CreateStorage(IDocumentSerializer serializer, string tableName)
            => BlobStorage.Create<DocumentEntity>(CloudStorageAccount.DevelopmentStorageAccount, tableName, serializer: serializer);

        protected override ITableStoragePartition<DocumentEntity> CreatePartition(IDocumentSerializer serializer, string tableName, string partitionKey)
            => BlobPartition.Create<DocumentEntity>(CloudStorageAccount.DevelopmentStorageAccount, tableName, partitionKey, serializer: serializer);
    }

    public abstract class TableStorageTests : IDisposable
    {
        public static IEnumerable<object[]> Serializers => new object[][]
        {
            new object[] { DocumentSerializer.Default },
            new object[] { JsonDocumentSerializer.Default },
            new object[] { BsonDocumentSerializer.Default },
            new object[] { MessagePackDocumentSerializer.Default },
            new object[] { ProtobufDocumentSerializer.Default },
        };

        string tableName = "a" + Guid.NewGuid().ToString("n");

        void IDisposable.Dispose() => Delete(tableName);

        protected abstract void Delete(string table);

        protected abstract ITableStorage<DocumentEntity> CreateStorage(IDocumentSerializer serializer, string tableName);

        protected abstract ITableStoragePartition<DocumentEntity> CreatePartition(IDocumentSerializer serializer, string tableName, string partitionKey);

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task StorageEndToEnd(IDocumentSerializer serializer)
        {
            var repo = CreateStorage(serializer, tableName);

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
            Assert.Equal(DateOnly.FromDateTime(DateTime.Today), saved.Date);

            var entities = new List<DocumentEntity>();

            await foreach (var e in repo.EnumerateAsync(partitionKey))
                entities.Add(e);

            Assert.Single(entities);

            await repo.DeleteAsync(saved);

            Assert.Null(await repo.GetAsync(partitionKey, rowKey));

            await foreach (var _ in repo.EnumerateAsync(partitionKey))
                Assert.Fail("Did not expect to find any entities");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task DocumentPartitionEndToEnd(IDocumentSerializer serializer)
        {
            var partitionKey = "P" + Guid.NewGuid().ToString("N");
            var repo = CreatePartition(serializer, tableName, partitionKey);
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
                Assert.Fail("Did not expect to find any entities");
        }

        [Fact]
        public async Task CanDeleteNonExistentEntity()
        {
            Assert.False(await CreateStorage(DocumentSerializer.Default, tableName)
                    .DeleteAsync("foo", "bar"));

            Assert.False(await CreatePartition(DocumentSerializer.Default, tableName, Guid.NewGuid().ToString("N"))
                    .DeleteAsync("foo"));
        }

        [ProtoContract]
        [MessagePackObject]
        public class DocumentEntity : IDocumentTimestamp
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
            [Key(3)]
            [ProtoMember(4, IsRequired = true)]
            public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
            [Key(4)]
            [ProtoMember(5)]
            public DateTimeOffset? Timestamp { get; set; }
        }
    }
}
