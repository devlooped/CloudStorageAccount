<!-- include ../../readme.md#Overview -->

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

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->

<!-- Excludes this readme from CI-based includes expansion -->
<!-- exclude -->
