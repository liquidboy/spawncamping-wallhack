﻿namespace Backend.LobbyServer
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Backend.GameLogic;
    using Backend.Utils.Networking;
    using DevelopmentSettings;
    using Backend.GameLogic.Configuration;

    class LobbyServerProgram
    {
        static void Main(string[] args)
        {
            Console.Title = "Backend.LobbyServer";

            var cts = new CancellationTokenSource();

            var compositionContainer = new CompositionContainer(new AggregateCatalog(
                new AssemblyCatalog(typeof(DevelopmentSettingsProvider).Assembly)
                ));
            var settings = compositionContainer.GetExportedValue<LobbyServiceSettings>();

            var lobbyServerImpl = compositionContainer.GetExportedValue<LobbyServerImpl>();
            var server = new AsyncServerHost(settings.IPEndPoint);
            Task t = server.Start(lobbyServerImpl, cts.Token);

            Console.WriteLine("Loby server launched");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}