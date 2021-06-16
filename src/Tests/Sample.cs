using System;
using System.IO;
using System.Linq;
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

        public async Task BooksAsync()
        {
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;

            // We lay out the parameter names for clarity only.
            var repo = TableRepository.Create<Book>(storageAccount,
                tableName: "Books",
                partitionKey: p => p.Author,
                rowKey: p => p.ISBN);

            //await LoadBooksAsync(repo);

            // Create a new Book and save it
            await repo.PutAsync(
                new Book("9781473217386", "Neuromancer", "William Gibson", BookFormat.Paperback, 320));

            await foreach (var book in from b in repo.CreateQuery()
                                       where b.Author == "William Gibson" && b.Format == BookFormat.Paperback
                                       select new { b.Title })
            {
                Console.WriteLine(book.Title);
            }
        }

        async Task LoadBooksAsync(ITableRepository<Book> books)
        {
            foreach (var book in File.ReadAllLines("Books.csv").Skip(1)
                .Select(line => line.Split(','))
                .Select(values => new Book(values[1], values[2], values[3], Enum.Parse<BookFormat>(values[4]), int.Parse(values[5]), bool.Parse(values[6]))))
            {
                await books.PutAsync(book);
            }
        }

        public enum BookFormat { Paperback, Hardback }

        public record Book(string ISBN, string Title, string Author, BookFormat Format, int Pages, bool IsPublished = true);

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
