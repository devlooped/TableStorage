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
            var repo = new TableRepository<MyEntity>(CloudStorageAccount.DevelopmentStorageAccount, "Entities");
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
            var repo = new EntityRepository<MyEntity>(CloudStorageAccount.DevelopmentStorageAccount);
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
                new TableRepository<EntityNoRowKey>(CloudStorageAccount.DevelopmentStorageAccount, "Entities"));
        }

        [Fact]
        public void DefaultTableNameRemovesEntitySuffix()
        {
            Assert.Equal("My", TableRepository<MyEntity>.DefaultTableName);
        }

        [Fact]
        public void DefaultTableNameUsesAttribute()
        {
            Assert.Equal("Entities", TableRepository<MyTableEntity>.DefaultTableName);
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
