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

## Visibily

This source-only package provides all types as partial and without an explicit 
visibility. This allows you to decide whether you want to make the types a 
public part of your project's API surface or not. Should you decide to make 
types public, you can use the approach used to compile the binary version 
[as a template](https://github.com/devlooped/CloudStorageAccount/blob/main/src/CloudStorageAccount/Visibility.cs):

```csharp
namespace Devlooped;

public partial class BlobAccountExtensions { }

public partial class CloudStorageAccount { }

public partial class QueueAccountExtensions { }

public partial class StorageCredentials { }

public partial class TableAccountExtensions { }
```