namespace Backend.GameLogic.Security
{
    using System;
    using System.ComponentModel.Composition;
    using System.IdentityModel.Protocols.WSTrust;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.ServiceModel.Security.Tokens;
    using System.Text;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using Messages;


    [Export(typeof(SymmetricKeyGenerator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SymmetricKeyGenerator : IPartImportsSatisfiedNotification
    {
        [Import(typeof(BackplaneSettings))]
        public BackplaneSettings BackplaneSettings { get; set; }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            _synchronizedKey = this.EstablishAndRetrieveSyncronizedKey();
        }

        private byte[] _synchronizedKey;

        private CloudBlockBlob GetKeyBlobReference()
        {
            var storageAccount = CloudStorageAccount.Parse(this.BackplaneSettings.StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var gamebackplaneContainer = blobClient.GetContainerReference("gamebackplane");
            gamebackplaneContainer.CreateIfNotExists();
            return  gamebackplaneContainer.GetBlockBlobReference("jwt.key");
        }

        private byte[] GetBlobContents(CloudBlockBlob keyBlob)
        {
            var base64 = keyBlob.DownloadText(encoding: Encoding.UTF8);
            return Convert.FromBase64String(base64);
        }

        private byte[] EstablishAndRetrieveSyncronizedKey()
        {
            var keyBlob = GetKeyBlobReference();
            if (keyBlob.Exists())
            {
                return GetBlobContents(keyBlob);
            } 
            else 
            {
                var newKey = CreateNewKey();
                try
                {
                    var base64 = Convert.ToBase64String(newKey);
                    keyBlob.UploadText(base64, encoding: Encoding.UTF8,  
                        accessCondition: AccessCondition.GenerateIfNoneMatchCondition("*"));
                }
                catch (StorageException) 
                {
                    return GetBlobContents(keyBlob);
                }

                return newKey;
            }
        }

        private byte[] CreateNewKey()
        {
            using (var rng = RNGCryptoServiceProvider.Create())
            {
                var key = new byte[1000];
                rng.GetBytes(key);
                return key;
            }
        }

        public PlayerAuthenticator CreateAuthenticator()
        {
            return new PlayerAuthenticator(this._synchronizedKey);
        }
    }
}
