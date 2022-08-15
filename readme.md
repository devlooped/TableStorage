![Icon](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/icon-32.png) TableStorage
============

[![Version](https://img.shields.io/nuget/v/Devlooped.TableStorage.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.TableStorage) 
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.TableStorage.svg?color=green)](https://www.nuget.org/packages/Devlooped.TableStorage) 
[![License](https://img.shields.io/github/license/devlooped/TableStorage.svg?color=blue)](https://github.com/devlooped/TableStorage/blob/main/license.txt) 
[![Build](https://github.com/devlooped/TableStorage/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/TableStorage/actions)

Repository pattern with POCO object support for storing to Azure/CosmosDB Table Storage

![Screenshot of basic usage](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/tablestorage.png)

## Usage

Given an entity like:

```csharp
public record Product(string Category, string Id) 
{
  public string? Title { get; init; }
  public double Price { get; init; }
  public DateOnly Created { get; init; }
}
```

> NOTE: entity can have custom constructor, key properties can be read-only, 
> and it doesn't need to inherit from anything, implement any interfaces or use 
> any custom attributes (unless you want to). As shown above, it can even be 
> a simple record type, with support for .NET 6 DateOnly type to boot!

The entity can be stored and retrieved with:

```csharp
var storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // or production one
// We lay out the parameter names for clarity only.
var repo = TableRepository.Create<Product>(storageAccount, 
    tableName: "Products",
    partitionKey: p => p.Category, 
    rowKey: p => p.Id);

var product = new Product("Book", "1234") 
{
  Title = "Table Storage is Cool",
  Price = 25.5,
};

// Insert or Update behavior (aka "upsert")
await repo.PutAsync(product);

// Enumerate all products in category "Book"
await foreach (var p in repo.EnumerateAsync("Book"))
   Console.WriteLine(p.Price);

// Query books priced in the 20-50 range, 
// project just title + price
await foreach (var info in from prod in repo.CreateQuery()
                           where prod.Price >= 20 and prod.Price <= 50
                           select new { prod.Title, prod.Price })
  Console.WriteLine($"{info.Title}: {info.Price}");

// Get previously saved product.
Product saved = await repo.GetAsync("Book", "1234");

// Delete product
await repo.DeleteAsync("Book", "1234");

// Can also delete passing entity
await repo.DeleteAsync(saved);
```

If a unique identifier among all entities already exists, you can also store all 
entities in a single table, using a fixed partition key matching the entity type name, for 
example. In such a case, instead of a `TableRepository`, you can use a `TablePartition`:

```csharp
public record Book(string ISBN, string Title, string Author, BookFormat Format, int Pages);
```

```csharp
var storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // or production one

// Leverage defaults: TableName=Entities, PartitionKey=Book
var repo = TablePartition.Create<Region>(storageAccount, 
  rowKey: book => book.ISBN);

// insert/get/delete same API shown above.

// query filtering by rowKey, in this case, books by a certain 
// language/publisher combination. For Disney/Hyperion in English, 
// for example: ISBNs starting with 978(prefix)-1(english)-4231(publisher)
var query = from book in repo.CreateQuery()
            where 
                book.ISBN.CompareTo("97814231") >= 0 &&
                book.ISBN.CompareTo("97814232") < 0
            select new { book.ISBN, book.Title };

await foreach (var book in query)
   ...
```

For the books example above, it might make sense to partition by author, 
for example. In that case, you could use a `TableRepository<Book>` when 
saving:

```csharp
var repo = TableRepository.Create<Book>(storageAccount, "Books", x => x.Author, x => x.ISBN);

await repo.PutAsync(book);
```

And later on when listing/filtering books by a particular author, you can use 
a `TablePartition<Book>` so all querying is automatically scoped to that author:

```csharp
var partition = TablePartition.Create<Book>(storageAccount, "Books", "Rick Riordan", x => x.ISBN);

// Get Rick Riordan books, only from Disney/Hyperion, with over 1000 pages
var query = from book in repo.CreateQuery()
            where 
                book.ISBN.CompareTo("97814231") >= 0 &&
                book.ISBN.CompareTo("97814232") < 0 && 
                book.Pages >= 1000
            select new { book.ISBN, book.Title };
```

Using table partitions is quite convenient for handling reference data too, for example.
Enumerating all entries in the partition wouldn't be something you'd typically do for 
your "real" data, but for reference data, it could come in handy.

Stored entities use individual columns for properties, which makes it easy to browse 
the data (and query, as shown above!). If you don't need the individual columns, and would 
like a document-like storage mechanism instead, you can use the `DocumentRepository.Create` 
and `DocumentPartition.Create` factory methods instead. The API is otherwise the same, but 
you can see the effect of using one or the other in the following screenshots of the 
[Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) for the same 
`Product` entity shown in the first example above:

![Screenshot of entity persisted with separate columns for properties](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/entity.png)

![Screenshot of entity persisted as a document](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/document.png)

The code that persisted both entities is:

```csharp
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
```

The `Type` column persisted in the table is the `Type.FullName` of the persited entity, and the 
`Version` is the `Major.Minor` of its assembly, which could be used for advanced data migration scenarios. 
The major and minor version components are also provided as individual columns for easier querying 
by various version ranges, using `IDocumentRepository.EnumerateAsync(predicate)`.

In addition to the default built-in JSON plain-text based serializer (which uses the 
[System.Text.Json](https://www.nuget.org/packages/system.text.json) package), there are other 
alternative serializers for the document-based repository, including various binary serializers 
which will instead persist the document as a byte array:

[![Json.NET](https://img.shields.io/nuget/v/Devlooped.TableStorage.Newtonsoft.svg?color=royalblue&label=Newtonsoft)](https://www.nuget.org/packages/Devlooped.TableStorage.Newtonsoft) [![Bson](https://img.shields.io/nuget/v/Devlooped.TableStorage.Bson.svg?color=royalblue&label=Bson)](https://www.nuget.org/packages/Devlooped.TableStorage.Bson) [![MessagePack](https://img.shields.io/nuget/v/Devlooped.TableStorage.MessagePack.svg?color=royalblue&label=MessagePack)](https://www.nuget.org/packages/Devlooped.TableStorage.MessagePack) [![Protobuf](https://img.shields.io/nuget/v/Devlooped.TableStorage.Protobuf.svg?color=royalblue&label=Protobuf)](https://www.nuget.org/packages/Devlooped.TableStorage.Protobuf)

You can pass the serializer to use to the factory method as follows: 

```csharp
var repo = TableRepository.Create<Product>(...,
    serializer: [JsonDocumentSerializer|BsonDocumentSerializer|MessagePackDocumentSerializer|ProtobufDocumentSerializer].Default);
```

> NOTE: when using alternative serializers, entities might need to be annotated with whatever 
> attributes are required by the underlying libraries.

> NOTE: if access to the `Timestamp` managed by Table Storage for the entity is needed, just declare a property 
> with that name with either `DateTimeOffset`, `DateTime` or `string` type.

### Attributes

If you want to avoid using strings with the factory methods, you can also annotate the 
entity type to modify the default values used:

* `[Table("tableName")]`: class-level attribute to change the default when no value is provided
* `[PartitionKey]`: annotates the property that should be used as the partition key
* `[RowKey]`: annotates the property that should be used as the row key.

Values passed to the factory methods override declarative attributes.

For the products example above, your record entity could be:

```csharp
[Table("Products")]
public record Product([PartitionKey] string Category, [RowKey] string Id) 
{
  public string? Title { get; init; }
  public double Price { get; init; }
}
```

And creating the repository wouldn't require any arguments besides the storage account:

```csharp
var repo = TableRepository.Create<Product>(CloudStorageAccount.DevelopmentStorageAccount);
```

In addition, if you want to omit a particular property from persistence, you can annotate 
it with `[Browsable(false)]` and it will be skipped when persisting and reading the entity.


### TableEntity Support

Since these repository APIs are quite a bit more intuitive than working directly against a  
`TableClient`, you might want to retrieve/enumerate entities just by their built-in `TableEntity` 
properties, like `PartitionKey`, `RowKey`, `Timestamp` and `ETag`. For this scenario, we 
also support creating `ITableRepository<TableEntity>` and `ITablePartition<TableEntity>` 
by using the factory methods `TableRepository.Create(...)` and `TablePartition.Create(...)` 
without a (generic) entity type argument.

For example, given you know all `Region` entities saved in the example above, use the region `Code` 
as the `RowKey`, you could simply enumerate all regions without using the `Region` type at all:

```csharp
var account = CloudStorageAccount.DevelopmentStorageAccount; // or production one
var repo = TablePartition.Create(storageAccount, 
  tableName: "Reference",
  partitionKey: "Region");

// Enumerate all regions within the partition as plain TableEntities
await foreach (TableEntity region in repo.EnumerateAsync())
   Console.WriteLine(region.RowKey);
```

You can access and add additional properties by just using the entity indexer, which you can 
later persist by calling `PutAsync`:

```csharp
await repo.PutAsync(
    new TableEntity("Book", "9781473217386") 
    {
        ["Title"] = "Neuromancer",
        ["Price"] = 7.32
    });

var entity = await repo.GetAsync("Book", "9781473217386");

Assert.Equal("Neuromancer", entity["Title"]);
Assert.Equal(7.32, (double)entity["Price"]);
```

## Installation

```
> Install-Package Devlooped.TableStorage
```

All packages also come in source-only versions, if you want to avoid an additional assembly dependency:

```
> Install-Package Devlooped.TableStorage.Source
```

The source-only packages includes all types with the default visibility (internal), so you can decide 
what types to make public by declaring a partial class with the desired visibility. To make them all 
public, for example, you can include the same [Visibility.cs](https://github.com/devlooped/TableStorage/blob/main/src/TableStorage/Visibility.cs) 
that the compiled version uses.


## Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/Devlooped.TableStorage/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)
[![Build](https://github.com/devlooped/TableStorage/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/TableStorage/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.io/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`


<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->