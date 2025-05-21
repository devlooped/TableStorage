using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Xunit;
using Xunit.Abstractions;
using static Devlooped.DocumentRepositoryTests;

namespace Devlooped;

public class RepositoryTests : IDisposable
{
    ITestOutputHelper output;
    CloudStorageAccount storage = CloudStorageAccount.DevelopmentStorageAccount;
    TableConnection table;

    public RepositoryTests(ITestOutputHelper output)
    {
        this.output = output;
        table = new TableConnection(storage, "a" + Guid.NewGuid().ToString("n"));
    }

    public void Dispose() => this.table.GetTableAsync().Result.Delete();

    [Fact]
    public async Task TableEndToEnd()
    {
        var repo = TableRepository.Create<MyEntity>(table);
        var entity = await repo.PutAsync(new MyEntity("asdf"));

        Assert.Equal("asdf", entity.Id);
        Assert.Null(entity.Name);
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), entity.Date);

        entity.Name = "kzu";

        await repo.PutAsync(entity);

        var saved = await repo.GetAsync("My", "asdf");

        Assert.NotNull(saved);
        Assert.Equal("kzu", saved!.Name);

        var entities = new List<MyEntity>();

        await foreach (var e in repo.EnumerateAsync("My"))
            entities.Add(e);

        Assert.Single(entities);

        Assert.True(await repo.DeleteAsync(saved));

        Assert.Null(await repo.GetAsync("My", "asdf"));

        await foreach (var _ in repo.EnumerateAsync("My"))
            Assert.Fail("Did not expect to find any entities");
    }

    [Fact]
    public async Task TableBatchEndToEnd()
    {
        var repo = TableRepository.Create<MyEntity>(CloudStorageAccount.DevelopmentStorageAccount, "BatchEntities");
        await repo.PutAsync(
        [
            new MyEntity("A"),
            new MyEntity("B"),
        ]);

        var entities = new List<MyEntity>();

        await foreach (var e in repo.EnumerateAsync("My"))
            entities.Add(e);

        Assert.Equal(2, entities.Count);
        Assert.Contains(entities, x => x.Id == "A");
        Assert.Contains(entities, x => x.Id == "B");
    }

    [Fact]
    public async Task TableRecordEndToEnd()
    {
        var repo = TableRepository.Create<AttributedRecordEntity>(table);
        output.WriteLine("Target table: " + repo.TableName);

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

        Assert.True(await repo.DeleteAsync(saved));

        Assert.Null(await repo.GetAsync("Book", "1234"));

        await foreach (var _ in repo.EnumerateAsync("Book"))
            Assert.Fail("Did not expect to find any entities");
    }

    [Fact]
    public async Task DoesNotDuplicateKeyProperties()
    {
        var repo = TableRepository.Create<RecordEntity>(this.table,
                x => x.Kind,
                x => x.ID);

        var entity = new RecordEntity("Request", "1234") { Status = "OK" };
        var saved = await repo.PutAsync(entity);

        Assert.True(entity.Equals(saved));

        var client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient();
        var table = client.GetTableClient(repo.TableName);

        var result = await table.GetEntityAsync<TableEntity>("Request", "1234");

        Assert.NotNull(result.Value);

        Assert.False(result.Value.ContainsKey(nameof(RecordEntity.Kind)));
        Assert.False(result.Value.ContainsKey(nameof(RecordEntity.ID)));
        Assert.True(result.Value.ContainsKey(nameof(RecordEntity.Status)));

        saved = await repo.GetAsync("Request", "1234");

        Assert.True(entity.Equals(saved));
    }
    static string Sanitize(string value) => new string(value.Where(c => !char.IsControl(c) && c != '|' && c != '/').ToArray());

    [Fact]
    public async Task PersistsKeyPropertyIfNotSimpleRetrieval()
    {
        var repo = TableRepository.Create<RecordEntity>(this.table,
                x => "Prefix-" + x.Kind,
                x => Sanitize(x.ID));

        await repo.DeleteAsync("Request", "1234");
        var entity = new RecordEntity("Request", "1234") { Status = "OK" };
        await repo.PutAsync(entity);

        var client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient();
        var table = client.GetTableClient(repo.TableName);
        var result = await table.GetEntityAsync<TableEntity>("Prefix-Request", "1234");

        Assert.NotNull(result.Value);

        Assert.True(result.Value.ContainsKey(nameof(RecordEntity.Kind)));
        Assert.True(result.Value.ContainsKey(nameof(RecordEntity.ID)));
    }

    [Fact]
    public async Task DoesNotDuplicateAttributedKeyProperties()
    {
        var repo = TableRepository.Create<AttributedRecordEntity>(table);
        await repo.DeleteAsync("Book", "1234");
        var entity = await repo.PutAsync(new AttributedRecordEntity("Book", "1234"));

        var generic = await new TableEntityRepository(CloudStorageAccount.DevelopmentStorageAccount, repo.TableName)
            .GetAsync(entity.Kind, entity.ID);

        Assert.NotNull(generic);
        Assert.False(generic.ContainsKey(nameof(entity.Kind)), "PartitionKey-annotated property should not be persisted by default");
        Assert.False(generic.ContainsKey(nameof(entity.ID)), "RowKey-annotated property should not be persisted by default");
    }

    [Fact]
    public async Task EntityEndToEnd()
    {
        var repo = TablePartition.Create<MyEntity>(table);
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

        Assert.True(await repo.DeleteAsync(saved));

        Assert.Null(await repo.GetAsync("asdf"));

        await foreach (var _ in repo.EnumerateAsync())
            Assert.Fail("Did not expect to find any entities");
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
    public async Task CanReadTimestamp()
    {
        await TablePartition
            .Create<TimestampedEntity>(
            table, "Timestamped", e => e.ID)
            .PutAsync(
                new TimestampedEntity("Foo"));

        Assert.NotNull((await TablePartition
            .Create<TimestampedEntity>(
            table, "Timestamped", e => e.ID)
            .GetAsync("Foo"))!.Timestamp);

        Assert.NotNull((await TablePartition
            .Create<StringTimestampedEntity>(
            table, "Timestamped", e => e.ID)
            .GetAsync("Foo"))!.Timestamp);

        Assert.NotNull((await TablePartition
            .Create<TimestampedDateTimeEntity>(
            table, "Timestamped", e => e.ID)
            .GetAsync("Foo"))!.Timestamp);
    }

    [Fact]
    public void DefaultTableNameUsesAttribute()
    {
        Assert.Equal("Entities", TableRepository.Create<MyTableEntity>(CloudStorageAccount.DevelopmentStorageAccount).TableName);
    }

    [Fact]
    public async Task TableEntityEndToEnd()
    {
        var repo = TableRepository.Create(table);
        var entity = await repo.PutAsync(new TableEntity("123", "Foo")
            {
                { "Bar", "Baz" }
            });

        Assert.Equal("123", entity.PartitionKey);
        Assert.Equal("Foo", entity.RowKey);
        Assert.Equal("Baz", entity["Bar"]);

        var saved = await repo.GetAsync("123", "Foo");

        Assert.NotNull(saved);
        Assert.Equal(entity.RowKey, saved!.RowKey);

        var entities = new List<ITableEntity>();

        await foreach (var e in repo.EnumerateAsync("123"))
            entities.Add(e);

        Assert.Single(entities);

        Assert.True(await repo.DeleteAsync(saved));

        Assert.Null(await repo.GetAsync("123", "Foo"));

        await foreach (var _ in repo.EnumerateAsync("123"))
            Assert.Fail("Did not expect to find any entities");
    }

    [Fact]
    public async Task TableEntityPartitionEndToEnd()
    {
        var partition = TablePartition.Create(table, "Watched");

        // Entity PartitionKey does not belong to the partition
        await Assert.ThrowsAsync<ArgumentException>(async () => await partition.PutAsync(new TableEntity("123", "Foo")));
        await Assert.ThrowsAsync<ArgumentException>(async () => await partition.DeleteAsync(new TableEntity("123", "Foo")));

        var entity = await partition.PutAsync(new TableEntity("Watched", "123"));

        Assert.Equal("Watched", entity.PartitionKey);
        Assert.Equal("123", entity.RowKey);

        var saved = await partition.GetAsync("123");

        Assert.NotNull(saved);
        Assert.Equal(entity.RowKey, saved!.RowKey);

        var entities = new List<ITableEntity>();

        await foreach (var e in partition.EnumerateAsync())
            entities.Add(e);

        Assert.Single(entities);

        Assert.True(await partition.DeleteAsync(saved));

        Assert.Null(await partition.GetAsync("123"));

        await foreach (var _ in partition.EnumerateAsync())
            Assert.Fail("Did not expect to find any entities");
    }

    [Fact]
    public async Task CanEnumerateEntities()
    {
        var partition = TablePartition.Create<MyEntity>(table, "Watched");

        await partition.PutAsync(new MyEntity("123") { Name = "Foo" });
        await partition.PutAsync(new MyEntity("456") { Name = "Bar" });

        var count = 0;
        await foreach (var entity in partition.EnumerateAsync())
            count++;

        Assert.Equal(2, count);

        var generic = TablePartition.Create(table, "Watched");

        await generic.PutAsync(new TableEntity("Watched", "789"));

        await foreach (var entity in generic.EnumerateAsync())
        {
            Assert.Equal("Watched", entity.PartitionKey);
            Assert.NotNull(entity.RowKey);
        }
    }

    [Fact]
    public async Task CanDeleteNonExistentEntity()
    {
        Assert.False(await TableRepository.Create<MyEntity>(table)
                .DeleteAsync("foo", "bar"));

        Assert.False(await TablePartition.Create<MyEntity>(table, "Watched")
                .DeleteAsync("foo"));

        Assert.False(await DocumentRepository.Create<MyEntity>(table)
                .DeleteAsync("foo", "bar"));

        Assert.False(await DocumentPartition.Create<MyEntity>(table, "Watched")
                .DeleteAsync("foo"));

        Assert.False(await TableRepository.Create(table)
                .DeleteAsync("foo", "bar"));

        Assert.False(await TablePartition.Create(table, "Watched")
                .DeleteAsync("foo"));
    }

    [Fact]
    public async Task CanMergeEntity()
    {
        var repo = TableRepository.Create<AttributedRecordEntity>(table, updateMode: TableUpdateMode.Merge);

        await repo.PutAsync(new AttributedRecordEntity("Book", "1234") { Status = "OK" });
        var record = await repo.PutAsync(new AttributedRecordEntity("Book", "1234") { Reason = "Done" });

        Assert.Equal("OK", record.Status);
        Assert.Equal("Done", record.Reason);

        await CloudStorageAccount.DevelopmentStorageAccount
            .CreateTableServiceClient()
            .GetTableClient(nameof(CanMergeEntity))
            .DeleteAsync();

        var partition = TablePartition.Create<AttributedRecordEntity>(table, updateMode: TableUpdateMode.Merge);

        await partition.PutAsync(new AttributedRecordEntity("Book", "1234") { Status = "OK" });
        record = await partition.PutAsync(new AttributedRecordEntity("Book", "1234") { Reason = "Done" });

        Assert.Equal("OK", record.Status);
        Assert.Equal("Done", record.Reason);
    }

    [Fact]
    public async Task CanMergeDynamicEntity()
    {
        var repo = TableRepository.Create(table, updateMode: TableUpdateMode.Merge);

        await repo.PutAsync(new TableEntity("Book", "1234")
            {
                { "Status", "OK" },
                { "Price", 7.32d },
            });

        await repo.PutAsync(new TableEntity("Book", "1234")
            {
                { "Reason", "Done" },
            });

        var result = await repo.GetAsync("Book", "1234");
        if (result is not TableEntity entity)
        {
            Assert.Fail("Expected TableEntity");
            return;
        }

        Assert.Equal("OK", entity["Status"]);
        Assert.Equal("Done", entity["Reason"]);
        Assert.Equal(7.32d, entity["Price"]);

        await CloudStorageAccount.DevelopmentStorageAccount
            .CreateTableServiceClient()
            .GetTableClient(nameof(CanMergeDynamicEntity))
            .DeleteAsync();

        var partition = TablePartition.Create(table, "Dynamic", updateMode: TableUpdateMode.Merge);

        await partition.PutAsync(new TableEntity("Dynamic", "1234")
            {
                { "Status", "OK" },
            });

        entity = (TableEntity)await partition.PutAsync(new TableEntity("Dynamic", "1234")
            {
                { "Reason", "Done" },
            });

        Assert.Equal("OK", entity["Status"]);
        Assert.Equal("Done", entity["Reason"]);
    }

    [Fact]
    public async Task CanMergeDifferentEntities()
    {
        await TablePartition
        .Create<MyEntity>(table, "Entity", updateMode: TableUpdateMode.Merge)
            .PutAsync(new MyEntity("1234") { Name = "kzu" });

        await TablePartition
        .Create<MyTableEntity>(table, "Entity", updateMode: TableUpdateMode.Merge)
            .PutAsync(new MyTableEntity("1234") { Notes = "rocks" });

        var entity = await TablePartition
        .Create(table, "Entity")
            .GetAsync("1234");

        Assert.NotNull(entity);
        Assert.Equal("kzu", entity["Name"]);
        Assert.Equal("rocks", entity["Notes"]);
    }

    [Fact]
    public void CanAnnotateFixedPartition()
    {
        var partition = TablePartition.Create<CustomPartition>(table, x => x.Id);

        Assert.Equal("MyPartition", partition.PartitionKey);
    }

    [Fact]
    public async Task CanPersistKeyProperties()
    {
        await new TablePartition<MyEntity>(table, "Entities")
        {
            PersistKeyProperties = true
        }.PutAsync(new MyEntity("1234") { Name = "kzu" });

        var entity = await TablePartition.Create(table, "Entities").GetAsync("1234");
        Assert.NotNull(entity);

        Assert.Equal("1234", entity["Id"]);
        Assert.Equal("kzu", entity["Name"]);
    }

    [Fact]
    public async Task CanGetFromEntity()
    {
        var repo = TableRepository.Create<AttributedRecordEntity>(CloudStorageAccount.DevelopmentStorageAccount);
        try
        {
            var entity = await repo.PutAsync(new AttributedRecordEntity("Book", "1234"));

            Assert.NotNull(entity);

            var stored = await repo.GetAsync(entity);

            Assert.NotNull(stored);
            Assert.Equal(entity.ID, stored.ID);
            Assert.Equal(entity.Kind, stored.Kind);

            var generic = await new TableEntityRepository(CloudStorageAccount.DevelopmentStorageAccount, TableRepository.GetDefaultTableName<AttributedRecordEntity>())
                .GetAsync(new TableEntity(entity.Kind, entity.ID));

            Assert.NotNull(generic);
            Assert.Equal(entity.ID, generic.RowKey);
            Assert.Equal(entity.Kind, generic.PartitionKey);
        }
        finally
        {
            await CloudStorageAccount.DevelopmentStorageAccount.CreateTableServiceClient()
                .GetTableClient(repo.TableName)
                .DeleteAsync();
        }
    }

    [Fact]
    public async Task CanGetFromEdmEntityTypes()
    {
        var repo = TableRepository.Create<EdmAnnotatedEntity>(table);
        var entity = new EdmAnnotatedEntity(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.Now, Encoding.UTF8.GetBytes("Hello World"), Random.Shared.NextInt64(int.MaxValue, long.MaxValue));

        var saved = await repo.PutAsync(entity);

        Assert.NotNull(saved);

        Assert.Equal(entity.ID, saved.ID);
        Assert.Equal(entity.Date, saved.Date);
        Assert.Equal("Hello World", Encoding.UTF8.GetString(saved.Data));
        Assert.Equal(entity.Count, saved.Count);

        var generic = await new TableEntityRepository(table)
                .GetAsync(new TableEntity(entity.Partition.ToString(), entity.ID.ToString()));

        Assert.NotNull(generic);
        Assert.Equal(entity.ID.ToString(), generic.RowKey);
        Assert.IsType<byte[]>(generic["Data"]);
        Assert.IsType<DateTimeOffset>(generic["Date"]);
        Assert.IsType<long>(generic["Count"]);
    }

    [Fact]
    public async Task CanPersistPropertiesFromComputedRowKeys()
    {
        var repo1 = TableRepository.Create<Dependency>(
        table,
            partitionKey: d => d.Repository,
            rowKey: d => $"{d.Name}|{d.Version}");

        var repo2 = TableRepository.Create<Dependency>(
        table,
            partitionKey: d => d.Name,
            rowKey: d => $"{d.Version}|{d.Repository}");

        var dep = new Dependency("org", "repo", "npm", "foo", "1.0");

        await repo1.PutAsync(dep);
        await repo2.PutAsync(dep);

        var entities = TableRepository.Create(table);

        var entity = await entities.GetAsync("repo", "foo|1.0");
        Assert.NotNull(entity);
        // Since the PK is the Repository property, it's not persisted as a property.
        Assert.Null(entity[nameof(Dependency.Repository)]);
        // But the name is, since it's a computed row key.
        Assert.Equal("foo", entity[nameof(Dependency.Name)]);

        entity = await entities.GetAsync("foo", "1.0|repo");
        Assert.NotNull(entity);
        // Conversely, since the PK here is the Name property, the Repository should be persisted.
        Assert.Equal("repo", entity[nameof(Dependency.Repository)]);
        // And the Name shouldn't, since it's the PK
        Assert.Null(entity[nameof(Dependency.Name)]);
    }

    [Fact]
    public async Task CanFilterBydate()
    {
        var repo = TablePartition.Create<TimestampedEntity>(
            table,
            nameof(TimestampedEntity),
            rowKey: x => x.ID);

        var first = await repo.PutAsync(new("Foo"));

        await Task.Delay(100);

        var second = await repo.PutAsync(new("Bar"));

        await Task.Delay(100);

        var third = await repo.PutAsync(new("Baz"));

        var typeName = typeof(DocumentEntity).FullName!.Replace('+', '.');

        var results = await repo.CreateQuery().Where(e => e.Timestamp >= second.Timestamp).AsAsyncEnumerable().ToListAsync();

        Assert.Equal(2, results.Count);

        Assert.Single(await repo.CreateQuery().Where(e =>
            e.Timestamp > second.Timestamp)
            .AsAsyncEnumerable()
            .ToListAsync());

        Assert.Single(await repo.CreateQuery().Where(e =>
            e.Timestamp < third.Timestamp &&
            e.Timestamp > first.Timestamp)
            .AsAsyncEnumerable()
            .ToListAsync());
    }

    record Dependency(string Organization, string Repository, string Type, string Name, string Version);

    class MyEntity
    {
        public MyEntity(string id) => Id = id;

        [RowKey]
        public string Id { get; }

        public string? Name { get; set; }

        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }

    [Table("Entities")]
    class MyTableEntity
    {
        public MyTableEntity(string id) => Id = id;

        [RowKey]
        public string Id { get; }

        public string? Notes { get; set; }
    }

    class EntityNoRowKey
    {
        public string? Id { get; set; }
    }

    record RecordEntity(string Kind, string ID)
    {
        public string? Status { get; set; }
    }

    [Table("EntityRequest")]
    record AttributedRecordEntity([PartitionKey] string Kind, [RowKey] string ID)
    {
        public string? Status { get; set; }
        public string? Reason { get; set; }
    }

    record TimestampedEntity(string ID)
    {
        public DateTimeOffset? Timestamp { get; set; }
    }

    record StringTimestampedEntity(string ID)
    {
        public string? Timestamp { get; set; }
    }

    record TimestampedDateTimeEntity(string ID)
    {
        public DateTime? Timestamp { get; set; }
    }

    [Table(nameof(EdmAnnotatedEntity))]
    record EdmAnnotatedEntity([PartitionKey] Guid Partition, [RowKey] Guid ID, DateTimeOffset Date, byte[] Data, long Count);

    [PartitionKey("MyPartition")]
    record CustomPartition(string Id);
}
