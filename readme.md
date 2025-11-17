![Icon](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/icon-32.png) TableStorage
============

[![Version](https://img.shields.io/nuget/v/Devlooped.TableStorage.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.TableStorage) 
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.TableStorage.svg?color=darkmagenta)](https://www.nuget.org/packages/Devlooped.TableStorage) 
[![EULA](https://img.shields.io/badge/EULA-OSMF-blue?labelColor=black&color=C9FF30)](osmfeula.txt)
[![OSS](https://img.shields.io/github/license/devlooped/oss.svg?color=blue)](license.txt) 

<!-- include https://github.com/devlooped/.github/raw/main/osmf.md -->

<!-- #content -->
Repository pattern with POCO object support for storing to Azure/CosmosDB Table Storage

![Screenshot of basic usage](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/tablestorage.png)

> NOTE: This library is a thin wrapper around the latest Azure SDK v12+ for Table Storage,
> and uses [CloudStorageAccount](https://www.nuget.org/packages/Devlooped.CloudStorageAccount) which 
> is a 100% backwards compatible implementation of the Azure SDK v11 `CloudStorageAccount` class.

## Usage

Given an entity like:

```csharp
public record Product(string Category, string Id) 
{
  public required string? Title { get; init; }
  public double Price { get; init; }
  public DateOnly CreatedAt { get; init; }
}
```

> NOTE: entity can have custom constructor, key properties can be read-only 
> (Category and Id in this case for example), and it doesn't need to inherit 
> from anything, implement any interfaces or use 
> any custom attributes (unless you want to). As shown above, it can even be 
> a simple record type.

The entity can be stored and retrieved with:

```csharp
var storageAccount = CloudStorageAccount.DevelopmentStorageAccount; // or production one
// We lay out the parameter names for clarity only.
var repo = TableRepository.Create<Product>(storageAccount, 
    tableName: "Products",
    partitionKey: p => p.Category, 
    rowKey: p => p.Id);

var product = new Product("book", "1234") 
{
  Title = "Table Storage is Cool",
  Price = 25.5,
};

// Insert or Update behavior (aka "upsert")
await repo.PutAsync(product);

// Enumerate all products in category "book"
await foreach (var p in repo.EnumerateAsync("book"))
   Console.WriteLine(p.Price);

// Query books priced in the 20-50 range, 
// project just title + price
await foreach (var info in from prod in repo.CreateQuery()
                           where prod.Price >= 20 and prod.Price <= 50
                           select new { prod.Title, prod.Price })
  Console.WriteLine($"{info.Title}: {info.Price}");

// Get previously saved product.
Product saved = await repo.GetAsync("book", "1234");

// Delete product
await repo.DeleteAsync("book", "1234");

// Can also delete passing entity
await repo.DeleteAsync(saved);
```

Attributes can also be used to eliminate the need for lambdas altogether when 
the entity storage layout is known at compile time:

```csharp
[Table("Products")]
public record Product([PartitionKey] string Category, [RowKey] string Id) ... 

var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
// Everything discovered from attributes.
var repo = TableRepository.Create<Product>(storageAccount);
```

See the [Attributes](#attributes) section below for more details on how to use them.

If the product were books for example, it might make sense to partition by author. 
In that case, you could use a `TableRepository<Book>` when saving:

```csharp
public record Book([RowKey] string ISBN, string Title, string Author, BookFormat Format, int Pages);

var repo = TableRepository.Create<Product>(storageAccount, "Books",
  partitionKey: x => x.Author);

await repo.PutAsync(book);
```

> Note how you can mix and match attributes and explicit lambdas as needed. 
> The latter takes precedence over the former.

And later on when listing/filtering books by a particular author, you can use 
a `TablePartition<Product>` so all querying is automatically scoped to that author:

```csharp
var partition = TablePartition.Create<Book>(storageAccount, "Books", 
  partitionKey: "Rick Riordan");

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

> NOTE: if access to the `Timestamp` managed by Azure Table Storage for the entity is needed, 
> just declare a property with that name with either `DateTimeOffset`, `DateTime` or `string` type
> to read it.

Stored entities using `TableRepository` and `TablePartition` use individual columns for 
properties, which makes it easy to browse the data (and query, as shown above!). 

> NOTE: partition and row keys can also be typed as `Guid`

Document-based storage is also available via `DocumentRepository` and `DocumentPartition` if 
you don't need the individual columns.

<!-- #documents -->
## Document Storage

The `DocumentRepository.Create` and `DocumentPartition.Create` factory methods provide access 
to document-based storage, exposing the a similar API as column-based storage. 

Document repositories cause entities to be persisted as a single document column, alongside type and version 
information to handle versioning at the app level as needed. 

The API is mostly the same as for column-based repositories (document repositories implement 
the same underlying `ITableStorage` interface):

```csharp
public record Product(string Category, string Id) 
{
  public string? Title { get; init; }
  public double Price { get; init; }
  public DateOnly CreatedAt { get; init; }
}

var book = new Product("book", "9781473217386")
{
    Title = "Neuromancer",
    Price = 7.32
};

// Column-based storage
var repo = TableRepository.Create<Product>(
    CloudStorageAccount.DevelopmentStorageAccount,
    tableName: "Products",
    partitionKey: p => p.Category,
    rowKey: p => p.Id);

await repo.PutAsync(book);

// Document-based storage
var docs = DocumentRepository.Create<Product>(
    CloudStorageAccount.DevelopmentStorageAccount,
    tableName: "Documents",
    partitionKey: p => p.Category,
    rowKey: p => p.Id
    serializer: [SERIALIZER]);

await docs.PutAsync(book);
```

> If not provided, the serializer defaults to the `System.Text.Json`-based `DocumentSerializer.Default`.

The resulting differences in storage can be seen in the following screenshots of the 
[Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/):

![Screenshot of entity persisted with separate columns for properties](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/entity.png)

![Screenshot of entity persisted as a document](https://raw.githubusercontent.com/devlooped/TableStorage/main/assets/img/document.png)


The `Type` column persisted in the documents table is the `Type.FullName` of the persisted entity, and the 
`Version` is the `[Major].[Minor]` of its assembly, which could be used for advanced data migration scenarios. 
The major and minor version components are also provided as individual columns for easier querying 
by various version ranges, using `IDocumentRepository.EnumerateAsync(predicate)`.

If the serialized documents need to access the `Timestamp` managed by Azure Table 
Storage, you can implement `IDocumentTimestamp` in your entity type.

<!-- #documents -->

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


## Attributes

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


## TableEntity Support

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

<!-- #content -->

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

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.app/vpre/Devlooped.TableStorage/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.app/index.json)
[![Build](https://github.com/devlooped/TableStorage/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/TableStorage/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.app/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`

<!-- #sponsors -->
<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek-gh.png "Torutek")](https://github.com/torutek-gh)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![sorahex](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sorahex.png "sorahex")](https://github.com/sorahex)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/v-limo.png "Vincent Limo")](https://github.com/v-limo)
[![Jordan S. Jones](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jordansjones.png "Jordan S. Jones")](https://github.com/jordansjones)
[![domischell](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DominicSchell.png "domischell")](https://github.com/DominicSchell)
[![Justin Wendlandt](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jwendl.png "Justin Wendlandt")](https://github.com/jwendl)
[![Adrian Alonso](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/adalon.png "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Eule02.png "Michael Hagedorn")](https://github.com/Eule02)
[![Matt Frear](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/mattfrear.png "Matt Frear")](https://github.com/mattfrear)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
