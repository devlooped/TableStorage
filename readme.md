![Icon](assets/img/icon-32.png) TableStorage
============

[![Version](https://img.shields.io/nuget/v/Devlooped.TableStorage.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.TableStorage)
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.TableStorage.svg?color=green)](https://www.nuget.org/packages/Devlooped.TableStorage)
[![License](https://img.shields.io/github/license/devlooped/TableStorage.svg?color=blue)](https://github.com/devlooped/TableStorage/blob/main/LICENSE)
[![Build](https://github.com/devlooped/TableStorage/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/TableStorage/actions)

Repository pattern with POCO object support for storing to Azure/CosmosDB Table Storage

## Usage

Given an entity like:

```csharp
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
```

> NOTE: entity can have custom constructor, key properties can be read-only, 
> and it doesn't need to inherit from anything, implement any interfaces or use 
> any custom attributes (unless you want to).

The entity can be stored and retrieved with:

```csharp
var account = CloudStorageAccount.DevelopmentStorageAccount; // or production one
// We lay out the parameter names for clarity only.
var repo = TableRepository.Create<Product>(storageAccount, 
    tableName: "Products",
    partitionKey: p => p.Category, 
    rowKey: p => p.Id);

var product = new Product("catId-asdf", "1234") 
{
  Title = "Table Storage is Cool",
  Price = 25.5,
};

// Insert or Update behavior (aka "upsert")
await repo.PutAsync(product);

// Enumerate all products in category "catId-asdf"
await foreach (var p in repo.EnumerateAsync("catId-asdf")
   Console.WriteLine(p.Price);

// Get previously saved product.
Product saved = await repo.GetAsync("catId-asdf", "1234");

// Delete product
await repo.DeleteAsync("catId-asdf", "1234");

// Can also delete passing entity
await repo.DeleteAsync(saved);
```

If a unique identifier among all entities exists already, you can also store all 
entities in a single table, using a fixed partition key matching the entity type name, for 
example. In such a case, instead of a `TableRepository`, you can use a `TablePartition`:

```csharp
class Region 
{
  public Region(string code, string name) 
    => (Id, Amount)
    = (id, amount);

  public string Code { get; }

  public string Name { get; }
}
```

```csharp
var account = CloudStorageAccount.DevelopmentStorageAccount; // or production one
// tableName will default to "Entity" and partition key to "Order", but they can 
// also be provided to the factory method to override the default behavior.
var repo = TablePartition.Create<Region>(storageAccount, region => region.Code);

var region = new Region("uk", "United Kingdom"); 

// Insert or Update behavior (aka "upsert")
await repo.PutAsync(region);

// Enumerate all regions within the partition
await foreach (var r in repo.EnumerateAsync()
   Console.WriteLine(r.Name);

// Get previously saved order.
Region saved = await repo.GetAsync("uk");

// Delete region
await repo.DeleteAsync("uk");

// Can also delete passing entity
await repo.DeleteAsync(saved);
```

This is quite convenient for handing reference data, for example. Enumerating all entries 
in the partition wouldn't be something you'd typically do for your "real" data, but for 
reference data, it could be useful.

### Attributes

If you want to avoid using strings with the factory methods, you can also annotate the 
entity type to modify the default values used:

* `[Table("tableName")]`: class-level attribute to change the default when no value is provided
* `[PartitionKey]`: annotates the property that should be used as the partition key
* `[RowKey]`: annotates the property that should be used as the row key.

Values passed to the `TableRepository.Create<T>` or `TablePartition.Create<T>` override 
declarative attributes.

## Installation

```
> Install-Package Devlooped.TableStorage
```

There is also a source-only version, if you want to avoid an additional assembly:

```
> Install-Package Devlooped.TableStorage.Source
```

The source-only package includes all types with the default visibility (internal), so you can decide 
what types to make public by declaring a partial class with the desired visibility. To make them all 
public, for example, you can include the same [Visibility.cs](https://github.com/devlooped/TableStorage/blob/main/src/TableStorage/Visibility.cs) 
that the compiled version uses, like:

```csharp
namespace Devlooped
{
    public partial interface ITableRepository<T> { }
    public partial interface ITablePartition<T> { }
    public partial class TableRepository { }
    public partial class TableRepository<T> { }
    public partial class TablePartition { }
    public partial class TablePartition<T> { }

    // Perhaps make the attributes visible too if you use them?
    public partial class TableAttribute { }
    public partial class PartitionKeyAttribute { }
    public partial class RowKeyAttribute { }
    public partial class TableStorageAttribute { }
}
```


## Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/Devlooped.TableStorage/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)
[![Build](https://github.com/devlooped/TableStorage/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/TableStorage/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.io/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`



## Sponsors

<h3 style="vertical-align: text-top" id="by-clarius">
<img src="https://raw.githubusercontent.com/devlooped/oss/main/assets/images/sponsors.svg" alt="sponsors" height="36" width="36" style="vertical-align: text-top; border: 0px; padding: 0px; margin: 0px">&nbsp;&nbsp;by&nbsp;<a href="https://github.com/clarius">@clarius</a>&nbsp;<img src="https://raw.githubusercontent.com/clarius/branding/main/logo/logo.svg" alt="sponsors" height="36" width="36" style="vertical-align: text-top; border: 0px; padding: 0px; margin: 0px">
</h3>

*[get mentioned here too](https://github.com/sponsors/devlooped)!*