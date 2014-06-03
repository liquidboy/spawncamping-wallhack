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

        public void StartLobbyServiceSiloAppDomain(string orleansConfigurationXml)
        {
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
                    AppDomainInitializer = SiloHost.InitSilo,
                    AppDomainInitializerArguments = new string[] { orleansConfigurationXml }
                });
        }

        public void Shutdown()
        {
            SiloHost.orleansSiloHostDomain.DoCallBack(SiloHost.ShutdownSilo);
        }

        private static void InitSilo(string[] args)
        {
            SiloHost.silo = new OrleansAzureSilo();
            var siloConfiguration = new OrleansConfiguration();
            siloConfiguration.Load(new StringReader(args[0]));

            SiloHost.siloStartResult = silo.Start(
                deploymentId: RoleEnvironment.DeploymentId,
                myRoleInstance: RoleEnvironment.CurrentRoleInstance,
                config: siloConfiguration);

            SiloHost.orleansSiloTask = Task.Factory.StartNew(() =>
            {
                SiloHost.silo.Run();
            });
        }

        private static void ShutdownSilo()
        {
            SiloHost.silo.Stop();
        }
    }
}