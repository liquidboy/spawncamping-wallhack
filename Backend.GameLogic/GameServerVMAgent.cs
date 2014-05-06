namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(GameServerVMAgent))]
    public class GameServerVMAgent
    {
        [Import(typeof(AzureGameServerSettings))]
        public AzureGameServerSettings Settings { get; set; }
    }

   
}
