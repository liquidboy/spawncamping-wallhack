namespace Cloud.LobbyService.WorkerRole
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.ServiceRuntime;

    using Orleans;
    using Orleans.Host.Azure;
    using Orleans.Host.Azure.Client;

    using AzureProductionSettings;
    using Backend.GameLogic;
    using Backend.GameLogic.Configuration;
    using Backend.Utils.Networking;
    using Backend.GrainInterfaces;
    using Backend.GrainImplementations;

    public class LobbyWorkerRole : RoleEntryPoint
    {
        [Import(typeof(LobbyServiceSettings))]
        private LobbyServiceSettings LobbyServiceSettings { get; set; }
        
        [Import(typeof(SharedSettings))]
        public SharedSettings SharedSettings { get; set; }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private readonly SiloHost siloHost = new SiloHost();

        private Task lobbyTask;

        private LobbyServerImpl lobbyServerImpl;

        private static Task orleansCLientTask;

        private void ComposeLobbyServiceEndpoints()
        {
            var cc = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(LobbyServiceSettings).Assembly),
                new AssemblyCatalog(typeof(AzureSettings).Assembly)));
            cc.SatisfyImportsOnce(this);

            // Trace.TraceInformation("ServiceBusCredentials  " + this.SharedSettings.ServiceBusCredentials);
            Trace.TraceInformation("LobbyServiceInstanceId " + this.SharedSettings.InstanceId);
            Trace.TraceInformation("Settings.IPEndPoint    " + this.LobbyServiceSettings.IPEndPoint.ToString());

            var server = new AsyncServerHost(this.LobbyServiceSettings.IPEndPoint);
            this.lobbyServerImpl = cc.GetExportedValue<LobbyServerImpl>();
            this.lobbyTask = server.Start(lobbyServerImpl, cts.Token);
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            this.ComposeLobbyServiceEndpoints();
            this.siloHost.StartLobbyServiceSilo();

            return true;
        }

        public override void Run() 
        {
            LobbyWorkerRole.orleansCLientTask = OrleansSampleClient.LaunchOrleansClients();

            while (true)
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        public override void OnStop()
        {
            this.cts.Cancel();
            this.lobbyServerImpl.Dispose();
            this.siloHost.Shutdown();

            base.OnStop();
        }
    }
}