namespace Backend.GameLogic.Configuration
{
    using System.ComponentModel.Composition;

    [Export(typeof(SharedSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SharedSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        public string InstanceId { get { return this.SettingsProvider.InstanceId; } }

        public string StorageConnectionString { get { return this.SettingsProvider.GetSetting("StorageConnectionString"); } }

        public string ServiceBusCredentials { get { return this.SettingsProvider.GetSetting("ServiceBusCredentials"); } }

        public string DNSName { get { return this.SettingsProvider.GetSetting("DNSName"); } }
    }
}
