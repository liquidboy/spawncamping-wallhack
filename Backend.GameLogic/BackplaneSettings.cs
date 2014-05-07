namespace Backend.GameLogic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Export(typeof(BackplaneSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BackplaneSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        public string InstanceId { get { return this.SettingsProvider.GetInstanceId(); } }

        public string StorageConnectionString { get { return this.SettingsProvider.GetSetting("StorageConnectionString"); } }

        public string ServiceBusCredentials { get { return this.SettingsProvider.GetSetting("ServiceBusCredentials"); } }
    }
}
