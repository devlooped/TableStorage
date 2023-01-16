using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Devlooped
{
    /// <summary>
    /// Represents a connection to a given <see cref="TableName"/> 
    /// over a given <see cref="CloudStorageAccount"/>.
    /// </summary>
    partial class TableConnection
    {
        readonly CloudStorageAccount storageAccount;
        TableClient? table;

        /// <summary>
        /// Creates the affinitized table connection for the given table name.
        /// </summary>
        /// <param name="storageAccount">The storage account to use.</param>
        /// <param name="tableName">The table to connect to.</param>
        public TableConnection(CloudStorageAccount storageAccount, string tableName)
        {
            this.storageAccount = storageAccount;
            TableName = tableName;
        }

        /// <summary>
        /// Gets the storage account used to connect to the table.
        /// </summary>
        public CloudStorageAccount StorageAccount => storageAccount;

        /// <summary>
        /// Gets the name of the table to use.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Gets table client for this connection, creating the table if it doesn't exist.
        /// </summary>
        public async Task<TableClient> GetTableAsync() => table ??= await CreateTableClientAsync();

        async Task<TableClient> CreateTableClientAsync()
        {
            var tableClient = storageAccount.CreateTableServiceClient();
            var table = tableClient.GetTableClient(TableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
