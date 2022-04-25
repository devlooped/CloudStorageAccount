using Xunit;

namespace Devlooped;

public class CloudStorageAccountTests
{
    const string AccountName = "foo";
    readonly string AccountKey = Convert.ToBase64String(new byte[] { 0, 1, 2 });

    [Fact]
    public void StorageCredentialsAnonymous()
    {
        var cred = new StorageCredentials();

        Assert.Null(cred.AccountName);
        Assert.Null(cred.AccountKey);
        Assert.Null(cred.Signature);

        Assert.True(cred.IsAnonymous);

        Assert.Throws<InvalidOperationException>(() => cred.UpdateKey("foo"));
        Assert.Throws<InvalidOperationException>(() => cred.UpdateSASToken("foo"));
    }

    [Fact]
    public void StorageCredentialsSharedKey()
    {
        var cred = new StorageCredentials(AccountName, AccountKey);

        Assert.Equal(AccountName, cred.AccountName);
        Assert.Equal(AccountKey, cred.AccountKey);

        Assert.False(cred.IsAnonymous);
        Assert.False(cred.IsSAS);
        Assert.True(cred.IsSharedKey);

        var key = Convert.ToBase64String(new byte[] { 2, 1, 0 });
        cred.UpdateKey(key);
        Assert.Equal(key, cred.AccountKey);

        Assert.Throws<InvalidOperationException>(() => cred.UpdateSASToken("foo"));
    }

    [Fact]
    public void StorageCredentialsSAS()
    {
        var key = Convert.ToBase64String(new byte[] { 0, 1, 2 });
        var cred = new StorageCredentials(key);

        Assert.Equal(key, cred.Signature);

        Assert.False(cred.IsAnonymous);
        Assert.False(cred.IsSharedKey);
        Assert.True(cred.IsSAS);

        var key2 = Convert.ToBase64String(new byte[] { 2, 1, 0 });
        cred.UpdateSASToken(key2);
        Assert.Equal(key2, cred.Signature);

        Assert.Throws<InvalidOperationException>(() => cred.UpdateKey("baz"));
    }

    [Fact]
    public void StorageCredentialsEmptyKeyValue()
    {
        var accountName = "foo";
        var keyValue = "bar";
        var emptyKeyValueAsString = string.Empty;
        var emptyKeyConnectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey=";

        var credentials1 = new StorageCredentials(accountName, emptyKeyValueAsString);
        Assert.Equal(accountName, credentials1.AccountName);
        Assert.False(credentials1.IsAnonymous);
        Assert.False(credentials1.IsSAS);
        Assert.True(credentials1.IsSharedKey);
        Assert.Equal(emptyKeyValueAsString, credentials1.AccountKey);

        var account2 = CloudStorageAccount.Parse(emptyKeyConnectionString);
        Assert.NotNull(account2?.Credentials);
        Assert.Equal(accountName, account2?.Credentials.AccountName);
        Assert.False(account2?.Credentials.IsAnonymous);
        Assert.False(account2?.Credentials.IsSAS);
        Assert.True(account2?.Credentials.IsSharedKey);
        Assert.Equal(emptyKeyValueAsString, account2?.Credentials.AccountKey);

        var isValidAccount3 = CloudStorageAccount.TryParse(emptyKeyConnectionString, out var account3);
        Assert.True(isValidAccount3);
        Assert.NotNull(account3);
        Assert.NotNull(account3?.Credentials);
        Assert.Equal(accountName, account3?.Credentials.AccountName);
        Assert.False(account3?.Credentials.IsAnonymous);
        Assert.False(account3?.Credentials.IsSAS);
        Assert.True(account3?.Credentials.IsSharedKey);
        Assert.Equal(emptyKeyValueAsString, account3?.Credentials.AccountKey);

        var credentials2 = new StorageCredentials(accountName, keyValue);
        Assert.Equal(accountName, credentials2.AccountName);
        Assert.False(credentials2.IsAnonymous);
        Assert.False(credentials2.IsSAS);
        Assert.True(credentials2.IsSharedKey);
        Assert.Equal(keyValue, credentials2.AccountKey);

        credentials2.UpdateKey(emptyKeyValueAsString);
        Assert.Equal(emptyKeyValueAsString, credentials2.AccountKey);
    }

    [Fact]
    public void CloudStorageAccountDevelopmentStorageAccount()
    {
        var devstoreAccount = CloudStorageAccount.DevelopmentStorageAccount;
        Assert.Equal(devstoreAccount.BlobEndpoint, new Uri("http://127.0.0.1:10000/devstoreaccount1"));
        Assert.Equal(devstoreAccount.QueueEndpoint, new Uri("http://127.0.0.1:10001/devstoreaccount1"));
        Assert.Equal(devstoreAccount.TableEndpoint, new Uri("http://127.0.0.1:10002/devstoreaccount1"));
        Assert.Null(devstoreAccount.FileEndpoint);

        var devstoreAccountToStringWithSecrets = devstoreAccount.ToString();
        var testAccount = CloudStorageAccount.Parse(devstoreAccountToStringWithSecrets);

        AccountsAreEqual(testAccount!, devstoreAccount);
        CloudStorageAccount? acct;
        if (!CloudStorageAccount.TryParse(devstoreAccountToStringWithSecrets, out acct))
        {
            Assert.True(false, "Expected TryParse success.");
        }
    }

    static void AccountsAreEqual(CloudStorageAccount a, CloudStorageAccount b)
    {
        // endpoints are the same
        Assert.Equal(a.BlobEndpoint, b.BlobEndpoint);
        Assert.Equal(a.QueueEndpoint, b.QueueEndpoint);
        Assert.Equal(a.TableEndpoint, b.TableEndpoint);
        Assert.Equal(a.FileEndpoint, b.FileEndpoint);

        // seralized representatons are the same.
        var aToStringNoSecrets = a.ToString();
        var bToStringNoSecrets = b.ToString();
        Assert.Equal(aToStringNoSecrets, bToStringNoSecrets, false);

        // credentials are the same
        if (a.Credentials != null && b.Credentials != null)
        {
            Assert.Equal(a.Credentials.IsAnonymous, b.Credentials.IsAnonymous);
            Assert.Equal(a.Credentials.IsSAS, b.Credentials.IsSAS);
            Assert.Equal(a.Credentials.IsSharedKey, b.Credentials.IsSharedKey);
            Assert.Equal(a.Credentials.TokenCredential, b.Credentials.TokenCredential);
        }
        else if (a.Credentials == null && b.Credentials == null)
        {
            return;
        }
        else
        {
            Assert.False(true, "credentials mismatch");
        }
    }

    [Fact]
    public void CloudStorageAccountDefaultStorageAccountWithHttp()
    {
        var cred = new StorageCredentials(AccountName, AccountKey);
        var cloudStorageAccount = new CloudStorageAccount(cred, false);
        Assert.Equal(cloudStorageAccount.BlobEndpoint, new Uri($"http://{AccountName}.blob.core.windows.net"));
        Assert.Equal(cloudStorageAccount.QueueEndpoint, new Uri($"http://{AccountName}.queue.core.windows.net"));
        Assert.Equal(cloudStorageAccount.TableEndpoint, new Uri($"http://{AccountName}.table.core.windows.net"));
        Assert.Equal(cloudStorageAccount.FileEndpoint, new Uri($"http://{AccountName}.file.core.windows.net"));
    }

    [Fact]
    public void CloudStorageAccountDefaultStorageAccountWithHttps()
    {
        var cred = new StorageCredentials(AccountName, AccountKey);
        var cloudStorageAccount = new CloudStorageAccount(cred, true);
        Assert.Equal(cloudStorageAccount.BlobEndpoint, new Uri($"https://{AccountName}.blob.core.windows.net"));
        Assert.Equal(cloudStorageAccount.QueueEndpoint, new Uri($"https://{AccountName}.queue.core.windows.net"));
        Assert.Equal(cloudStorageAccount.TableEndpoint, new Uri($"https://{AccountName}.table.core.windows.net"));
        Assert.Equal(cloudStorageAccount.FileEndpoint, new Uri($"https://{AccountName}.file.core.windows.net"));
    }

    [Fact]
    public void CloudStorageAccountEndpointSuffixWithHttp()
    {
        const string TestEndpointSuffix = "fake.endpoint.suffix";

        var cloudStorageAccount = CloudStorageAccount.Parse(
            $"DefaultEndpointsProtocol=http;AccountName={AccountName};AccountKey={AccountKey};EndpointSuffix={TestEndpointSuffix};");

        Assert.Equal(cloudStorageAccount.BlobEndpoint, new Uri($"http://{AccountName}.blob.{TestEndpointSuffix}"));
        Assert.Equal(cloudStorageAccount.QueueEndpoint, new Uri($"http://{AccountName}.queue.{TestEndpointSuffix}"));
        Assert.Equal(cloudStorageAccount.TableEndpoint, new Uri($"http://{AccountName}.table.{TestEndpointSuffix}"));
        Assert.Equal(cloudStorageAccount.FileEndpoint, new Uri($"http://{AccountName}.file.{TestEndpointSuffix}"));
    }

    [Fact]
    public void CloudStorageAccountEndpointSuffixWithHttps()
    {
        var key = Convert.ToBase64String(new byte[] { 0, 1, 2 });
        const string TestEndpointSuffix = "fake.endpoint.suffix";

        var cloudStorageAccount = CloudStorageAccount.Parse(
            $"DefaultEndpointsProtocol=https;AccountName={AccountName};AccountKey={AccountKey};EndpointSuffix={TestEndpointSuffix};");

        Assert.Equal(cloudStorageAccount.BlobEndpoint, new Uri($"https://{AccountName}.blob.{TestEndpointSuffix}"));
        Assert.Equal(cloudStorageAccount.QueueEndpoint, new Uri($"https://{AccountName}.queue.{TestEndpointSuffix}"));
        Assert.Equal(cloudStorageAccount.TableEndpoint, new Uri($"https://{AccountName}.table.{TestEndpointSuffix}"));
        Assert.Equal(cloudStorageAccount.FileEndpoint, new Uri($"https://{AccountName}.file.{TestEndpointSuffix}"));
    }

    [Fact]
    public void CloudStorageAccountEndpointSuffixWithBlob()
    {
        const string TestEndpointSuffix = "fake.endpoint.suffix";
        const string AlternateBlobEndpoint = "http://blob.other.endpoint/";

        var testAccount = CloudStorageAccount.Parse(
                $"DefaultEndpointsProtocol=http;AccountName={AccountName};AccountKey={AccountKey};EndpointSuffix={TestEndpointSuffix};BlobEndpoint={AlternateBlobEndpoint}");

        var cloudStorageAccount = CloudStorageAccount.Parse(testAccount.ToString(true));

        // make sure it round trips
        AccountsAreEqual(testAccount, cloudStorageAccount);
    }

    [Fact]
    public void CloudStorageAccountConnectionStringRoundtrip()
    {
        var accountKeyParams = new[]
            {
                    AccountName,
                    AccountKey,
                    "fake.endpoint.suffix",
                    "https://primary.endpoint/",
                };

        var accountSasParams = new[]
            {
                    AccountName,
                    "sasTest",
                    "fake.endpoint.suffix",
                    "https://primary.endpoint/",
                };

        // account key

        var accountString1 =
            string.Format(
                "DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1};EndpointSuffix={2};",
                accountKeyParams);

        var accountString2 =
            string.Format(
                "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};",
                accountKeyParams);

        var accountString3 =
            string.Format(
                "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};QueueEndpoint={3}",
                accountKeyParams);

        var accountString4 =
            string.Format(
                "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix={2};QueueEndpoint={3}",
                accountKeyParams);

        connectionStringRoundtripHelper(accountString1);
        connectionStringRoundtripHelper(accountString2);
        connectionStringRoundtripHelper(accountString3);
        connectionStringRoundtripHelper(accountString4);

        var accountString5 =
            string.Format(
                "AccountName={0};AccountKey={1};EndpointSuffix={2};",
                accountKeyParams);

        var accountString6 =
            string.Format(
                "AccountName={0};AccountKey={1};",
                accountKeyParams);

        var accountString7 =
            string.Format(
                "AccountName={0};AccountKey={1};QueueEndpoint={3}",
                accountKeyParams);

        var accountString8 =
            string.Format(
                "AccountName={0};AccountKey={1};EndpointSuffix={2};QueueEndpoint={3}",
                accountKeyParams);

        connectionStringRoundtripHelper(accountString5);
        connectionStringRoundtripHelper(accountString6);
        connectionStringRoundtripHelper(accountString7);
        connectionStringRoundtripHelper(accountString8);

        // shared access

        var accountString9 =
            string.Format(
                "DefaultEndpointsProtocol=http;AccountName={0};SharedAccessSignature={1};EndpointSuffix={2};",
                accountSasParams);

        var accountString10 =
            string.Format(
                "DefaultEndpointsProtocol=https;AccountName={0};SharedAccessSignature={1};",
                accountSasParams);

        var accountString11 =
            string.Format(
                "DefaultEndpointsProtocol=https;AccountName={0};SharedAccessSignature={1};QueueEndpoint={3}",
                accountSasParams);

        var accountString12 =
            string.Format(
                "DefaultEndpointsProtocol=https;AccountName={0};SharedAccessSignature={1};EndpointSuffix={2};QueueEndpoint={3}",
                accountSasParams);

        connectionStringRoundtripHelper(accountString9);
        connectionStringRoundtripHelper(accountString10);
        connectionStringRoundtripHelper(accountString11);
        connectionStringRoundtripHelper(accountString12);

        var accountString13 =
            string.Format(
                "AccountName={0};SharedAccessSignature={1};EndpointSuffix={2};",
                accountSasParams);

        var accountString14 =
            string.Format(
                "AccountName={0};SharedAccessSignature={1};",
                accountSasParams);

        var accountString15 =
            string.Format(
                "AccountName={0};SharedAccessSignature={1};QueueEndpoint={3}",
                accountSasParams);

        var accountString16 =
            string.Format(
                "AccountName={0};SharedAccessSignature={1};EndpointSuffix={2};QueueEndpoint={3}",
                accountSasParams);

        connectionStringRoundtripHelper(accountString13);
        connectionStringRoundtripHelper(accountString14);
        connectionStringRoundtripHelper(accountString15);
        connectionStringRoundtripHelper(accountString16);

        // shared access no account name

        var accountString17 =
            string.Format(
                "SharedAccessSignature={1};QueueEndpoint={3}",
                accountSasParams);

        connectionStringRoundtripHelper(accountString17);
    }

    void connectionStringRoundtripHelper(string accountString)
    {
        var originalAccount = CloudStorageAccount.Parse(accountString);
        var copiedAccountString = originalAccount.ToString(true);
        var copiedAccount = CloudStorageAccount.Parse(copiedAccountString);

        // make sure it round trips
        AccountsAreEqual(originalAccount, copiedAccount);
    }

    [Fact]
    public void CloudStorageAccountConnectionStringExpectedExceptions()
    {
        var endpointCombinations = new[]
            {
                    new[] { "BlobEndpoint={3}" },
                    new[] { "QueueEndpoint={3}" },
                    new[] { "TableEndpoint={3}" },
                    new[] { "FileEndpoint={3}" }
                };

        var accountKeyParams = new[]
            {
                    AccountName,
                    AccountKey,
                    "fake.endpoint.suffix",
                    "https://primary.endpoint/",
                };

        var accountSasParams = new[]
            {
                    AccountName,
                    "sasTest",
                    "fake.endpoint.suffix",
                    "https://primary.endpoint/",
                };

        foreach (var endpointCombination in endpointCombinations)
        {
            // account key

            var accountStringKeyPrimary =
                string.Format(
                    "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};EndpointSuffix={2};" + endpointCombination[0],
                    accountKeyParams
                    );

            CloudStorageAccount.Parse(accountStringKeyPrimary); // no exception expected

            // account key, no default protocol

            var accountStringKeyNoDefaultProtocolPrimary =
                string.Format(
                    "AccountName={0};AccountKey={1};EndpointSuffix={2};" + endpointCombination[0],
                    accountKeyParams
                    );

            CloudStorageAccount.Parse(accountStringKeyNoDefaultProtocolPrimary); // no exception expected

            // SAS

            var accountStringSasPrimary =
                string.Format(
                    "DefaultEndpointsProtocol=https;AccountName={0};SharedAccessSignature={1};EndpointSuffix={2};" + endpointCombination[0],
                    accountSasParams
                    );

            CloudStorageAccount.Parse(accountStringSasPrimary); // no exception expected

            // SAS, no default protocol

            var accountStringSasNoDefaultProtocolPrimary =
                string.Format(
                    "AccountName={0};SharedAccessSignature={1};EndpointSuffix={2};" + endpointCombination[0],
                    accountSasParams
                    );

            CloudStorageAccount.Parse(accountStringSasNoDefaultProtocolPrimary); // no exception expected

            // SAS without AccountName

            var accountStringSasNoNameNoEndpoint =
                string.Format(
                    "SharedAccessSignature={1}",
                    accountSasParams
                    );

            var accountStringSasNoNamePrimary =
                string.Format(
                    "SharedAccessSignature={1};" + endpointCombination[0],
                    accountSasParams
                    );

            Assert.Throws<FormatException>(() => CloudStorageAccount.Parse(accountStringSasNoNameNoEndpoint));

            CloudStorageAccount.Parse(accountStringSasNoNamePrimary); // no exception expected
        }
    }

    [Fact]
    public void CloudStorageAccountClientMethods()
    {
        var account = new CloudStorageAccount(new StorageCredentials(AccountName, AccountKey), true);

        var blob = account.CreateCloudBlobClient();
        var queue = account.CreateCloudQueueClient();
        var table = account.CreateCloudTableClient();

        // check endpoints
        Assert.True(account.BlobEndpoint!.IsBaseOf(blob.Uri), "Blob endpoint doesn't match account");
        Assert.True(account.QueueEndpoint!.IsBaseOf(queue.Uri), "Queue endpoint doesn't match account");

        Assert.Equal(account.Credentials.AccountName, table.AccountName);
    }

    [Fact]
    public void CloudStorageAccountTryParseNullEmpty()
    {
        // TryParse should not throw exception when passing in null or empty string
        Assert.False(CloudStorageAccount.TryParse(null!, out var account));
        Assert.False(CloudStorageAccount.TryParse(string.Empty, out account));
    }

    [Fact]
    public void CloudStorageAccountDevStoreNonTrueFails()
    {
        Assert.False(CloudStorageAccount.TryParse("UseDevelopmentStorage=false", out var account));
    }

    [Fact]
    public void CloudStorageAccountDevStorePlusAccountFails()
    {
        Assert.False(CloudStorageAccount.TryParse("UseDevelopmentStorage=false;AccountName=devstoreaccount1", out var account));
    }

    [Fact]
    public void CloudStorageAccountDevStorePlusEndpointFails()
    {
        Assert.False(CloudStorageAccount.TryParse("UseDevelopmentStorage=false;BlobEndpoint=http://127.0.0.1:1000/devstoreaccount1", out var account));
    }

    [Fact]
    public void CloudStorageAccountDefaultEndpointOverride()
    {
        Assert.True(CloudStorageAccount.TryParse("DefaultEndpointsProtocol=http;BlobEndpoint=http://customdomain.com/;AccountName=asdf;AccountKey=123=", out var account));
        Assert.Equal(new Uri("http://customdomain.com/"), account?.BlobEndpoint);
    }

    [Fact]
    public void CloudStorageAccountDevStoreProxyUri()
    {
        Assert.True(CloudStorageAccount.TryParse("UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://ipv4.fiddler", out var account));
        Assert.Equal(new Uri("http://ipv4.fiddler:10000/devstoreaccount1"), account?.BlobEndpoint);
        Assert.Equal(new Uri("http://ipv4.fiddler:10001/devstoreaccount1"), account?.QueueEndpoint);
        Assert.Equal(new Uri("http://ipv4.fiddler:10002/devstoreaccount1"), account?.TableEndpoint);
        Assert.Null(account?.FileEndpoint);
    }

    [Fact]
    public void CloudStorageAccountDevStoreRoundtrip()
    {
        var accountString = "UseDevelopmentStorage=true";

        Assert.Equal(accountString, CloudStorageAccount.Parse(accountString).ToString(true));
    }

    [Fact]
    public void CloudStorageAccountDevStoreProxyRoundtrip()
    {
        var accountString = "UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://ipv4.fiddler/";

        Assert.Equal(accountString, CloudStorageAccount.Parse(accountString).ToString(true));
    }

    [Fact]
    public void CloudStorageAccountDefaultCloudRoundtrip()
    {
        var accountString = "DefaultEndpointsProtocol=http;AccountName=test;AccountKey=abc=";

        Assert.Equal(accountString, CloudStorageAccount.Parse(accountString).ToString(true));
    }

    [Fact]
    public void CloudStorageAccountAnonymousRoundtrip()
    {
        var accountString = "BlobEndpoint=http://blobs/";

        Assert.Equal(accountString, CloudStorageAccount.Parse(accountString).ToString(true));

        var account = new CloudStorageAccount(new StorageCredentials(), new Uri("http://blobs/"), null, null, null);

        AccountsAreEqual(account, CloudStorageAccount.Parse(account.ToString(true)));
    }
}