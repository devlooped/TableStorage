![Icon](assets/img/icon32.png) TableStorage
============

[![Version](https://img.shields.io/nuget/v/Devlooped.TableStorage.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.TableStorage)
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.TableStorage.svg?color=green)](https://www.nuget.org/packages/Devlooped.TableStorage)
[![License](https://img.shields.io/github/license/devlooped/TableStorage.svg?color=blue)](https://github.com/devlooped/TableStorage/blob/main/LICENSE)
[![Build](https://github.com/devlooped/TableStorage/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/TableStorage/actions)

Repository pattern with POCO object support for storing to Azure/CosmosDB Table Storage

## Usage

Given an entity like:

```csharp
[Table("Products")]
class Product 
{
  public Product(string category, string id) 
  {
    Category = category;
    Id = id;
  }

  [PartitionKey]
  public string Category { get; }  

  [RowKey]
  public string Id { get; }

  public string? Title { get; set; }

  public double Price { get; set; }
}
```

it can be stored and retrieved with:

```csharp
var account = CloudStorageAccount.DevelopmentStorageAccount; // or production one
var repo = new TableRepository<Product>(storageAccount);

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