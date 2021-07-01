using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace Devlooped
{
    public class RepositoryTests
    {
        [Fact]
        public async Task TableEndToEnd()
        {
            var repo = TableRepository.Create<MyEntity>(CloudStorageAccount.DevelopmentStorageAccount, "Entities");
            var entity = await repo.PutAsync(new MyEntity("asdf"));

            Assert.Equal("asdf", entity.Id);
            Assert.Null(entity.Name);

            entity.Name = "kzu";

            await repo.PutAsync(entity);

            var saved = await repo.GetAsync("My", "asdf");

            Assert.NotNull(saved);
            Assert.Equal("kzu", saved!.Name);

            var entities = new List<MyEntity>();

            await foreach (var e in repo.EnumerateAsync("My"))
                entities.Add(e);

            Assert.Single(entities);

            await repo.DeleteAsync(saved);

            Assert.Null(await repo.GetAsync("My", "asdf"));

            await foreach (var _ in repo.EnumerateAsync("My"))
                Assert.False(true, "Did not expect to find any entities");
        }

        [Fact]
        public async Task TableRecordEndToEnd()
        {
            var repo = TableRepository.Create<AttributedRecordEntity>(CloudStorageAccount.DevelopmentStorageAccount);
            var entity = await repo.PutAsync(new AttributedRecordEntity("Book", "1234"));

            Assert.Equal("1234", entity.ID);
            Assert.Null(entity.Status);

            entity.Status = "Pending";

            await repo.PutAsync(entity);

            var saved = await repo.GetAsync("Book", "1234");

            Assert.NotNull(saved);
            Assert.Equal("Pending", saved!.Status);

            var entities = new List<AttributedRecordEntity>();

            await foreach (var e in repo.EnumerateAsync("Book"))
                entities.Add(e);

            Assert.Single(entities);

            await repo.DeleteAsync(saved);

            Assert.Null(await repo.GetAsync("Book", "1234"));

            await foreach (var _ in repo.EnumerateAsync("Book"))
                Assert.False(true, "Did not expect to find any entities");
        }

        [Fact]
        public async Task DoesNotDuplicateKeyProperties()
        {
            var repo = TableRepository.Create<RecordEntity>(CloudStorageAccount.DevelopmentStorageAccount,
                x => x.Kind,
                x => x.ID);

            var entity = new RecordEntity("Request", "1234") { Status = "OK" };
            var saved = await repo.PutAsync(entity);

            Assert.True(entity.Equals(saved));

            var client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient();
            var table = client.GetTableReference(repo.TableName);

            var result = await table.ExecuteAsync(TableOperation.Retrieve("Request", "1234"));

            Assert.NotNull(result.Result);

            var dynamic = (DynamicTableEntity)result.Result;

            Assert.False(dynamic.Properties.ContainsKey(nameof(RecordEntity.Kind)));
            Assert.False(dynamic.Properties.ContainsKey(nameof(RecordEntity.ID)));
            Assert.True(dynamic.Properties.ContainsKey(nameof(RecordEntity.Status)));

            saved = await repo.GetAsync("Request", "1234");

            Assert.True(entity.Equals(saved));
        }

        [Fact]
        public async Task EntityEndToEnd()
        {
            var repo = TablePartition.Create<MyEntity>(CloudStorageAccount.DevelopmentStorageAccount);
            var entity = await repo.PutAsync(new MyEntity("asdf"));

            Assert.Equal("asdf", entity.Id);
            Assert.Null(entity.Name);

            entity.Name = "kzu";

            await repo.PutAsync(entity);

            var saved = await repo.GetAsync("asdf");

            Assert.NotNull(saved);
            Assert.Equal("kzu", saved!.Name);

            var entities = new List<MyEntity>();

            await foreach (var e in repo.EnumerateAsync())
                entities.Add(e);

            Assert.Single(entities);

            await repo.DeleteAsync(saved);

            Assert.Null(await repo.GetAsync("asdf"));

            await foreach (var _ in repo.EnumerateAsync())
                Assert.False(true, "Did not expect to find any entities");
        }

        [Fact]
        public void ThrowsIfEntityHasNoRowKey()
        {
            Assert.Throws<ArgumentException>(() =>
                TableRepository.Create<EntityNoRowKey>(CloudStorageAccount.DevelopmentStorageAccount, "Entities"));
        }

        [Fact]
        public void CanSpecifyPartitionAndRowKeyLambdas()
        {
            TableRepository.Create<EntityNoRowKey>(CloudStorageAccount.DevelopmentStorageAccount, "Entities",
                e => "FixedPartition",
                e => e.Id ?? "");
        }

        [Fact]
        public void CanSpecifyRowKeyLambda()
        {
            TablePartition.Create<EntityNoRowKey>(CloudStorageAccount.DevelopmentStorageAccount, e => e.Id ?? "");
        }

        [Fact]
        public void DefaultTableNameUsesAttribute()
        {
            Assert.Equal("Entities", TableRepository.Create<MyTableEntity>(CloudStorageAccount.DevelopmentStorageAccount).TableName);
        }

        [Fact]
        public async Task TableEntityEndToEnd()
        {
            var repo = TableRepository.Create(CloudStorageAccount.DevelopmentStorageAccount, "Entities");
            var entity = await repo.PutAsync(new TableEntity("123", "Foo"));

            Assert.Equal("123", entity.PartitionKey);
            Assert.Equal("Foo", entity.RowKey);

            var saved = await repo.GetAsync("123", "Foo");

            Assert.NotNull(saved);
            Assert.Equal(entity.RowKey, saved!.RowKey);

            var entities = new List<TableEntity>();

            await foreach (var e in repo.EnumerateAsync("123"))
                entities.Add(e);

            Assert.Single(entities);

            await repo.DeleteAsync(saved);

            Assert.Null(await repo.GetAsync("123", "Foo"));

            await foreach (var _ in repo.EnumerateAsync("123"))
                Assert.False(true, "Did not expect to find any entities");
        }

        [Fact]
        public async Task TableEntityPartitionEndToEnd()
        {
            var partition = TablePartition.Create(CloudStorageAccount.DevelopmentStorageAccount, "Entities", "Watched");

            // Entity PartitionKey does not belong to the partition
            await Assert.ThrowsAsync<ArgumentException>(async () => await partition.PutAsync(new TableEntity("123", "Foo")));
            await Assert.ThrowsAsync<ArgumentException>(async () => await partition.DeleteAsync(new TableEntity("123", "Foo")));

            var entity = await partition.PutAsync(new TableEntity("Watched", "123"));

            Assert.Equal("Watched", entity.PartitionKey);
            Assert.Equal("123", entity.RowKey);

            var saved = await partition.GetAsync("123");

            Assert.NotNull(saved);
            Assert.Equal(entity.RowKey, saved!.RowKey);

            var entities = new List<TableEntity>();

            await foreach (var e in partition.EnumerateAsync())
                entities.Add(e);

            Assert.Single(entities);

            await partition.DeleteAsync(saved);

            Assert.Null(await partition.GetAsync("123"));

            await foreach (var _ in partition.EnumerateAsync())
                Assert.False(true, "Did not expect to find any entities");
        }

        [Fact]
        public async Task CanEnumerateEntities()
        {
            await CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient().GetTableReference(nameof(CanEnumerateEntities))
                .DeleteIfExistsAsync();

            var partition = TablePartition.Create<MyEntity>(CloudStorageAccount.DevelopmentStorageAccount, nameof(CanEnumerateEntities), "Watched");

            await partition.PutAsync(new MyEntity("123") { Name = "Foo" });
            await partition.PutAsync(new MyEntity("456") { Name = "Bar" });

            var count = 0;
            await foreach (var entity in partition.EnumerateAsync())
                count++;

            Assert.Equal(2, count);

            var generic = TablePartition.Create(CloudStorageAccount.DevelopmentStorageAccount, nameof(CanEnumerateEntities), "Watched");

            await generic.PutAsync(new TableEntity("Watched", "789"));

            await foreach (var entity in generic.EnumerateAsync())
            {
                Assert.Equal("Watched", entity.PartitionKey);
                Assert.NotNull(entity.RowKey);
            }
        }

        [Fact]
        public async Task CanMergeEntity()
        {
            await CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient().GetTableReference(nameof(CanMergeEntity))
                .DeleteIfExistsAsync();

            var repo = TableRepository.Create<AttributedRecordEntity>(CloudStorageAccount.DevelopmentStorageAccount, nameof(CanMergeEntity), updateStrategy: UpdateStrategy.Merge);

            await repo.PutAsync(new AttributedRecordEntity("Book", "1234") { Status = "OK" });
            var record = await repo.PutAsync(new AttributedRecordEntity("Book", "1234") { Reason = "Done" });

            Assert.Equal("OK", record.Status);

            await CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient().GetTableReference(nameof(CanMergeEntity))
                .DeleteIfExistsAsync();

            var partition = TablePartition.Create<AttributedRecordEntity>(CloudStorageAccount.DevelopmentStorageAccount, nameof(CanMergeEntity), updateStrategy: UpdateStrategy.Merge);

            await partition.PutAsync(new AttributedRecordEntity("Book", "1234") { Status = "OK" });
            record = await partition.PutAsync(new AttributedRecordEntity("Book", "1234") { Reason = "Done" });

            Assert.Equal("OK", record.Status);
        }

        class MyEntity
        {
            public MyEntity(string id) => Id = id;

            [RowKey]
            public string Id { get; }

            public string? Name { get; set; }
        }

        [Table("Entities")]
        class MyTableEntity
        {
            public MyTableEntity(string id) => Id = id;

            [RowKey]
            public string Id { get; }
        }

        class EntityNoRowKey
        {
            public string? Id { get; set; }
        }

        record RecordEntity(string Kind, string ID)
        {
            public string? Status { get; set; }
        }

        [Table("Record")]
        record AttributedRecordEntity([PartitionKey] string Kind, [RowKey] string ID)
        {
            public string? Status { get; set; }
            public string? Reason { get; set; }
        }
    }
}
