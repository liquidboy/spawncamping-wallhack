﻿namespace Backend.GameLogic
{
    using System;
    using System.ComponentModel.Composition;
    using System.Net;

    [Export(typeof(AzureGameServerSettings))]
    public class AzureGameServerSettings
    {
        [Import(typeof(ISettingsProvider))]
        public ISettingsProvider SettingsProvider { get; set; }

        [Import(typeof(BackplaneSettings))]
        public BackplaneSettings BackplaneSettings { get; set; }

        public IPEndPoint ProxyIPEndPoint { get { return this.SettingsProvider.GetIPEndpoint("gameServerInstanceEndpoint"); } }

        public int PublicGameServerPort { get { return this.SettingsProvider.GetPublicPort("gameServerInstanceEndpoint"); } }
    }
}