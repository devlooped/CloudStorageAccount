![Icon](https://raw.githubusercontent.com/devlooped/CloudStorageAccount/main/assets/img/icon-32.png) CloudStorageAccount
============

[![Version](https://img.shields.io/nuget/v/Devlooped.CloudStorageAccount.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.CloudStorageAccount) 
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.CloudStorageAccount.svg?color=green)](https://www.nuget.org/packages/Devlooped.CloudStorageAccount) 
[![License](https://img.shields.io/github/license/devlooped/CloudStorageAccount.svg?color=blue)](https://github.com/devlooped/CloudStorageAccount/blob/main/license.txt) 
[![Build](https://github.com/devlooped/CloudStorageAccount/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/CloudStorageAccount/actions)

CloudStorageAccount for Azure Storage v12+.

# Overview
<!-- #Overview -->
The new unified Azure Storage and Tables client libraries do away with the 
[CloudStorageAccount](https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.storage.cloudstorageaccount?view=azure-dotnet) 
that was typically used. This makes migration a bit painful, as noted in:

* [Azure.Data.Tables](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/tables/Azure.Data.Tables/MigrationGuide.md) migration guide
* [Azure.Storage.Blobs](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/storage/Azure.Storage.Blobs/AzureStorageNetMigrationV12.md) migration guide

This package provides a (mostly) drop-in replacement, with source code brought (and updated) 
from the [original location](https://github.com/Azure/azure-storage-net/blob/master/Lib/Common/CloudStorageAccount.cs).
Just replace the old namespace `Microsoft.Azure.Storage` with `Devlooped` and you're mostly done.

In addition to the legacy, backwards-compatible APIs so projects compile right away with this 
package when upgrading to v12 client libraries, there are a few newer APIs that are more aligned 
with the new APIs, such as:

* CloudStorageAccount.CreateBlobServiceClient (extension method)
* CloudStorageAccount.CreateQueueServiceClient (extension method)
* CloudStorageAccount.CreateTableServiceClient (extension method)

These make it more explicit that you're creating instances of the new service clients.

## Usage

```csharp
var account = CloudStorageAccount.DevelopmentStorageAccount;

var tableService = account.CreateTableServiceClient();
// legacy invocation works too: account.CreateCloudTableClient();

// Can also access the endpoints for each service:
Console.WriteLine(account.BlobEndpoint);
Console.WriteLine(account.QueueEndpoint);
Console.WriteLine(account.TableEndpoint);
```
<!-- #Overview -->

## Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/Devlooped.CloudStorageAccount/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)
[![Build](https://github.com/devlooped/CloudStorageAccount/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/CloudStorageAccount/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.io/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`


<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->