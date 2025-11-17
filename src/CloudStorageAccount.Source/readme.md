[![EULA](https://img.shields.io/badge/EULA-OSMF-blue?labelColor=black&color=C9FF30)](osmfeula.txt)
[![OSS](https://img.shields.io/github/license/devlooped/oss.svg?color=blue)](license.txt) 
[![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/devlooped/CloudStorageAccount)

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

<!-- include https://github.com/devlooped/.github/raw/main/osmf.md -->
<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
<!-- Excludes this readme from CI-based includes expansion -->
<!-- exclude -->
