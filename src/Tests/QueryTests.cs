using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Xunit;

namespace Devlooped
{
    public class QueryTests
    {
        [Fact]
        public async Task CanTake()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var repo = TableRepository.Create<Book>(account, nameof(CanTake), x => "Book", x => x.ISBN);
            await LoadBooksAsync(repo);

            var query = from book in repo.CreateQuery()
                        where book.Format == BookFormat.Hardback && book.IsPublished
                        select new { book.ISBN, book.Title };

            var result = await query.Take(2).AsAsyncEnumerable().ToListAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task CanGetAll()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var repo = TableRepository.Create<Book>(account, nameof(CanGetAll), x => x.Author, x => x.ISBN);
            await LoadBooksAsync(repo);

            var result = await repo.CreateQuery().AsAsyncEnumerable().ToListAsync();
            var all = await repo.EnumerateAsync().ToListAsync();

            Assert.Equal(all.Count, result.Count);
        }

        [Fact]
        public async Task CanProject()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var repo = TableRepository.Create<Book>(account, nameof(CanProject), x => x.Author, x => x.ISBN);
            await LoadBooksAsync(repo);

            var hasResults = false;

            await foreach (var info in from book in repo.CreateQuery()
                                       where book.Author == "Rick Riordan" && book.Format == BookFormat.Hardback && book.IsPublished
                                       select new { book.ISBN, book.Title })
            {
                hasResults = true;
                Assert.NotNull(info.ISBN);
                Assert.NotNull(info.Title);
            }

            Assert.True(hasResults);
        }

        [Fact]
        public async Task CanProjectPartition()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;

            // Load with author + isbn as keys
            await LoadBooksAsync(TableRepository.Create<Book>(account, nameof(CanProjectPartition), x => x.Author, x => x.ISBN));

            // Query single author by scoping to partition key
            var repo = TablePartition.Create<Book>(account, nameof(CanProjectPartition), "Rick Riordan", x => x.ISBN);

            var hasResults = false;

            await foreach (var info in from book in repo.CreateQuery()
                                       where book.Format == BookFormat.Hardback && book.IsPublished
                                       select new { book.ISBN, book.Title })
            {
                hasResults = true;
                Assert.NotNull(info.ISBN);
                Assert.NotNull(info.Title);
            }

            Assert.True(hasResults);
        }

        [Fact]
        public async Task EnumFailsInTableClient()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var table = account.CreateCloudTableClient().GetTableReference(nameof(EnumFailsInTableClient));

            await table.CreateIfNotExistsAsync();

            Assert.Throws<StorageException>(() =>
                (from book in table.CreateQuery<BookEntity>()
                 where book.Format == BookFormat.Hardback && book.IsPublished
                 select new { book.Title })
                .ToList());
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

        public record Book(string ISBN, string Title, string Author, BookFormat Format, int? Pages = null, bool IsPublished = true);

        public class BookEntity : TableEntity
        {
            public bool IsPublished { get; set; }
            public string? Title { get; set; }
            public BookFormat Format { get; set; }
        }
    }
}
