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
        public LobbyWorkerRole()
        {
            this.cc = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(LobbyServiceSettings).Assembly),
                new AssemblyCatalog(typeof(AzureSettings).Assembly)));

            this.cc.SatisfyImportsOnce(this);
        }

        [Import(typeof(LobbyServiceSettings))]
        public LobbyServiceSettings LobbyServiceSettings { get; set; }
        
        [Import(typeof(SharedSettings))]
        public SharedSettings SharedSettings { get; set; }

        private readonly CompositionContainer cc;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private SiloHost siloHost;
        private Task lobbyTask;
        private LobbyServerImpl lobbyServerImpl;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            #region Launch silo in separate AppDomain

            var dataConnectionString = RoleEnvironment.GetConfigurationSettingValue("DataConnectionString");
            var orleansSiloConfiguration = File.ReadAllText("OrleansConfiguration.xml").Replace(
                "XXXDataConnectionStringValueXXX", dataConnectionString);
            this.siloHost = new SiloHost();
            this.siloHost.StartLobbyServiceSiloAppDomain(orleansSiloConfiguration);

            #endregion

            Trace.TraceInformation("LobbyServiceInstanceId " + this.SharedSettings.InstanceId);
            Trace.TraceInformation("Settings.IPEndPoint    " + this.LobbyServiceSettings.IPEndPoint.ToString());

            if (!OrleansAzureClient.IsInitialized)
            {
                OrleansAzureClient.Initialize("ClientConfiguration.xml");
            }

            var server = new AsyncServerHost(this.LobbyServiceSettings.IPEndPoint);
            this.lobbyServerImpl = cc.GetExportedValue<LobbyServerImpl>();
            this.lobbyTask = server.Start(lobbyServerImpl, cts.Token);

            return true;
        }

        private Task orleansClientTask;

        public override void Run() 
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        public override void OnStop()
        {
            this.cts.Cancel();
            this.siloHost.Shutdown();

            base.OnStop();
        }
    }
}