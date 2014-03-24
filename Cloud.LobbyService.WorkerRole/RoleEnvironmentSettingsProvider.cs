﻿namespace Cloud.LobbyService.WorkerRole
{
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.ComponentModel.Composition;

    public class RoleEnvironmentSettingsProvider
    {
        [Export("ServiceBusCredentials")]
        public string ServiceBusCredentials
        {
            get
            {
                return RoleEnvironment.GetConfigurationSettingValue("ServiceBusCredentials");
            }
        }
    }
}
