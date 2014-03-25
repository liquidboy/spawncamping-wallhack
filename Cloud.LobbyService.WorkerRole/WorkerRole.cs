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

    public class WorkerRole : RoleEntryPoint
    {
        [Import(typeof(ILobbyServiceSettings))]
        private ILobbyServiceSettings Settings { get; set; }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public override bool OnStart()
        {
            var cc = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(RoleEnvironmentSettingsProvider).Assembly),
                new AssemblyCatalog(typeof(LobbyServiceBackplane).Assembly)));
            cc.SatisfyImportsOnce(this);

            ServicePointManager.DefaultConnectionLimit = 12;

            Trace.TraceInformation("Use Service bus " + this.Settings.ServiceBusCredentials);
            Trace.TraceInformation("Lobby Instance " + this.Settings.LobbyServiceInstanceId);

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

            base.OnStop();
        }
    }
}