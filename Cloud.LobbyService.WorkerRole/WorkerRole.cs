namespace Cloud.LobbyService.WorkerRole
{
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
    using System;
    using Backend.GrainImplementations;

    public class WorkerRole : RoleEntryPoint
    {
        [Import(typeof(LobbyServiceSettings))]
        private LobbyServiceSettings LobbyServiceSettings { get; set; }
        
        [Import(typeof(SharedSettings))]
        public SharedSettings SharedSettings { get; set; }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private Task lobbyTask;

        private LobbyServerImpl lobbyServerImpl;

        private OrleansAzureSilo silo;

        private void ComposeLobbyServiceEndpoints()
        {
            var cc = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(AzureSettings).Assembly),
                new AssemblyCatalog(typeof(LobbyServiceBackplane).Assembly)));
            cc.SatisfyImportsOnce(this);

            Trace.TraceInformation("ServiceBusCredentials  " + this.SharedSettings.ServiceBusCredentials);
            Trace.TraceInformation("LobbyServiceInstanceId " + this.SharedSettings.InstanceId);
            Trace.TraceInformation("Settings.IPEndPoint    " + this.LobbyServiceSettings.IPEndPoint.ToString());

            this.lobbyServerImpl = cc.GetExportedValue<LobbyServerImpl>();
            var server = new AsyncServerHost(this.LobbyServiceSettings.IPEndPoint);
            this.lobbyTask = server.Start(lobbyServerImpl, cts.Token);
        }

        private bool StartLobbyServiceSilo()
        {
            this.silo = new OrleansAzureSilo();

            var cfgXml = File.ReadAllText("OrleansConfiguration.xml")
                .Replace("XXXDataConnectionStringValueXXX",
                RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            var siloConfiguration = new OrleansConfiguration();
            siloConfiguration.Load(new StringReader(cfgXml));

            return silo.Start(
                deploymentId: RoleEnvironment.DeploymentId,
                myRoleInstance: RoleEnvironment.CurrentRoleInstance,
                config: siloConfiguration);
        }


        Task orleansCLientTask;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 64;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            this.ComposeLobbyServiceEndpoints();
            return this.StartLobbyServiceSilo();
        }

        public override void Run() 
        {
            this.orleansCLientTask = OrleansClient.LaunchOrleansClients();

            this.silo.Run(); 
        }

        public override void OnStop()
        {
            this.cts.Cancel();
            this.lobbyServerImpl.Dispose();

            this.silo.Stop(); 

            base.OnStop();
        }
    }

    public static class OrleansClient
    {
        public static Task LaunchOrleansClients()
        {
            return Task.Factory.StartNew(() =>
            {
                if (!OrleansAzureClient.IsInitialized)
                {
                    var x = GrainClient.Current;
                    OrleansAzureClient.Initialize("ClientConfiguration.xml");
                }


                var infinitelyRunningTasks = new string[] 
                    { 
                        "00000000-acf8-4f39-9cf1-14a84bb82980",
                        "11000000-95f1-42e4-b69f-90eab2ac8ec1",
                        "22000000-e9f0-4a37-8d6b-24de340d6b68",
                        "33000000-7968-4d94-9859-4cd869319cb8",

                        "44000000-53f9-4a96-acb6-e96f5d8085cf",
                        "55000000-4880-43ee-95eb-3523d8888608",
                        "66000000-e614-455b-8d8d-25d899ee9acd",
                        "77000000-6fc4-47a6-ad5b-57df7c324848",
                        "88000000-3742-4846-97e3-82f2e506652d",
                        "99000000-7379-47fe-85ae-2b4e284ace05",
                        "aa000000-8be5-4701-b4e9-f4c608637493",
                        "bb000000-0117-4741-8e58-7bb81e403947",
                        "cc000000-c961-4dbc-9ba0-614a93aa1bc9",
                        "dd000000-c328-498f-a653-7aecccf3ba54",
                        "ee000000-feeb-4f56-ac73-783578198438",
                        "ff000000-d10c-4d79-b0f8-8ed67ac33ca7",
                        "1e159e6d-cc31-4028-ac23-64ee318f973f",
                        "789eeea4-ef13-4d81-9ceb-ca30bfae50cb",
                        "ba8ded1c-9831-4f44-a9b9-bfee869007d3",
                        "0d64a5d8-60c8-4ea2-87cc-376dac108cfc",
                        "b94b40c2-c3da-4106-99cf-d9fb81e0111b",
                        "7b13b66b-accc-434c-ad82-e5cd0eb01aee"
                    }
                        .Select(_ => Guid.Parse(_))
                        .Select(gamerId => Task.Factory.StartNew(async () =>
                        {
                            GameServerStartParams gameServerInfo = null;
                            var gamer = await Gamer.CreateAsync(gamerId, server =>
                            {
                                gameServerInfo = server;
                                Trace.WriteLine(string.Format("Player {0} joins server {1}", gamerId, server.GameServerID));
                            });

                            Trace.TraceInformation(string.Format("Start gamer ID {0}", gamerId));

                            while (true)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(2));

                                if (gameServerInfo == null)
                                {
                                    Trace.TraceInformation(string.Format("Game for player {0} not yet started", gamer.Id));
                                }
                                else
                                {
                                    Trace.TraceInformation(string.Format("Game for player {0} started: gameServerGrain {1}",
                                        gamer.Id, gameServerInfo.GameServerID));
                                }
                            }
                        }).Unwrap())
                        .ToArray();

                try
                {
                    Task.WaitAll(infinitelyRunningTasks);


                    while (true)
                    {
                        Trace.TraceInformation("Endless useless loop");
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }

                    Trace.TraceInformation("Task.WaitAll ended?");
                }
                catch (Exception)
                {
                    foreach (var t in infinitelyRunningTasks)
                    {
                        if (t.Exception != null)
                        {
                            foreach (var ie in t.Exception.InnerExceptions)
                            {
                                for (var e = ie; e != null; e = e.InnerException)
                                {
                                    Trace.TraceError(e.Message);
                                    Trace.TraceError(string.Format(e.Message));
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}