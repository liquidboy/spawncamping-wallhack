namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    using Security;
    using Configuration;
    using Microsoft.DPE.Samples.ChGeuer;

    /// <summary>
    /// Manages the game server processes on a virtual machine. 
    /// </summary>
    [Export(typeof(GameServerVMAgent))]
    public class GameServerVMAgent : IPartImportsSatisfiedNotification, IDisposable
    {
        [Import(typeof(GameServerSettings))]
        public GameServerSettings Settings { get; set; }
        
        [Import(typeof(SharedSettings))]
        public SharedSettings SharedSettings { get; set; }

        [Import(typeof(SymmetricKeyGenerator))]
        public SymmetricKeyGenerator SymmetricKeyGenerator { get; set; }

        private List<Task> runningHosts = new List<Task>();
        private CloudTableClient m_cloudTableClient;
        private CloudTable m_table;
        private const string lobbyServiceTableName = "gameserveragents";

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() 
        {
            var storageAccount = CloudStorageAccount.Parse(this.SharedSettings.StorageConnectionString);
            this.m_cloudTableClient = storageAccount.CreateCloudTableClient();
            this.m_table = m_cloudTableClient.GetTableReference(tableName: lobbyServiceTableName);
            this.m_table.CreateIfNotExists();
        }

        private Func<Process> CreateGameServerRecipy(int internalServerPort)
        {
            return () =>
            {
                var arguments = string.Join(" ", new List<object> 
                {
                    this.Settings.ProxyIPEndPoint.Address,
                    internalServerPort,
                    this.SymmetricKeyGenerator.GetKeyForGameServer()
                });

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = ".",
                        FileName = @".\Backend.GameServer.exe",
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                Action<DataReceivedEventArgs> outputDataReceived = args => Trace.WriteLine(args.Data, "stdout");
                Action<DataReceivedEventArgs> errorDataReceived = args => Trace.WriteLine(args.Data, "stderr");

                process.OutputDataReceived += (s, a) => outputDataReceived(a);
                process.ErrorDataReceived += (s, a) => errorDataReceived(a);

                return process;
            };
        }

        public Task Start(CancellationTokenSource cts)
        {
            Trace.TraceInformation("Launching message pump");
            var agentTask = Task.Factory.StartNew(
                async () => await RunAsync(cts.Token),
                cts.Token, 
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Current).Unwrap();

            Trace.TraceInformation("Launching game server process from agent");
            var innerGameServerPort = 4002;
            var host = new RestartingProcessHost(CreateGameServerRecipy(innerGameServerPort), cts, 
                RestartingProcessHost.RestartPolicy.MaximumLaunchTimes(1));
            var runningHostTask = host.StartRunTask();
            runningHosts.Add(runningHostTask);

            return agentTask;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Agent loops, checking for new game server orders", "Information");

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

       

        #region IDisposable

        // Flag: Has Dispose already been called? 
        bool m_disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if (disposing)
            {
                Trace.TraceInformation("Disposing agent", "Information");
                // this.LobbyConnector.Dispose();
            }

            // Free any unmanaged objects here. 
            //
            m_disposed = true;
        }

        #endregion
    }
}