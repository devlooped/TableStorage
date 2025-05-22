using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using MessagePack;
using ProtoBuf;
using Xunit;

namespace Devlooped
{
    public class DocumentRepositoryTests : IDisposable
    {
        public static IEnumerable<object[]> Serializers => new object[][]
        {
            new object[] { DocumentSerializer.Default },
            new object[] { JsonDocumentSerializer.Default },
            new object[] { BsonDocumentSerializer.Default },
            new object[] { MessagePackDocumentSerializer.Default },
            new object[] { ProtobufDocumentSerializer.Default },
        };

        TableConnection table = new TableConnection(CloudStorageAccount.DevelopmentStorageAccount, "a" + Guid.NewGuid().ToString("n"));
        void IDisposable.Dispose() => this.table.GetTableAsync().Result.Delete();

        protected virtual IDocumentRepository<DocumentEntity> CreateRepository(IDocumentSerializer serializer)
            => DocumentRepository.Create<DocumentEntity>(table, serializer: serializer);

        protected virtual IDocumentPartition<DocumentEntity> CreatePartition(IDocumentSerializer serializer)
            => DocumentPartition.Create<DocumentEntity>(table, serializer: serializer);

        protected virtual bool VerifyTableStorage => true;

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task DocumentEndToEnd(IDocumentSerializer serializer)
        {
            var repo = CreateRepository(serializer);

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

            // Verify that the entity is not serialized as a string for non-string serializer
            if (VerifyTableStorage && serializer is not IStringDocumentSerializer)
            {
                var generic = TableRepository.Create(table);
                var row = await generic.GetAsync(partitionKey, rowKey);
                Assert.NotNull(row);
                Assert.IsType<byte[]>(row["Document"]);
            }

            await repo.DeleteAsync(saved);

            Assert.Null(await repo.GetAsync(partitionKey, rowKey));

            await foreach (var _ in repo.EnumerateAsync(partitionKey))
                Assert.Fail("Did not expect to find any entities");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task DocumentPartitionEndToEnd(IDocumentSerializer serializer)
        {
            var repo = CreatePartition(serializer);

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
                Assert.Fail("Did not expect to find any entities");
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task CanQueryDocument(IDocumentSerializer serializer)
        {
            var repo = CreateRepository(serializer);

            var partitionKey = "P5943C610208D4008BEC052272ED07214";

            await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Bar",
                Title = "Bar",
            });

            await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Foo",
                Title = "Foo",
            });

            var typeName = typeof(DocumentEntity).FullName!.Replace('+', '.');

            var entities = await repo.EnumerateAsync(e =>
                e.PartitionKey == partitionKey &&
                e.RowKey.CompareTo("Foo") >= 0 && e.RowKey.CompareTo("Fop") < 0 &&
                e.Version != "1.0" &&
                e.Type == typeName)
                .ToListAsync();

            Assert.Single(entities);
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task CanFilterBydate(IDocumentSerializer serializer)
        {
            var repo = CreateRepository(serializer);

            var partitionKey = "P5943C610208D4008BEC052272ED07214";

            var first = await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Foo",
                Title = "Foo",
            });

            await Task.Delay(100);

            var second = await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Bar",
                Title = "Bar",
            });

            await Task.Delay(100);

            var third = await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Baz",
                Title = "Baz",
            });

            var typeName = typeof(DocumentEntity).FullName!.Replace('+', '.');

            var results = await repo.EnumerateAsync(e =>
                e.PartitionKey == partitionKey &&
                e.Timestamp >= second.Timestamp)
                .ToListAsync();

            Assert.Equal(2, results.Count);

            Assert.Single((await repo.EnumerateAsync(e =>
                e.PartitionKey == partitionKey &&
                e.Timestamp > second.Timestamp)
                .ToListAsync()));

            Assert.Single((await repo.EnumerateAsync(e =>
                e.PartitionKey == partitionKey &&
                e.Timestamp < third.Timestamp &&
                e.Timestamp > first.Timestamp)
                .ToListAsync()));
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task CanIncludeProperties(IDocumentSerializer serializer)
        {
            var repo = DocumentRepository.Create<DocumentEntity>(table, serializer: serializer, includeProperties: true);

            var partitionKey = "P5943C610208D4008BEC052272ED07214";

            await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Bar",
                Title = "Bar",
            });

            await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Foo",
                Title = "Foo",
            });

            var client = await table.GetTableAsync();
            var entity = client.GetEntity<TableEntity>(partitionKey, "Bar");
            Assert.Equal("Bar", entity.Value["Title"]);

            entity = client.GetEntity<TableEntity>(partitionKey, "Foo");
            Assert.Equal("Foo", entity.Value["Title"]);
        }

        [Theory]
        [MemberData(nameof(Serializers))]
        public async Task CanIncludePropertiesInParition(IDocumentSerializer serializer)
        {
            var partitionKey = "P5943C610208D4008BEC052272ED07214";
            var repo = DocumentPartition.Create<DocumentEntity>(table, partitionKey, serializer: serializer, includeProperties: true);

            await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Bar",
                Title = "Bar",
            });

            await repo.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Foo",
                Title = "Foo",
            });

            var client = await table.GetTableAsync();
            var entity = client.GetEntity<TableEntity>(partitionKey, "Bar");
            Assert.Equal("Bar", entity.Value["Title"]);

            entity = client.GetEntity<TableEntity>(partitionKey, "Foo");
            Assert.Equal("Foo", entity.Value["Title"]);
        }

        [Fact]
        public async Task CanDeleteNonExistentEntity()
        {
            Assert.False(await CreateRepository(DocumentSerializer.Default)
                    .DeleteAsync("foo", "bar"));

            Assert.False(await CreatePartition(DocumentSerializer.Default)
                    .DeleteAsync("foo"));
        }

        [Fact]
        public async Task CanQueryPartitionByExpression()
        {
            var partition = CreatePartition(DocumentSerializer.Default);
            var partitionKey = TablePartition.GetDefaultPartitionKey<DocumentEntity>();

            await partition.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Foo",
                Title = "Foo",
            });

            await partition.PutAsync(new DocumentEntity
            {
                PartitionKey = partitionKey,
                RowKey = "Bar",
                Title = "Bar",
            });

            var entity = await partition.EnumerateAsync(x => x.RowKey == "Bar").ToListAsync();

            Assert.Single(entity);
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
