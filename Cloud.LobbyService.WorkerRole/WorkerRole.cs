namespace Cloud.LobbyService.WorkerRole
{
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    
    using Microsoft.WindowsAzure.ServiceRuntime;

    using Backend.GameLogic;
    using DevelopmentSettings;

    public class WorkerRole : RoleEntryPoint
    {
        [Import(typeof(ILobbyServiceSettings))]
        private ILobbyServiceSettings Settings { get; set; }

        public override bool OnStart()
        {
            var compositionContainer = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(RoleEnvironmentSettingsProvider).Assembly),
                new AssemblyCatalog(typeof(LobbyServiceBackplane).Assembly)));
            compositionContainer.SatisfyImportsOnce(this);

            ServicePointManager.DefaultConnectionLimit = 12;

            Trace.TraceInformation("Use Service bus " + this.Settings.ServiceBusCredentials);
            Trace.TraceInformation("Lobby Instance " + this.Settings.LobbyServiceInstanceId);

            return base.OnStart();
        }

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("Cloud.LobbyService.WorkerRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working", "Information");
            }
        }
    }
}