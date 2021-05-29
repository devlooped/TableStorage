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
    }
}
