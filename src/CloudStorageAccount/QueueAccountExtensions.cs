using System;
using System.ComponentModel;
using Azure.Storage;
using Azure.Storage.Queues;

namespace Devlooped;

/// <summary>
/// Provides the <see cref="CreateCloudQueueClient(CloudStorageAccount)"/> extension method.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class QueueAccountExtensions
{
    /// <summary>
    /// Creates a Queue service client from the given account.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static QueueServiceClient CreateCloudQueueClient(this CloudStorageAccount account)
        => CreateQueueClient(account);

    /// <summary>
    /// Creates a Queue service client from the given account.
    /// </summary>
    public static QueueServiceClient CreateQueueClient(this CloudStorageAccount account)
    {
        if (account.QueueEndpoint == null)
            throw new InvalidOperationException("No queue endpoint configured.");

        if (account.Credentials.TokenCredential != null)
            return new QueueServiceClient(account.QueueEndpoint, account.Credentials.TokenCredential);
        else if (account.Credentials.IsSharedKey)
            return new QueueServiceClient(account.QueueEndpoint,
                new StorageSharedKeyCredential(account.Credentials.AccountName, account.Credentials.AccountKey));
        else if (account.Credentials.IsAnonymous)
            return new QueueServiceClient(account.QueueEndpoint);

        throw new InvalidOperationException("Account credentials are not supported for Queue client.");
    }
}
