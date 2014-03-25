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

    using Backend.GameLogic;
    using DevelopmentSettings;
    using Backend.Utils.Networking;

    public class WorkerRole : RoleEntryPoint
    {
        [Import(typeof(ILobbyServiceSettings))]
        private ILobbyServiceSettings Settings { get; set; }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private Task lobbyTask;
        private LobbyServerImpl lobbyServerImpl;

        public override bool OnStart()
        {
            var cc = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(RoleEnvironmentSettingsProvider).Assembly),
                new AssemblyCatalog(typeof(LobbyServiceBackplane).Assembly)));
            cc.SatisfyImportsOnce(this);

            ServicePointManager.DefaultConnectionLimit = 12;

            Trace.TraceInformation("Use Service bus " + this.Settings.ServiceBusCredentials);
            Trace.TraceInformation("Lobby Instance " + this.Settings.LobbyServiceInstanceId);
            Trace.TraceInformation("Lobby Port" + this.Settings.IPEndPoint.ToString());

            this.lobbyServerImpl = cc.GetExportedValue<LobbyServerImpl>();
            var server = new AsyncServerHost(this.Settings.IPEndPoint);
            this.lobbyTask = server.Start(lobbyServerImpl.HandleClient, cts.Token);

            return base.OnStart();
        }


        private async Task RunAsync(CancellationToken ct)
        {
            Trace.TraceInformation("Cloud.LobbyService.WorkerRole entry point called", "Information");

            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

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
            this.lobbyServerImpl.ShutDownAsync().Wait();

            base.OnStop();
        }
    }
}