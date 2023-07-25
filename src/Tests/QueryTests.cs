using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Xunit;
using Xunit.Abstractions;

namespace Devlooped
{
    public class QueryTests(ITestOutputHelper output)
    {
        string TableName([CallerMemberName] string? caller = default) => $"{nameof(QueryTests)}{caller}";

        [Fact]
        public async Task CanTake()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var repo = TableRepository.Create<Book>(account, TableName(), x => "Book", x => x.ISBN);
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
            var repo = TableRepository.Create<Book>(account, TableName(), x => x.Author, x => x.ISBN);
            await LoadBooksAsync(repo);

            var result = await repo.CreateQuery().AsAsyncEnumerable().ToListAsync();
            var all = await repo.EnumerateAsync().ToListAsync();

            Assert.Equal(all.Count, result.Count);
        }

        [Fact]
        public async Task CanProject()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var repo = TableRepository.Create<Book>(account, TableName(), x => x.Author, x => x.ISBN);

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
            await LoadBooksAsync(TableRepository.Create<Book>(account, TableName(), x => x.Author, x => x.ISBN));

            // Query single author by scoping to partition key
            var repo = TablePartition.Create<Book>(account, TableName(), "Rick Riordan", x => x.ISBN);

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
        public async Task CanFilterByRowKey()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            await LoadBooksAsync(TableRepository.Create<Book>(account, TableName(), x => x.Author, x => x.ISBN));

            var repo = TablePartition.Create<Book>(account, TableName(), "Rick Riordan", x => x.ISBN);

            // Get specific set of books from one particular publisher/country combination
            // in this case, 978-[English-speaking country, 1][Disney Editions, 4231]
            // See https://en.wikipedia.org/wiki/List_of_group-1_ISBN_publisher_codes
            var query = from book in repo.CreateQuery()
                        where
                            book.ISBN.CompareTo("97814231") >= 0 &&
                            book.ISBN.CompareTo("97814232") < 0
                        select new { book.ISBN, book.Title };

            var result = await query.AsAsyncEnumerable().ToListAsync();

            Assert.Equal(4, result.Count);
        }

        [Fact]
        public async Task CanFilterByColumn()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            await LoadBooksAsync(TableRepository.Create<Book>(account, TableName(), x => x.Author, x => x.ISBN));

            var repo = TablePartition.Create<Book>(account, TableName(), "Rick Riordan", x => x.ISBN);

            // Get specific set of books from one particular publisher/country combination
            // in this case, 978-[English-speaking country, 1][Disney Editions, 4231]
            // See https://en.wikipedia.org/wiki/List_of_group-1_ISBN_publisher_codes
            var query = from book in repo.CreateQuery()
                        where book.Pages >= 1000 && book.Pages <= 1500
                        select new { book.ISBN, book.Title };

            var result = await query.AsAsyncEnumerable().ToListAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task CanFilterDocuments()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            await LoadBooksAsync(DocumentRepository.Create<Book>(
                account, TableName(), x => x.Author, x => x.ISBN));

            var repo = DocumentPartition.Create<Book>(account, TableName(), "Rick Riordan", x => x.ISBN);

            //// Get specific set of books from one particular publisher/country combination
            //// in this case, 978-[English-speaking country, 1][Disney Editions, 4231]
            //// See https://en.wikipedia.org/wiki/List_of_group-1_ISBN_publisher_codes
            //var query = from book in repo.CreateQuery()
            //            where
            //                book.ISBN.CompareTo("97814231") >= 0 &&
            //                book.ISBN.CompareTo("97814232") < 0
            //            select new { book.ISBN, book.Title };

            //var result = await query.AsAsyncEnumerable().ToListAsync();

            //Assert.Equal(4, result.Count);
        }

        [Fact]
        public async Task EnumFailsInTableClient()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var table = account.CreateTableServiceClient().GetTableClient(TableName());

            await table.CreateIfNotExistsAsync();

            var books = table.QueryAsync<BookEntity>("Format eq 'Hardback'").ToHashSetAsync();

            //Assert.Throws<StorageException>(() =>
            //    (from book in table.CreateQuery<BookEntity>()
            //     where book.Format == BookFormat.Hardback && book.IsPublished
            //     select new { book.Title })
            //    .ToList());
        }

        [Fact]
        public async Task CanFilterByEntityRowKey()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            await LoadBooksAsync(TableRepository.Create<Book>(account, TableName(), x => x.Author, x => x.ISBN));
            var repo = TableRepository.Create(account, TableName());

            var query = from book in repo.CreateQuery()
                        where
                            book.PartitionKey == "Rick Riordan" &&
                            book.RowKey.CompareTo("97814231") >= 0 &&
                            book.RowKey.CompareTo("97814232") < 0
                        select book;

            // Can project direct entities
            Assert.Equal(4, (await query.AsAsyncEnumerable().ToListAsync()).Count);

            var entity = (await query.AsAsyncEnumerable().ToListAsync()).First();
            Assert.NotEmpty(entity.PartitionKey);
            Assert.NotEmpty(entity.RowKey);
            Assert.NotEqual(default(ETag), entity.ETag);
            Assert.NotEqual(DateTime.MinValue, entity.Timestamp);

            var projection = from book in repo.CreateQuery()
                             where
                                 book.PartitionKey == "Rick Riordan" &&
                                 book.RowKey.CompareTo("97814231") >= 0 &&
                                 book.RowKey.CompareTo("97814232") < 0
                             select new { Title = (string)book["Title"], Pages = (int)book["Pages"] };

            var result = await projection.AsAsyncEnumerable().ToListAsync();
            // Can project directly just the titles
            Assert.Equal(4, result.Count);

            Assert.Contains(result, x => x.Title == "The Son of Neptune");
            Assert.Contains(result, x => x.Title == "The Mark of Athena");
            Assert.Contains(result, x => x.Title == "Percy Jackson & the Olympians Boxed Set");
            Assert.Contains(result, x => x.Title == "The Blood of Olympus");

            // Can enumerate async directly too
            await foreach (var item in projection)
            {
                Assert.True(item.Pages > 100);
                Assert.NotEmpty(item.Title);
            }
        }

        [Fact]
        public async Task EmptyQueryDoesNotFail()
        {
            var account = CloudStorageAccount.DevelopmentStorageAccount;
            var repo = TableRepository.Create(account, TableName());

            var query = from book in repo.CreateQuery()
                        where
                            book.PartitionKey == "Rick Riordan" &&
                            book.RowKey.CompareTo("97814231") >= 0 &&
                            book.RowKey.CompareTo("97814232") < 0
                        select book;

            Assert.Empty((await query.AsAsyncEnumerable().ToListAsync()));
        }

        async Task LoadBooksAsync(ITableStorage<Book> books)
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

        public class BookEntity : ITableEntity
        {
            public bool IsPublished { get; set; }
            public string? Title { get; set; }
            public BookFormat Format { get; set; }

            public string PartitionKey { get; set; } = "";
            public string RowKey { get; set; } = "";
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; } = ETag.All;
        }
    }
}
