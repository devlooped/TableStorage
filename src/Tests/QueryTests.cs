using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.OData.Client;
using Microsoft.OData.UriParser;
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
                        where book.Format == "Hardback" && book.IsPublished
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
                                       where book.Author == "Rick Riordan" && book.Format == "Hardback" && book.IsPublished
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
                                       where book.Format == "Hardback" && book.IsPublished
                                       select new { book.ISBN, book.Title })
            {
                hasResults = true;
                Assert.NotNull(info.ISBN);
                Assert.NotNull(info.Title);
            }

            Assert.True(hasResults);
        }

        async Task LoadBooksAsync(ITableRepository<Book> books)
        {
            foreach (var book in File.ReadAllLines("Books.csv").Skip(1)
                .Select(line => line.Split(','))
                .Select(values => new Book(values[1], values[2], values[3], values[4], int.Parse(values[5]), bool.Parse(values[6]))))
            {
                await books.PutAsync(book);
            }
        }

        public record Book(string ISBN, string Title, string Author, string Format, int? Pages = null, bool IsPublished = true);

        public class RequestStatusEntity : TableEntity
        {
            public RequestStatusEntity()
            {
            }

            public RequestStatusEntity(string ID) : base(nameof(RequestStatus), ID)
            {
            }

            public string? ID
            {
                get => RowKey;
                set => RowKey = value;
            }

            public int Status { get; set; }
            public string? Reason { get; set; }
        }

        public record RequestStatus(string ID)
        {
            public int Status { get; set; }
            public string? Reason { get; set; }
        }
    }
}
