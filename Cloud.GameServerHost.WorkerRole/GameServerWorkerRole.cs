namespace Cloud.GameServerHost.WorkerRole
{
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using Microsoft.WindowsAzure.ServiceRuntime;

    using AzureProductionSettings;
    using Backend.GameLogic;
    using System.Threading.Tasks;
    using System;

    public class GameServerWorkerRole : RoleEntryPoint
    {
        [Import]
        GameServerVMAgent agent;
        
        private Task agentTask;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public GameServerWorkerRole()
        {
            var compositionContainer = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(AzureSettings).Assembly),
                new AssemblyCatalog(typeof(GameServerVMAgent).Assembly)
                ));

            compositionContainer.SatisfyImportsOnce(this);

            var a = this.agent.Settings.IPEndPoint;
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            this.agentTask = this.agent.Start(cts.Token);

            return base.OnStart();
        }

        private async Task RunAsync(CancellationToken ct)
        {
            Trace.TraceInformation("Cloud.GameServerHost.WorkerRole entry point called", "Information");

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
            this.agent.Dispose();
        }
    }
}