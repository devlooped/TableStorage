using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace Devlooped
{
    public class Sample
    {
        public async Task RunAsync()
        {
            var repo = TableRepository.Create<Product>(
                CloudStorageAccount.DevelopmentStorageAccount,
                tableName: "Products",
                partitionKey: p => p.Category,
                rowKey: p => p.Id);

            await repo.PutAsync(new Product("book", "9781473217386")
            {
                Title = "Neuromancer",
                Price = 7.32
            });

            var docs = DocumentRepository.Create<Product>(
                CloudStorageAccount.DevelopmentStorageAccount,
                tableName: "Documents",
                partitionKey: p => p.Category,
                rowKey: p => p.Id);

            await docs.PutAsync(new Product("book", "9781473217386")
            {
                Title = "Neuromancer",
                Price = 7.32
            });

            var bin = DocumentRepository.Create<Product>(
                CloudStorageAccount.DevelopmentStorageAccount,
                tableName: "Documents",
                partitionKey: p => p.Category,
                rowKey: p => p.Id,
                serializer: BsonDocumentSerializer.Default);

            await docs.PutAsync(new Product("book", "9781473217386")
            {
                Title = "Neuromancer",
                Price = 7.32
            });

        }

        class Product
        {
            public Product(string category, string id)
            {
                Category = category;
                Id = id;
            }

            public string Category { get; }

            public string Id { get; }

            public string? Title { get; set; }

            public double Price { get; set; }
        }
    }
}
