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

    public interface ILobbyServerDatabase
    {
        Task LoadAsync();
    }

    [Export(typeof(ILobbyServerDatabase))]
    public class LobbyServerDatabaseTableStorage : ILobbyServerDatabase, IPartImportsSatisfiedNotification
    {
        [Import(typeof(LobbyServiceSettings))]
        public LobbyServiceSettings LobbyServiceSettings { get; set; }

        private const string lobbyServiceTableName = "lobbyservice";
        private CloudTableClient m_cloudTableClient;
        private CloudTable m_table;

        public LobbyServerDatabaseTableStorage() { }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() { this.OnImportsSatisfiedAsync().Wait(); }

        private async Task OnImportsSatisfiedAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(this.LobbyServiceSettings.LobbyStorageConnectionString);
            m_cloudTableClient = storageAccount.CreateCloudTableClient();
            m_table = m_cloudTableClient.GetTableReference(tableName: lobbyServiceTableName);
            if (!await m_table.ExistsAsync())
            {
                await m_table.CreateAsync();
            }
        }

        async Task ILobbyServerDatabase.LoadAsync()
        {
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