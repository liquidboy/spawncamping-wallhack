namespace DevelopmentSettings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;


    public class EnvironmentSettingsProvider
    {
        [Export("ServiceBusCredentials")]
        public string ServiceBusCredentials 
        { 
            get 
            {
                return Environment.GetEnvironmentVariable("ServiceBusCredentials");
            } 
        }
    }
}
