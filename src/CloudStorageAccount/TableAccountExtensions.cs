using System;
using System.ComponentModel;
using Azure;
using Azure.Data.Tables;

namespace Devlooped;

/// <summary>
/// Provides the <see cref="CreateCloudTableClient(CloudStorageAccount)"/> extension method.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TableAccountExtensions
{
    /// <summary>
    /// Creates a Table service client from the given account.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static TableServiceClient CreateCloudTableClient(this CloudStorageAccount account)
        => CreateTableClient(account);

    /// <summary>
    /// Creates a Table service client from the given account.
    /// </summary>
    public static TableServiceClient CreateTableClient(this CloudStorageAccount account)
    {
        if (account.TableEndpoint == null)
            throw new InvalidOperationException("No table endpoint configured.");

        if (account.Credentials.IsSAS)
            return new TableServiceClient(account.TableEndpoint, new AzureSasCredential(account.Credentials.Signature!));
        else if (account.Credentials.IsSharedKey)
            return new TableServiceClient(account.TableEndpoint,
                new TableSharedKeyCredential(account.Credentials.AccountName, account.Credentials.AccountKey));
        else if (account.Credentials.IsAnonymous)
            return new TableServiceClient(account.TableEndpoint);

        throw new InvalidOperationException("Account credentials are not supported for Table client.");
    }
}