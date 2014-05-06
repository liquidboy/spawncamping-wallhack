namespace Cloud.GameServerHost.WorkerRole
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Backend.GameLogic;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition;
    using AzureProductionSettings;

    public class GameServerWorkerRole : RoleEntryPoint
    {
        [Import]
        GameServerVMAgent agent;

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
            ServicePointManager.DefaultConnectionLimit = 12;

            return base.OnStart();
        }

        public override void Run()
        {
            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working", "Information");
            }
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}
