namespace Backend.GameLogic
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Linq;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using System.Collections.Generic;
    using Microsoft.DPE.Samples.ChGeuer;
using System.Text;
    using Backend.GameLogic.Security;

    /// <summary>
    /// Manages the game server processes on a virtual machine. 
    /// </summary>
    [Export(typeof(GameServerVMAgent))]
    public class GameServerVMAgent : IPartImportsSatisfiedNotification, IDisposable
    {
        [Import(typeof(GameServerSettings))]
        public GameServerSettings Settings { get; set; }
        
        [Import(typeof(BackplaneSettings))]
        public BackplaneSettings BackplaneSettings { get; set; }


         [Import(typeof(SymmetricKeyGenerator))]
        public SymmetricKeyGenerator SymmetricKeyGenerator { get; set; }


        private List<Task> runningHosts = new List<Task>();
        private const string lobbyServiceTableName = "gameserveragents";
        private CloudTableClient m_cloudTableClient;
        private CloudTable m_table;

        void IPartImportsSatisfiedNotification.OnImportsSatisfied() { this.OnImportsSatisfiedAsync().Wait(); }

        private async Task OnImportsSatisfiedAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(this.BackplaneSettings.StorageConnectionString);
            m_cloudTableClient = storageAccount.CreateCloudTableClient();
            m_table = m_cloudTableClient.GetTableReference(tableName: lobbyServiceTableName);
            if (!await m_table.ExistsAsync())
            {
                try
                {
                    await m_table.CreateAsync();
                } catch (StorageException) { }
            }
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
            var agentTask = Task.Factory.StartNew(
                async () => await RunAsync(cts.Token),
                cts.Token, 
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Current).Unwrap();

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
                Trace.TraceInformation("Agent loops", "Information");

                await Task.Delay(TimeSpan.FromSeconds(1));
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