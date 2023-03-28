using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using AccountSetting = System.Collections.Generic.KeyValuePair<string, System.Func<string, bool>>;
using ConnectionStringFilter = System.Func<System.Collections.Generic.IDictionary<string, string>, System.Collections.Generic.IDictionary<string, string>?>;

namespace Devlooped;

/// <summary>
/// Represents a Microsoft Azure Storage account.
/// </summary>
partial class CloudStorageAccount
{
    /// <summary>
    /// The setting name for using the development storage.
    /// </summary>
    const string UseDevelopmentStorageSettingString = "UseDevelopmentStorage";

    /// <summary>
    /// The setting name for specifying a development storage proxy Uri.
    /// </summary>
    const string DevelopmentStorageProxyUriSettingString = "DevelopmentStorageProxyUri";

    /// <summary>
    /// The setting name for using the default storage endpoints with the specified protocol.
    /// </summary>
    const string DefaultEndpointsProtocolSettingString = "DefaultEndpointsProtocol";

    /// <summary>
    /// The setting name for the account name.
    /// </summary>
    const string AccountNameSettingString = "AccountName";

    /// <summary>
    /// The setting name for the account key name.
    /// </summary>
    const string AccountKeyNameSettingString = "AccountKeyName";

    /// <summary>
    /// The setting name for the account key.
    /// </summary>
    const string AccountKeySettingString = "AccountKey";

    /// <summary>
    /// The setting name for a custom blob storage endpoint.
    /// </summary>
    const string BlobEndpointSettingString = "BlobEndpoint";

    /// <summary>
    /// The setting name for a custom queue endpoint.
    /// </summary>
    const string QueueEndpointSettingString = "QueueEndpoint";

    /// <summary>
    /// The setting name for a custom table storage endpoint.
    /// </summary>
    const string TableEndpointSettingString = "TableEndpoint";

    /// <summary>
    /// The setting name for a custom file storage endpoint.
    /// </summary>
    const string FileEndpointSettingString = "FileEndpoint";

    /// <summary>
    /// The setting name for a custom storage endpoint suffix.
    /// </summary>
    const string EndpointSuffixSettingString = "EndpointSuffix";

    /// <summary>
    /// The setting name for a shared access key.
    /// </summary>
    const string SharedAccessSignatureSettingString = "SharedAccessSignature";

    /// <summary>
    /// The default account name for the development storage.
    /// </summary>
    const string DevstoreAccountName = "devstoreaccount1";

    /// <summary>
    /// The default account key for the development storage.
    /// </summary>
    const string DevstoreAccountKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

    /// <summary>
    /// The suffix appended to account in order to access secondary location for read only access.
    /// </summary>
    const string SecondaryLocationAccountSuffix = "-secondary";

    /// <summary>
    /// The default storage service hostname suffix.
    /// </summary>
    const string DefaultEndpointSuffix = "core.windows.net";

    /// <summary>
    /// The default blob storage DNS hostname prefix.
    /// </summary>
    const string DefaultBlobHostnamePrefix = "blob";

    /// <summary>
    /// The root queue DNS name prefix.
    /// </summary>
    const string DefaultQueueHostnamePrefix = "queue";

    /// <summary>
    /// The root table storage DNS name prefix.
    /// </summary>
    const string DefaultTableHostnamePrefix = "table";

    /// <summary>
    /// The default file storage DNS hostname prefix.
    /// </summary>
    const string DefaultFileHostnamePrefix = "file";

    /// <summary>
    /// Validator for the UseDevelopmentStorage setting. Must be "true".
    /// </summary>
    static readonly AccountSetting UseDevelopmentStorageSetting = Setting(UseDevelopmentStorageSettingString, "true");

    /// <summary>
    /// Validator for the DevelopmentStorageProxyUri setting. Must be a valid Uri.
    /// </summary>
    static readonly AccountSetting DevelopmentStorageProxyUriSetting = Setting(DevelopmentStorageProxyUriSettingString, IsValidUri);

    /// <summary>
    /// Validator for the DefaultEndpointsProtocol setting. Must be either "http" or "https".
    /// </summary>
    static readonly AccountSetting DefaultEndpointsProtocolSetting = Setting(DefaultEndpointsProtocolSettingString, "http", "https");

    /// <summary>
    /// Validator for the AccountName setting. No restrictions.
    /// </summary>
    static readonly AccountSetting AccountNameSetting = Setting(AccountNameSettingString);

    /// <summary>
    /// Validator for the AccountKey setting. No restrictions.
    /// </summary>
    static readonly AccountSetting AccountKeyNameSetting = Setting(AccountKeyNameSettingString);

    /// <summary>
    /// Validator for the AccountKey setting. Must be a valid base64 string.
    /// </summary>
    static readonly AccountSetting AccountKeySetting = Setting(AccountKeySettingString, IsValidBase64String);

    /// <summary>
    /// Validator for the BlobEndpoint setting. Must be a valid Uri.
    /// </summary>
    static readonly AccountSetting BlobEndpointSetting = Setting(BlobEndpointSettingString, IsValidUri);

    /// <summary>
    /// Validator for the QueueEndpoint setting. Must be a valid Uri.
    /// </summary>
    static readonly AccountSetting QueueEndpointSetting = Setting(QueueEndpointSettingString, IsValidUri);

    /// <summary>
    /// Validator for the TableEndpoint setting. Must be a valid Uri.
    /// </summary>
    static readonly AccountSetting TableEndpointSetting = Setting(TableEndpointSettingString, IsValidUri);

    /// <summary>
    /// Validator for the FileEndpoint setting. Must be a valid Uri.
    /// </summary>
    static readonly AccountSetting FileEndpointSetting = Setting(FileEndpointSettingString, IsValidUri);

    /// <summary>
    /// Validator for the EndpointSuffix setting. Must be a valid Uri.
    /// </summary>
    static readonly AccountSetting EndpointSuffixSetting = Setting(EndpointSuffixSettingString, IsValidDomain);

    /// <summary>
    /// Validator for the SharedAccessSignature setting. No restrictions.
    /// </summary>
    static readonly AccountSetting SharedAccessSignatureSetting = Setting(SharedAccessSignatureSettingString);

    /// <summary>
    /// Singleton instance for the development storage account.
    /// </summary>
    static CloudStorageAccount? devStoreAccount;

    /// <summary>
    /// Private record of the account name for use in ToString(bool).
    /// </summary>
    string? accountName;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudStorageAccount"/> class using the specified
    /// credentials and service endpoints.
    /// </summary>
    /// <param name="storageCredentials">A <see cref="StorageCredentials"/> object.</param>
    /// <param name="blobEndpoint">A <see cref="Uri"/> specifying the primary Blob service endpoint.</param>
    /// <param name="queueEndpoint">A <see cref="Uri"/> specifying the primary Queue service endpoint.</param>
    /// <param name="tableEndpoint">A <see cref="Uri"/> specifying the primary Table service endpoint.</param>
    /// <param name="fileEndpoint">A <see cref="Uri"/> specifying the primary File service endpoint.</param>
    public CloudStorageAccount(StorageCredentials storageCredentials, Uri? blobEndpoint, Uri? queueEndpoint, Uri? tableEndpoint, Uri? fileEndpoint)
    {
        Credentials = storageCredentials;
        BlobEndpoint = blobEndpoint;
        QueueEndpoint = queueEndpoint;
        TableEndpoint = tableEndpoint;
        FileEndpoint = fileEndpoint;
        DefaultEndpoints = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudStorageAccount"/> class using the specified
    /// credentials, and specifies whether to use HTTP or HTTPS to connect to the storage services. 
    /// </summary>
    /// <param name="storageCredentials">A <see cref="StorageCredentials"/> object.</param>
    /// <param name="useHttps"><c>true</c> to use HTTPS to connect to storage service endpoints; otherwise, <c>false</c>.</param>
    /// <remarks>Using HTTPS to connect to the storage services is recommended.</remarks>
    public CloudStorageAccount(StorageCredentials storageCredentials, bool useHttps = true)
        : this(storageCredentials, null /* endpointSuffix */, useHttps)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudStorageAccount"/> class using the specified
    /// credentials and endpoint suffix, and specifies whether to use HTTP or HTTPS to connect to the storage services.
    /// </summary>
    /// <param name="storageCredentials">A <see cref="StorageCredentials"/> object.</param>
    /// <param name="endpointSuffix">The DNS endpoint suffix for all storage services, e.g. "core.windows.net".</param>
    /// <param name="useHttps"><c>true</c> to use HTTPS to connect to storage service endpoints; otherwise, <c>false</c>.</param>
    /// <remarks>Using HTTPS to connect to the storage services is recommended.</remarks>
    public CloudStorageAccount(StorageCredentials storageCredentials, string? endpointSuffix, bool useHttps)
        : this(storageCredentials, storageCredentials.AccountName, endpointSuffix, useHttps)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudStorageAccount"/> class using the specified
    /// credentials and endpoint suffix, and specifies whether to use HTTP or HTTPS to connect to the storage services.
    /// </summary>
    /// <param name="storageCredentials">A <see cref="StorageCredentials"/> object.</param>
    /// <param name="accountName">The name of the account.</param>
    /// <param name="endpointSuffix">The DNS endpoint suffix for all storage services, e.g. "core.windows.net".</param>
    /// <param name="useHttps"><c>true</c> to use HTTPS to connect to storage service endpoints; otherwise, <c>false</c>.</param>
    /// <remarks>Using HTTPS to connect to the storage services is recommended.</remarks>
    public CloudStorageAccount(StorageCredentials storageCredentials, string? accountName, string? endpointSuffix, bool useHttps)
    {
        if (!string.IsNullOrEmpty(storageCredentials.AccountName))
        {
            if (string.IsNullOrEmpty(accountName))
            {
                accountName = storageCredentials.AccountName;
            }
            else
            {
                if (string.Compare(storageCredentials.AccountName, accountName, StringComparison.Ordinal) != 0)
                {
                    throw new ArgumentException($"Account names do not match.  First account name is {storageCredentials.AccountName}, second is {accountName}.");
                }
            }
        }

        if (accountName == null)
            throw new ArgumentNullException(nameof(accountName));

        var protocol = useHttps ? "https" : "http";

        BlobEndpoint = ConstructBlobEndpoint(protocol, accountName, endpointSuffix);
        QueueEndpoint = ConstructQueueEndpoint(protocol, accountName, endpointSuffix);
        TableEndpoint = ConstructTableEndpoint(protocol, accountName, endpointSuffix);
        FileEndpoint = ConstructFileEndpoint(protocol, accountName, endpointSuffix);
        Credentials = storageCredentials;
        EndpointSuffix = endpointSuffix;
        DefaultEndpoints = true;
    }

    /// <summary>
    /// Gets a <see cref="CloudStorageAccount"/> object that references the well-known development storage account.
    /// </summary>
    /// <value>A <see cref="CloudStorageAccount"/> object representing the development storage account.</value>
    public static CloudStorageAccount DevelopmentStorageAccount => devStoreAccount ??= GetDevelopmentStorageAccount(null);

    /// <summary>
    /// Indicates whether this account is a development storage account.
    /// </summary>
    bool IsDevStoreAccount { get; set; }

    /// <summary>
    /// The storage service hostname suffix set by the user, if any.
    /// </summary>
    string? EndpointSuffix { get; set; }

    /// <summary>
    /// The connection string parsed into settings.
    /// </summary>
    IDictionary<string, string>? Settings { get; set; }

    /// <summary>
    /// True if the user used a constructor that auto-generates endpoints.
    /// </summary>
    bool DefaultEndpoints { get; set; }

    /// <summary>
    /// Gets the primary endpoint for the Blob service, as configured for the storage account.
    /// </summary>
    /// <value>A <see cref="Uri"/> containing the primary Blob service endpoint.</value>
    public Uri? BlobEndpoint { get; }

    /// <summary>
    /// Gets the primary endpoint for the Queue service, as configured for the storage account.
    /// </summary>
    /// <value>A <see cref="Uri"/> containing the primary Queue service endpoint.</value>
    public Uri? QueueEndpoint { get; }

    /// <summary>
    /// Gets the primary endpoint for the Table service, as configured for the storage account.
    /// </summary>
    /// <value>A <see cref="Uri"/> containing the primary Table service endpoint.</value>
    public Uri? TableEndpoint { get; }

    /// <summary>
    /// Gets the primary endpoint for the File service, as configured for the storage account.
    /// </summary>
    /// <value>A <see cref="Uri"/> containing the primary File service endpoint.</value>
    public Uri? FileEndpoint { get; }

    /// <summary>
    /// Gets the credentials used to create this <see cref="CloudStorageAccount"/> object.
    /// </summary>
    /// <value>A <see cref="StorageCredentials"/> object.</value>
    public StorageCredentials Credentials { get; }

    /// <summary>
    /// Parses a connection string and returns a <see cref="CloudStorageAccount"/> created
    /// from the connection string.
    /// </summary>
    /// <param name="connectionString">A valid connection string.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionString"/> is null or empty.</exception>
    /// <exception cref="FormatException">Thrown if <paramref name="connectionString"/> is not a valid connection string.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="connectionString"/> cannot be parsed.</exception>
    /// <returns>A <see cref="CloudStorageAccount"/> object constructed from the values provided in the connection string.</returns>
    public static CloudStorageAccount Parse(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("connectionString");
        }

        if (ParseImpl(connectionString, out var ret, err => { throw new FormatException(err); }))
        {
            return ret;
        }

        throw new ArgumentException("Error parsing value");
    }

    /// <summary>
    /// Indicates whether a connection string can be parsed to return a <see cref="CloudStorageAccount"/> object.
    /// </summary>
    /// <param name="connectionString">The connection string to parse.</param>
    /// <param name="account">A <see cref="CloudStorageAccount"/> object to hold the instance returned if
    /// the connection string can be parsed.</param>
    /// <returns><b>true</b> if the connection string was successfully parsed; otherwise, <b>false</b>.</returns>
    public static bool TryParse(string? connectionString, [NotNullWhen(true)] out CloudStorageAccount? account)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            account = null;
            return false;
        }

        try
        {
            return ParseImpl(connectionString!, out account, err => { });
        }
        catch (Exception)
        {
            account = null;
            return false;
        }
    }

    /// <summary>
    /// Returns a connection string for this storage account, without sensitive data.
    /// </summary>
    /// <returns>A connection string.</returns>
    public override string ToString() => ToString(false);

    /// <summary>
    /// Returns a connection string for this storage account, optionally with sensitive data.
    /// </summary>
    /// <returns>A connection string.</returns>
    public string ToString(bool exportSecrets)
    {
        if (Settings == null)
        {
            Settings = new Dictionary<string, string>();

            if (DefaultEndpoints)
            {
                var scheme =
                    BlobEndpoint?.Scheme ??
                    QueueEndpoint?.Scheme ??
                    TableEndpoint?.Scheme ??
                    FileEndpoint?.Scheme;

                if (scheme != null)
                    Settings.Add(DefaultEndpointsProtocolSettingString, scheme);

                if (EndpointSuffix != null)
                {
                    Settings.Add(EndpointSuffixSettingString, EndpointSuffix);
                }
            }
            else
            {
                if (BlobEndpoint != null)
                {
                    Settings.Add(BlobEndpointSettingString, BlobEndpoint.ToString());
                }

                if (QueueEndpoint != null)
                {
                    Settings.Add(QueueEndpointSettingString, QueueEndpoint.ToString());
                }

                if (TableEndpoint != null)
                {
                    Settings.Add(TableEndpointSettingString, TableEndpoint.ToString());
                }

                if (FileEndpoint != null)
                {
                    Settings.Add(FileEndpointSettingString, FileEndpoint.ToString());
                }
            }
        }

        var listOfSettings = Settings.Select(pair => string.Format(CultureInfo.InvariantCulture, "{0}={1}", pair.Key, pair.Value)).ToList();

        if (Credentials != null && !IsDevStoreAccount && !Credentials.IsAnonymous)
        {
            listOfSettings.Add(Credentials.ToString(exportSecrets));
        }

        if (!string.IsNullOrWhiteSpace(accountName) && (Credentials != null ? string.IsNullOrWhiteSpace(Credentials.AccountName) : true))
        {
            listOfSettings.Add(string.Format(CultureInfo.InvariantCulture, "{0}={1}", AccountNameSettingString, accountName));
        }

        return string.Join(";", listOfSettings);
    }

    /// <summary>
    /// Returns a <see cref="CloudStorageAccount"/> with development storage credentials using the specified proxy Uri.
    /// </summary>
    /// <param name="proxyUri">The proxy endpoint to use.</param>
    /// <returns>The new <see cref="CloudStorageAccount"/>.</returns>
    static CloudStorageAccount GetDevelopmentStorageAccount(Uri? proxyUri)
    {
        var builder = proxyUri != null ?
            new UriBuilder(proxyUri.Scheme, proxyUri.Host) :
            new UriBuilder("http", "127.0.0.1");

        builder.Path = DevstoreAccountName;

        builder.Port = 10000;
        var blobEndpoint = builder.Uri;

        builder.Port = 10001;
        var queueEndpoint = builder.Uri;

        builder.Port = 10002;
        var tableEndpoint = builder.Uri;

        builder.Path = DevstoreAccountName + SecondaryLocationAccountSuffix;

        var credentials = new StorageCredentials(DevstoreAccountName, DevstoreAccountKey);
        var account = new CloudStorageAccount(
            credentials,
            blobEndpoint,
            queueEndpoint,
            tableEndpoint,
            null /* fileStorageUri */);

        account.Settings = new Dictionary<string, string>
        {
            { UseDevelopmentStorageSettingString, "true" }
        };

        if (proxyUri != null)
        {
            account.Settings.Add(DevelopmentStorageProxyUriSettingString, proxyUri.ToString());
        }

        account.IsDevStoreAccount = true;

        return account;
    }

    /// <summary>
    /// Internal implementation of Parse/TryParse.
    /// </summary>
    /// <param name="connectionString">The string to parse.</param>
    /// <param name="accountInformation">The <see cref="CloudStorageAccount"/> to return.</param>
    /// <param name="error">A callback for reporting errors.</param>
    /// <returns>If true, the parse was successful. Otherwise, false.</returns>
    internal static bool ParseImpl(string connectionString,
        [NotNullWhen(true)] out CloudStorageAccount? accountInformation, Action<string> error)
    {
        var settings = ParseStringIntoSettings(connectionString, error);

        // malformed settings string

        if (settings == null)
        {
            accountInformation = null;
            return false;
        }

        // helper method 

        string settingOrDefault(string key)
        {
            settings.TryGetValue(key, out var result);
            return result;
        }

        // devstore case

        if (MatchesSpecification(
            settings,
            AllRequired(UseDevelopmentStorageSetting),
            Optional(DevelopmentStorageProxyUriSetting)))
        {

            if (settings.TryGetValue(DevelopmentStorageProxyUriSettingString, out var proxyUri))
            {
                accountInformation = GetDevelopmentStorageAccount(new Uri(proxyUri));
            }
            else
            {
                accountInformation = DevelopmentStorageAccount;
            }

            accountInformation.Settings = ValidCredentials(settings);

            return true;
        }

        // non-devstore case

        var endpointsOptional =
            Optional(
                BlobEndpointSetting,
                QueueEndpointSetting,
                TableEndpointSetting,
                FileEndpointSetting
                );

        var primaryEndpointRequired =
            AtLeastOne(
                BlobEndpointSetting,
                QueueEndpointSetting,
                TableEndpointSetting,
                FileEndpointSetting
                );

        var automaticEndpointsMatchSpec =
            MatchesExactly(MatchesAll(
                MatchesOne(
                    MatchesAll(AllRequired(AccountKeySetting), Optional(AccountKeyNameSetting)), // Key + Name, Endpoints optional
                    AllRequired(SharedAccessSignatureSetting) // SAS + Name, Endpoints optional
                ),
                AllRequired(AccountNameSetting), // Name required to automatically create URIs
                endpointsOptional,
                Optional(DefaultEndpointsProtocolSetting, EndpointSuffixSetting)
                ));

        var explicitEndpointsMatchSpec =
            MatchesExactly(MatchesAll( // Any Credentials, Endpoints must be explicitly declared
                ValidCredentials,
                primaryEndpointRequired
                ));

        var matchesAutomaticEndpointsSpec = MatchesSpecification(settings, automaticEndpointsMatchSpec);
        var matchesExplicitEndpointsSpec = MatchesSpecification(settings, explicitEndpointsMatchSpec);

        if (matchesAutomaticEndpointsSpec || matchesExplicitEndpointsSpec)
        {
            if (matchesAutomaticEndpointsSpec && !settings.ContainsKey(DefaultEndpointsProtocolSettingString))
            {
                settings.Add(DefaultEndpointsProtocolSettingString, "https");
            }

            var blobEndpoint = settingOrDefault(BlobEndpointSettingString);
            var queueEndpoint = settingOrDefault(QueueEndpointSettingString);
            var tableEndpoint = settingOrDefault(TableEndpointSettingString);
            var fileEndpoint = settingOrDefault(FileEndpointSettingString);

            Func<string, Func<IDictionary<string, string>, Uri>, Uri?> createStorageUri =
                (primary, factory) =>
                    !string.IsNullOrWhiteSpace(primary)
                        ? new Uri(primary)
                    : matchesAutomaticEndpointsSpec && factory != null
                        ? factory(settings)
                    : null
                    ;

            accountInformation = new CloudStorageAccount(
                GetCredentials(settings) ?? new StorageCredentials(),
                createStorageUri(blobEndpoint, ConstructBlobEndpoint),
                createStorageUri(queueEndpoint, ConstructQueueEndpoint),
                createStorageUri(tableEndpoint, ConstructTableEndpoint),
                createStorageUri(fileEndpoint, ConstructFileEndpoint))
            {
                DefaultEndpoints = (blobEndpoint == null && queueEndpoint == null && tableEndpoint == null && fileEndpoint == null),
                EndpointSuffix = settingOrDefault(EndpointSuffixSettingString),
                Settings = ValidCredentials(settings)
            };

            accountInformation.accountName = settingOrDefault(AccountNameSettingString);

            return true;
        }

        // not valid
        accountInformation = null;

        error("No valid combination of account information found.");

        return false;
    }

    /// <summary>
    /// Tokenizes input and stores name value pairs.
    /// </summary>
    /// <param name="connectionString">The string to parse.</param>
    /// <param name="error">Error reporting delegate.</param>
    /// <returns>Tokenized collection.</returns>
    static IDictionary<string, string>? ParseStringIntoSettings(string connectionString, Action<string> error)
    {
        var settings = new Dictionary<string, string>();
        var splitted = connectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var nameValue in splitted)
        {
            var splittedNameValue = nameValue.Split(new char[] { '=' }, 2);

            if (splittedNameValue.Length != 2)
            {
                error("Settings must be of the form \"name=value\".");
                return null;
            }

            if (settings.ContainsKey(splittedNameValue[0]))
            {
                error(string.Format(CultureInfo.InvariantCulture, "Duplicate setting '{0}' found.", splittedNameValue[0]));
                return null;
            }

            settings.Add(splittedNameValue[0], splittedNameValue[1]);
        }

        return settings;
    }

    /// <summary>
    /// Encapsulates a validation rule for an enumeration based account setting.
    /// </summary>
    /// <param name="name">The name of the setting.</param>
    /// <param name="validValues">A list of valid values for the setting.</param>
    /// <returns>An <see cref="AccountSetting"/> representing the enumeration constraint.</returns>
    static AccountSetting Setting(string name, params string[] validValues) => new AccountSetting(
            name,
            (settingValue) =>
            {
                if (validValues.Length == 0)
                {
                    return true;
                }

                return validValues.Contains(settingValue);
            });

    /// <summary>
    /// Encapsulates a validation rule using a func.
    /// </summary>
    /// <param name="name">The name of the setting.</param>
    /// <param name="isValid">A func that determines if the value is valid.</param>
    /// <returns>An <see cref="AccountSetting"/> representing the constraint.</returns>
    static AccountSetting Setting(string name, Func<string, bool> isValid) => new AccountSetting(name, isValid);

    /// <summary>
    /// Determines whether the specified setting value is a valid base64 string.
    /// </summary>
    /// <param name="settingValue">The setting value.</param>
    /// <returns><c>true</c> if the specified setting value is a valid base64 string; otherwise, <c>false</c>.</returns>
    static bool IsValidBase64String(string settingValue)
    {
        try
        {
            Convert.FromBase64String(settingValue);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Validation function that validates Uris.
    /// </summary>
    /// <param name="settingValue">Value to validate.</param>
    /// <returns><c>true</c> if the specified setting value is a valid Uri; otherwise, <c>false</c>.</returns>
    static bool IsValidUri(string settingValue) => Uri.IsWellFormedUriString(settingValue, UriKind.Absolute);

    /// <summary>
    /// Validation function that validates a domain name.
    /// </summary>
    /// <param name="settingValue">Value to validate.</param>
    /// <returns><c>true</c> if the specified setting value is a valid domain; otherwise, <c>false</c>.</returns>
    static bool IsValidDomain(string settingValue) => Uri.CheckHostName(settingValue).Equals(UriHostNameType.Dns);

    /// <summary>
    /// Settings filter that requires all specified settings be present and valid.
    /// </summary>
    /// <param name="requiredSettings">A list of settings that must be present.</param>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter AllRequired(params AccountSetting[] requiredSettings)
    {
        return (settings) =>
        {
            IDictionary<string, string> result = new Dictionary<string, string>(settings);

            foreach (var requirement in requiredSettings)
            {
                if (result.TryGetValue(requirement.Key, out var value) && requirement.Value(value))
                {
                    result.Remove(requirement.Key);
                }
                else
                {
                    return null;
                }
            }

            return result;
        };
    }

    /// <summary>
    /// Settings filter that removes optional values.
    /// </summary>
    /// <param name="optionalSettings">A list of settings that are optional.</param>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter Optional(params AccountSetting[] optionalSettings)
    {
        return (settings) =>
        {
            IDictionary<string, string> result = new Dictionary<string, string>(settings);

            foreach (var requirement in optionalSettings)
            {
                if (result.TryGetValue(requirement.Key, out var value) && requirement.Value(value))
                {
                    result.Remove(requirement.Key);
                }
            }

            return result;
        };
    }

    /// <summary>
    /// Settings filter that ensures that at least one setting is present.
    /// </summary>
    /// <param name="atLeastOneSettings">A list of settings of which one must be present.</param>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter AtLeastOne(params AccountSetting[] atLeastOneSettings)
    {
        return (settings) =>
        {
            IDictionary<string, string> result = new Dictionary<string, string>(settings);
            var foundOne = false;

            foreach (var requirement in atLeastOneSettings)
            {
                if (result.TryGetValue(requirement.Key, out var value) && requirement.Value(value))
                {
                    result.Remove(requirement.Key);
                    foundOne = true;
                }
            }

            return foundOne ? result : null;
        };
    }

    /// <summary>
    /// Settings filter that ensures that none of the specified settings are present.
    /// </summary>
    /// <param name="atLeastOneSettings">A list of settings of which one must not be present.</param>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter None(params AccountSetting[] atLeastOneSettings)
    {
        return (settings) =>
        {
            IDictionary<string, string> result = new Dictionary<string, string>(settings);
            var foundOne = false;

            foreach (var requirement in atLeastOneSettings)
            {
                if (result.TryGetValue(requirement.Key, out var value) && requirement.Value(value))
                {
                    foundOne = true;
                }
            }

            return foundOne ? null : result;
        };
    }

    /// <summary>
    /// Settings filter that ensures that all of the specified filters match.
    /// </summary>
    /// <param name="filters">A list of filters of which all must match.</param>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter MatchesAll(params ConnectionStringFilter[] filters)
    {
        return (settings) =>
        {
            IDictionary<string, string>? result = new Dictionary<string, string>(settings);

            foreach (var filter in filters)
            {
                if (result == null)
                {
                    break;
                }

                result = filter(result);
            }

            return result;
        };
    }

    /// <summary>
    /// Settings filter that ensures that exactly one filter matches.
    /// </summary>
    /// <param name="filters">A list of filters of which exactly one must match.</param>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter MatchesOne(params ConnectionStringFilter[] filters)
    {
        return (settings) =>
        {
            var results =
                filters
                .Select(filter => filter(new Dictionary<string, string>(settings)))
                .Where(result => result != null)
                .Take(2)
                .ToArray();

            if (results.Length != 1)
            {
                return null;
            }
            else
            {
                return results.First();
            }
        };
    }

    /// <summary>
    /// Settings filter that ensures that the specified filter is an exact match.
    /// </summary>
    /// <param name="filter">A list of filters of which ensures that the specified filter is an exact match.</param>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter MatchesExactly(ConnectionStringFilter filter)
    {
        return (settings) =>
        {
            var results = filter(settings);

            if (results == null || results.Any())
            {
                return null;
            }
            else
            {
                return results;
            }
        };
    }

    /// <summary>
    /// Settings filter that ensures that a valid combination of credentials is present.
    /// </summary>
    /// <returns>The remaining settings or <c>null</c> if the filter's requirement is not satisfied.</returns>
    static ConnectionStringFilter ValidCredentials =
        MatchesOne(
            MatchesAll(AllRequired(AccountNameSetting, AccountKeySetting), None(SharedAccessSignatureSetting)),              // AccountAndKey
            MatchesAll(AllRequired(SharedAccessSignatureSetting), Optional(AccountNameSetting), None(AccountKeySetting)),    // SharedAccessSignature (AccountName optional)
            None(AccountNameSetting, AccountKeySetting, SharedAccessSignatureSetting)                                        // Anonymous
        );

    /// <summary>
    /// Tests to see if a given list of settings matches a set of filters exactly.
    /// </summary>
    /// <param name="settings">The settings to check.</param>
    /// <param name="constraints">A list of filters to check.</param>
    /// <returns>
    /// If any filter returns null, false.
    /// If there are any settings left over after all filters are processed, false.
    /// Otherwise true.
    /// </returns>
    static bool MatchesSpecification(
        IDictionary<string, string> settings,
        params ConnectionStringFilter[] constraints)
    {
        foreach (var constraint in constraints)
        {
            var remainingSettings = constraint(settings);

            if (remainingSettings == null)
            {
                return false;
            }
            else
            {
                settings = remainingSettings;
            }
        }

        if (settings.Count == 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets a StorageCredentials object corresponding to whatever credentials are supplied in the given settings.
    /// </summary>
    /// <param name="settings">The settings to check.</param>
    /// <returns>The StorageCredentials object specified in the settings.</returns>
    static StorageCredentials? GetCredentials(IDictionary<string, string> settings)
    {
        settings.TryGetValue(AccountNameSettingString, out var accountName);
        settings.TryGetValue(AccountKeySettingString, out var accountKey);
        settings.TryGetValue(SharedAccessSignatureSettingString, out var sharedAccessSignature);

        if (accountName != null && accountKey != null && sharedAccessSignature == null)
        {
            return new StorageCredentials(accountName, accountKey);
        }

        if (accountKey == null && accountKey == null && sharedAccessSignature != null)
        {
            return new StorageCredentials(sharedAccessSignature);
        }

        return null;
    }

    /// <summary>
    /// Gets the default blob endpoint using specified settings.
    /// </summary>
    /// <param name="settings">The settings to use.</param>
    /// <returns>The default blob endpoint.</returns>
    static Uri ConstructBlobEndpoint(IDictionary<string, string> settings) => ConstructBlobEndpoint(
            settings[DefaultEndpointsProtocolSettingString],
            settings[AccountNameSettingString],
            settings.ContainsKey(EndpointSuffixSettingString) ? settings[EndpointSuffixSettingString] : null);

    /// <summary>
    /// Gets the default blob endpoint using the specified protocol and account name.
    /// </summary>
    /// <param name="scheme">The protocol to use.</param>
    /// <param name="accountName">The name of the storage account.</param>
    /// <param name="endpointSuffix">The Endpoint DNS suffix; use <c>null</c> for default.</param>
    /// <returns>The default blob endpoint.</returns>
    static Uri ConstructBlobEndpoint(string scheme, string accountName, string? endpointSuffix)
    {
        if (string.IsNullOrEmpty(scheme))
        {
            throw new ArgumentNullException("scheme");
        }

        if (string.IsNullOrEmpty(accountName))
        {
            throw new ArgumentNullException("accountName");
        }

        if (string.IsNullOrEmpty(endpointSuffix))
        {
            endpointSuffix = DefaultEndpointSuffix;
        }

        var primaryUri = string.Format(
            CultureInfo.InvariantCulture,
            "{0}://{1}.{2}.{3}/",
            scheme,
            accountName,
            DefaultBlobHostnamePrefix,
            endpointSuffix);

        return new Uri(primaryUri);
    }

    /// <summary>
    /// Gets the default file endpoint using specified settings.
    /// </summary>
    /// <param name="settings">The settings to use.</param>
    /// <returns>The default file endpoint.</returns>
    static Uri ConstructFileEndpoint(IDictionary<string, string> settings) => ConstructFileEndpoint(
            settings[DefaultEndpointsProtocolSettingString],
            settings[AccountNameSettingString],
            settings.ContainsKey(EndpointSuffixSettingString) ? settings[EndpointSuffixSettingString] : null);

    /// <summary>
    /// Gets the default file endpoint using the specified protocol and account name.
    /// </summary>
    /// <param name="scheme">The protocol to use.</param>
    /// <param name="accountName">The name of the storage account.</param>
    /// <param name="endpointSuffix">The Endpoint DNS suffix; use <c>null</c> for default.</param>
    /// <returns>The default file endpoint.</returns>
    static Uri ConstructFileEndpoint(string scheme, string accountName, string? endpointSuffix)
    {
        if (string.IsNullOrEmpty(scheme))
        {
            throw new ArgumentNullException("scheme");
        }

        if (string.IsNullOrEmpty(accountName))
        {
            throw new ArgumentNullException("accountName");
        }

        if (string.IsNullOrEmpty(endpointSuffix))
        {
            endpointSuffix = DefaultEndpointSuffix;
        }

        var primaryUri = string.Format(
            CultureInfo.InvariantCulture,
            "{0}://{1}.{2}.{3}/",
            scheme,
            accountName,
            DefaultFileHostnamePrefix,
            endpointSuffix);

        return new Uri(primaryUri);
    }

    /// <summary>
    /// Gets the default queue endpoint using the specified settings.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The default queue endpoint.</returns>
    static Uri ConstructQueueEndpoint(IDictionary<string, string> settings) => ConstructQueueEndpoint(
            settings[DefaultEndpointsProtocolSettingString],
            settings[AccountNameSettingString],
            settings.ContainsKey(EndpointSuffixSettingString) ? settings[EndpointSuffixSettingString] : null);

    /// <summary>
    /// Gets the default queue endpoint using the specified protocol and account name.
    /// </summary>
    /// <param name="scheme">The protocol to use.</param>
    /// <param name="accountName">The name of the storage account.</param>
    /// <param name="endpointSuffix">The Endpoint DNS suffix; use <c>null</c> for default.</param>
    /// <returns>The default queue endpoint.</returns>
    static Uri ConstructQueueEndpoint(string scheme, string accountName, string? endpointSuffix)
    {
        if (string.IsNullOrEmpty(scheme))
        {
            throw new ArgumentNullException("scheme");
        }

        if (string.IsNullOrEmpty(accountName))
        {
            throw new ArgumentNullException("accountName");
        }

        if (string.IsNullOrEmpty(endpointSuffix))
        {
            endpointSuffix = DefaultEndpointSuffix;
        }

        var primaryUri = string.Format(
            CultureInfo.InvariantCulture,
            "{0}://{1}.{2}.{3}/",
            scheme,
            accountName,
            DefaultQueueHostnamePrefix,
            endpointSuffix);

        return new Uri(primaryUri);
    }

    /// <summary>
    /// Gets the default table endpoint using the specified settings.
    /// </summary>
    /// <param name="settings">The settings.</param>
    /// <returns>The default table endpoint.</returns>
    static Uri ConstructTableEndpoint(IDictionary<string, string> settings) => ConstructTableEndpoint(
            settings[DefaultEndpointsProtocolSettingString],
            settings[AccountNameSettingString],
            settings.ContainsKey(EndpointSuffixSettingString) ? settings[EndpointSuffixSettingString] : null);

    /// <summary>
    /// Gets the default table endpoint using the specified protocol and account name.
    /// </summary>
    /// <param name="scheme">The protocol to use.</param>
    /// <param name="accountName">The name of the storage account.</param>
    /// <param name="endpointSuffix">The Endpoint DNS suffix; use <c>null</c> for default.</param>
    /// <returns>The default table endpoint.</returns>
    static Uri ConstructTableEndpoint(string scheme, string accountName, string? endpointSuffix)
    {
        if (string.IsNullOrEmpty(scheme))
        {
            throw new ArgumentNullException("scheme");
        }

        if (string.IsNullOrEmpty(accountName))
        {
            throw new ArgumentNullException("accountName");
        }

        if (string.IsNullOrEmpty(endpointSuffix))
        {
            endpointSuffix = DefaultEndpointSuffix;
        }

        var primaryUri = string.Format(
            CultureInfo.InvariantCulture,
            "{0}://{1}.{2}.{3}/",
            scheme,
            accountName,
            DefaultTableHostnamePrefix,
            endpointSuffix);

        return new Uri(primaryUri);
    }
}