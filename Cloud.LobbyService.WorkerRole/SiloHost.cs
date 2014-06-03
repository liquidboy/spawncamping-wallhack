namespace Cloud.LobbyService.WorkerRole
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.ServiceRuntime;
    
    using Orleans;
    using Orleans.Host.Azure;

    public class SiloHost
    {
        private static AppDomain orleansSiloHostDomain;
        private static OrleansAzureSilo silo;
        private static bool siloStartResult;
        private static Task orleansSiloTask;

        public void StartLobbyServiceSilo()
        {
            // var dire = new FileInfo(typeof(LobbyWorkerRole).Assembly.Location).Directory.FullName;

            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            SiloHost.orleansSiloHostDomain = AppDomain.CreateDomain(
                friendlyName: "OrleansHost",
                securityInfo: null,
                info: new AppDomainSetup
                {
                    ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                    ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                    PrivateBinPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath,
                    AppDomainInitializer = SiloHost.InitSilo
                });
        }

        public void Shutdown()
        {
            SiloHost.orleansSiloHostDomain.DoCallBack(SiloHost.ShutdownSilo);
        }

        static void InitSilo(string[] args)
        {
            SiloHost.silo = new OrleansAzureSilo();

            var cfgXml = File.ReadAllText("OrleansConfiguration.xml")
                .Replace("XXXDataConnectionStringValueXXX",
                RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"));

            var siloConfiguration = new OrleansConfiguration();
            siloConfiguration.Load(new StringReader(cfgXml));

            SiloHost.siloStartResult = silo.Start(
                deploymentId: RoleEnvironment.DeploymentId,
                myRoleInstance: RoleEnvironment.CurrentRoleInstance,
                config: siloConfiguration);

            SiloHost.orleansSiloTask = Task.Factory.StartNew(() =>
            {
                SiloHost.silo.Run();
            });
        }

        static void ShutdownSilo()
        {
            SiloHost.silo.Stop();
        }
    }
}
