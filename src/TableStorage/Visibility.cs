namespace Devlooped
{
    // Sets default visibility when using compiled version, where everything is public
    public partial class PartitionKeyAttribute { }
    public partial class RowKeyAttribute { }
    public partial class TableAttribute { }
    public partial class TableStorageAttribute { }
    public partial interface ITableRepository<T> { }
    public partial class TableRepository<T> { } 
}
