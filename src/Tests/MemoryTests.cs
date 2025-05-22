using System;
using System.Linq.Expressions;
using Azure.Data.Tables;
using Xunit.Abstractions;

namespace Devlooped;

public class MemoryDocTests : DocumentRepositoryTests
{
    protected override IDocumentRepository<DocumentEntity> CreateRepository(IDocumentSerializer serializer)
        => MemoryRepository.Create<DocumentEntity>();

    protected override IDocumentPartition<DocumentEntity> CreatePartition(IDocumentSerializer serializer)
        => MemoryPartition.Create<DocumentEntity>();

    protected override bool VerifyTableStorage => false;
}

public class MemoryRepoTests(ITestOutputHelper output) : RepositoryTests(output)
{
    protected override ITablePartition<TableEntity> CreatePartition(string partitionKey)
        => MemoryPartition.Create("Entities", partitionKey);

    protected override ITablePartition<T> CreatePartition<T>(string? partitionKey = null, Expression<Func<T, string>>? rowKey = null)
        => MemoryPartition.Create<T>(partitionKey: partitionKey, rowKey: rowKey);

    protected override ITableRepository<TableEntity> CreateRepository()
        => MemoryRepository.Create();

    protected override ITableRepository<T> CreateRepository<T>(Expression<Func<T, string>>? partitionKey = null, Expression<Func<T, string>>? rowKey = null)
        => MemoryRepository.Create<T>(partitionKey: partitionKey!, rowKey: rowKey!);
}
