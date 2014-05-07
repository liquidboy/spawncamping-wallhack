namespace Cloud.LobbyService.WorkerRole
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    
    using Microsoft.WindowsAzure.ServiceRuntime;

    using Backend.Utils.Networking;
    using Backend.GameLogic;
    using AzureProductionSettings;

    public class WorkerRole : RoleEntryPoint
    {
        [Import(typeof(LobbyServiceSettings))]
        private LobbyServiceSettings LobbyServiceSettings { get; set; }
        
        [Import(typeof(BackplaneSettings))]
        public BackplaneSettings BackplaneSettings { get; set; }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private Task lobbyTask;

        private LobbyServerImpl lobbyServerImpl;

        public override bool OnStart()
        {
            var cc = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(AzureSettings).Assembly),
                new AssemblyCatalog(typeof(LobbyServiceBackplane).Assembly)));
            cc.SatisfyImportsOnce(this);

            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            Trace.TraceInformation("ServiceBusCredentials  " + this.BackplaneSettings.ServiceBusCredentials);
            Trace.TraceInformation("LobbyServiceInstanceId " + this.BackplaneSettings.InstanceId);
            Trace.TraceInformation("Settings.IPEndPoint    " + this.LobbyServiceSettings.IPEndPoint.ToString());

            this.lobbyServerImpl = cc.GetExportedValue<LobbyServerImpl>();
            var server = new AsyncServerHost(this.LobbyServiceSettings.IPEndPoint);
            this.lobbyTask = server.Start(lobbyServerImpl, cts.Token);

            return base.OnStart();
        }

        private async Task RunAsync(CancellationToken ct)
        {
            Trace.TraceInformation("Cloud.LobbyService.WorkerRole entry point called", "Information");

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));

                Trace.TraceInformation("Working", "Information");
            }
        }

        public override void Run()
        {
            RunAsync(this.cts.Token).Wait();
        }

        public override void OnStop()
        {
            this.cts.Cancel();
            this.lobbyServerImpl.Dispose();

            base.OnStop();
        }
    }
}