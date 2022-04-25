﻿using System;
using System.ComponentModel;
using Azure.Core;

namespace Devlooped;

/// <summary>
/// Represents a set of credentials used to authenticate access 
/// to a Microsoft Azure storage account.
/// </summary>
public class StorageCredentials
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageCredentials"/> with 
    /// no credentials information (i.e. for anonymous access).
    /// </summary>
    public StorageCredentials() => IsAnonymous = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageCredentials"/> class with the 
    /// specified account name and key value.
    /// </summary>
    /// <param name="accountName">The name of the Storage Account.</param>
    /// <param name="accountKey">A Storage Account access key.</param>
    public StorageCredentials(string accountName, string accountKey)
    {
        AccountName = accountName;
        AccountKey = accountKey;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageCredentials"/> class with the
    /// specified shared access signature token.
    /// </summary>
    /// <param name="signature">A string representing the shared access signature token.</param>
    public StorageCredentials(string signature) => Signature = signature;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageCredentials"/> class with the
    /// specified token credentials. Will typically be an instance of <c>DefaultAzureCredentials</c>.
    /// </summary>
    public StorageCredentials(TokenCredential tokenCredential) => TokenCredential = tokenCredential;

    /// <summary>
    /// Gets a value indicating whether the credentials are for anonymous access.
    /// </summary>
    public bool IsAnonymous { get; } = false;

    /// <summary>
    /// Gets a value indicating whether the credentials are a shared access signature token.
    /// </summary>
    public bool IsSAS => Signature != null;

    /// <summary>
    /// Gets a value indicating whether the credentials are a shared key.
    /// </summary>
    public bool IsSharedKey => Signature == null && AccountName != null;

    /// <summary>
    /// Gets the associated account name for the credentials if <see cref="IsSharedKey"/> 
    /// is <see langword="true"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? AccountName { get; }

    /// <summary>
    /// Gets the associated key for the credentials if <see cref="IsSharedKey"/>
    /// is <see langword="true"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string? Key => AccountKey;

    internal string? AccountKey { get; private set; }

    internal string? Signature { get; private set; }

    internal TokenCredential? TokenCredential { get; }

    /// <summary>
    /// Update the Storage Account's access key when using a shared access key. 
    /// This intended to be used when you've regenerated your Storage Account's 
    /// access keys and want to update long lived clients.
    /// </summary>
    /// <param name="accountKey">A Storage Account access key.</param>
    /// <exception cref="InvalidOperationException">Thrown if the credentials were not created as shared access key credentials initially.</exception>
    public void SetAccountKey(string accountKey) => AccountKey = IsSharedKey ? accountKey
        : throw new InvalidOperationException("Cannot update account key unless shared access key credentials are used.");

    /// <summary>
    /// Updates the shared access signature. This is intended to be used when you've
    /// regenerated your shared access signature and want to update long lived clients.
    /// </summary>
    /// <param name="signature">Shared access signature to authenticate the service against.</param>
    /// <exception cref="ArgumentNullException">Thrown when the signature is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the signature is empty.</exception>
    /// <exception cref="InvalidOperationException">Throws when the credentials are not </exception>
    /// <exception cref="InvalidOperationException">Thrown if the credentials were not created as shared access signature credentials initially.</exception>
    public void Update(string signature) => Signature = IsSAS ? signature
        : throw new InvalidOperationException("Cannot update key unless shared access signature credentials are used.");

    /// <summary>
    /// Updates the shared access signature (SAS) token value for storage credentials created with a shared access signature.
    /// </summary>
    /// <param name="sasToken">A string that specifies the SAS token value to update.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void UpdateSASToken(string sasToken) => Update(sasToken);

    /// <summary>
    /// Update the Storage Account's access key. This intended to be used when you've
    /// regenerated your Storage Account's access keys and want to update long lived
    /// clients.
    /// </summary>
    /// <param name="accountKey">A Storage Account access key.</param>
    /// <exception cref="InvalidOperationException">Thrown if the credentials were not created as shared access key credentials initially.</exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void UpdateKey(string accountKey) => SetAccountKey(accountKey);

    internal string ToString(bool exportSecrets)
    {
        if (IsSharedKey)
        {
            return $"{nameof(AccountName)}={AccountName};{nameof(AccountKey)}={(exportSecrets ? AccountKey : "[key hidden]")}";
        }

        if (IsSAS)
        {
            return $"SharedAccessSignature={(exportSecrets ? Signature : "[signature hidden]")}";
        }

        return string.Empty;
    }
}