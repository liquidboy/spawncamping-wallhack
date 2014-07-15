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
    using Backend.GameLogic.Configuration;
    using Microsoft.DPE.Samples.ChGeuer;
    using System.Text;
    using System.IO;

    public class GameServerWorkerRole : RoleEntryPoint
    {
        [Import(typeof(GameServerVMAgent))]
        public GameServerVMAgent agent;

        [Import(typeof(GameServerSettings))]
        public GameServerSettings Settings;

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

        private Func<Process> CreateProxyProcess()
        {
            return () =>
            {
                var arguments = new StringBuilder();
                arguments.Append(string.Format(" {0}", this.Settings.ProxyIPEndPoint.Address.ToString()));
                arguments.Append(string.Format(" {0}", this.Settings.ProxyIPEndPoint.Port.ToString()));

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = ".",
                        FileName = "Backend.ProxyServer.exe",
                        Arguments = arguments.ToString(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    },
                    EnableRaisingEvents = true
                };

                //Func<string, string> getVariable = variable => process.StartInfo.EnvironmentVariables[variable];
                //Action<string, string> setVariable = (variable, value) => process.StartInfo.EnvironmentVariables[variable] = value;
                //setVariable("JAVA_HOME", relToTomcat(jdkFolder));

                Action<DataReceivedEventArgs> outputDataReceived = args => Trace.WriteLine(args.Data, "stdout");
                Action<DataReceivedEventArgs> errorDataReceived = args => Trace.WriteLine(args.Data, "stderr");

                process.OutputDataReceived += (s, a) => outputDataReceived(a);
                process.ErrorDataReceived += (s, a) => errorDataReceived(a);

                return process;
            };
        }


        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;


            var proxyHost = new RestartingProcessHost(CreateProxyProcess(), cts, 
                RestartingProcessHost.RestartPolicy.IgnoreCrashes());
            this.proxyTask = proxyHost.StartRunTask();


            this.agentTask = this.agent.Start(cts);

            return base.OnStart();
        }


        public override void Run()
        {
            Task.WaitAll(this.proxyTask, this.agentTask);
        }

        public override void OnStop()
        {
            this.cts.Cancel();
            this.agent.Dispose();
        }
    }
}