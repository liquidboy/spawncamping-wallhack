namespace Cloud.GameServerHost.WorkerRole
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.ServiceRuntime;

    using AzureProductionSettings;
    using Backend.GameLogic;
    using Backend.Utils.Networking;

    public class GameServerWorkerRole : RoleEntryPoint
    {
        [Import(typeof(GameServerVMAgent))]
        GameServerVMAgent agent;

        [Import(typeof(GameServerSettings))]
        GameServerSettings Settings;

        private Task agentTask;

        private Task proxyTask;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public GameServerWorkerRole()
        {
            var compositionContainer = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(AzureSettings).Assembly),
                new AssemblyCatalog(typeof(GameServerVMAgent).Assembly)
                ));

            compositionContainer.SatisfyImportsOnce(this);
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            var proxyServerHost = new AsyncServerHost(this.Settings.ProxyIPEndPoint);
            this.proxyTask = proxyServerHost.StartFunctional(async (client, ct) => await ProxyConnection.ProxyLogic(client, cts), cts.Token);

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