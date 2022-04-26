using System;
using System.ComponentModel;
using Azure.Storage;
using Azure.Storage.Blobs;

namespace Devlooped;

/// <summary>
/// Provides the <see cref="CreateCloudBlobClient(CloudStorageAccount)"/> extension method.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
static partial class BlobAccountExtensions
{
    /// <summary>
    /// Creates a Blob service client from the given account.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static BlobServiceClient CreateCloudBlobClient(this CloudStorageAccount account)
        => CreateBlobServiceClient(account);

    /// <summary>
    /// Creates a Blob service client from the given account.
    /// </summary>
    public static BlobServiceClient CreateBlobServiceClient(this CloudStorageAccount account)
    {
        if (account.BlobEndpoint == null)
            throw new InvalidOperationException("No blob endpoint configured.");

        if (account.Credentials.TokenCredential != null)
            return new BlobServiceClient(account.BlobEndpoint, account.Credentials.TokenCredential);
        else if (account.Credentials.IsSharedKey)
            return new BlobServiceClient(account.BlobEndpoint,
                new StorageSharedKeyCredential(account.Credentials.AccountName, account.Credentials.AccountKey));
        else if (account.Credentials.IsAnonymous)
            return new BlobServiceClient(account.BlobEndpoint);

        throw new InvalidOperationException("Account credentials are not supported for Blob client.");
    }
}
