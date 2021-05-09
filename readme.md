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

If the `Product.Id` were unique among all products, and moreover, each of your 
entities had such a unique property already, you might decide to store all entities 
in a single table, with a fixed partition key matching the entity type name, for 
example. In this case, instead of a `TableRepository`, you can use a `TablePartition`:


```csharp
var account = CloudStorageAccount.DevelopmentStorageAccount; // or production one
// tableName will default to "Entity" and partition key to "Order", but they can 
// also be provided to the factory method to override the default behavior.
var repo = TablePartition.Create<Order>(storageAccount, order => order.Id);

var order = new Order("1234") 
{
  Amount = 25.5,
};

// Insert or Update behavior (aka "upsert")
await repo.PutAsync(order);

// Enumerate all orders within the partition
await foreach (var o in repo.EnumerateAsync()
   Console.WriteLine(o.Amount);

// Get previously saved order.
Order saved = await repo.GetAsync("1234");

// Delete order
await repo.DeleteAsync("1234");

// Can also delete passing entity
await repo.DeleteAsync(saved);
```

### Attributes

If you want to avoid using strings with the factory methods, you can also annotate the 
entity type to modify the default values used:

* `[Table("tableName")]`: class-level attribute to change the default when no value is provided
* `[PartitionKey]`: annotates the property that should be used as the partition key
* `[RowKey]`: annotates the property that should be used as the row key.



## Installation

```
> Install-Package Devlooped.TableStorage
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