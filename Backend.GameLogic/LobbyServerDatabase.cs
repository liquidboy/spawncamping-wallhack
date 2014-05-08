namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Configuration;

    public interface ILobbyServerDatabase
    {
        Task LoadAsync();
    }

    [Export(typeof(ILobbyServerDatabase))]
    public class LobbyServerDatabaseTableStorage : ILobbyServerDatabase, IPartImportsSatisfiedNotification
    {
        [Import(typeof(LobbyServiceSettings))]
        public LobbyServiceSettings LobbyServiceSettings { get; set; }

        [Import(typeof(SharedSettings))]
        public SharedSettings SharedSettings { get; set; }

        private const string lobbyServiceTableName = "lobbyservice";
        private CloudTableClient m_cloudTableClient;
        private CloudTable m_table;

        public LobbyServerDatabaseTableStorage() { }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() { this.OnImportsSatisfiedAsync().Wait(); }

        private async Task OnImportsSatisfiedAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(this.SharedSettings.StorageConnectionString);
            m_cloudTableClient = storageAccount.CreateCloudTableClient();
            m_table = m_cloudTableClient.GetTableReference(tableName: lobbyServiceTableName);
            if (!await m_table.ExistsAsync())
            {
                try
                {
                    await m_table.CreateAsync();
                }
                catch (StorageException) { }
            }
        }

        async Task ILobbyServerDatabase.LoadAsync()
        {
            await Task.Yield();
            // throw new NotImplementedException();
        }

        //public static async Task Loopy() {}

        //Task ILobbyServerDatabase.LoadAsync()
        //{
        //    Task.Factory.StartNew(async () => { Loopy(); }, TaskCreationOptions.LongRunning)
        //        .ContinueWith(HandleError, TaskContinuationOptions.OnlyOnFaulted);
        //}

        //private static Task HandleError(Task t) {
        //    if (t.IsFaulted) {
        //        var e = t.Exception;
        //    }
        //}
    }
}