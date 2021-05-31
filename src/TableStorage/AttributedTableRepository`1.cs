using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    /// <summary>
    /// A <see cref="ITableRepository{T}"/> implementation which relies on the entity type <typeparamref name="T"/>
    /// being annotated with <see cref="PartitionKeyAttribute"/> and <see cref="RowKeyAttribute"/>, and 
    /// optionally <see cref="TableAttribute"/> (defaults to type name).
    /// </summary>
    /// <remarks>
    /// When attributed entities are used, this is a convenient generic implementation for use with 
    /// a dependency injection container, such as in ASP.NET Core:
    /// <code>
    /// services.AddScoped(typeof(ITableRepository&lt;&gt;), typeof(AttributedTableRepository&lt;&gt;));
    /// </code>
    /// </remarks>
    partial class AttributedTableRepository<T> : TableRepository<T> where T : class
    {
        /// <summary>
        /// Initializes the repository using the given storage account.
        /// </summary>
        /// <param name="storageAccount">Storage account to connect to.</param>
        public AttributedTableRepository(CloudStorageAccount storageAccount)
            : base(storageAccount,
                  TableRepository.GetDefaultTableName<T>(),
                  PartitionKeyAttribute.CreateAccessor<T>(),
                  RowKeyAttribute.CreateAccessor<T>())
        {
        }
    }
}
