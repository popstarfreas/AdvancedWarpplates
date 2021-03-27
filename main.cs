using System;
using System.Reflection;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace AdvancedWarpplates
{
    [ApiVersion(2, 1)]
    public class WarpplatePlugin : TerrariaPlugin
    {
        public Player[] Players = new Player[256];
        public WarpplateManager Manager;
        public Timer QuickUpdate = new Timer(1000);
        public Commands Commands;

        private DateTime LastCheck = DateTime.UtcNow;

        public override string Name
        {
            get { return "Warpplate"; }
        }
        public override string Author
        {
            get { return "Maintained by popstarfreas"; }
        }
        public override string Description
        {
            get { return "Warpplate"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public WarpplatePlugin(Main game)
            : base(game)
        {
            Order = 1;
        }

        /// <summary>
        /// Registers all necessary hooks to set up the plugin
        /// </summary>
        public override void Initialize()
        {
            Manager = new WarpplateManager(TShock.DB);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInit);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        }

        /// <summary>
        /// Disposes all hooks registered
        /// </summary>
        /// <param name="disposing">Whether or not the plugin is being disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInit);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Loads all the warpplates for the world when it has loaded
        /// </summary>
        /// <param name="args"></param>
        private async void OnPostInit(EventArgs args)
        {
            await Manager.ReloadAllWarpplates();
            QuickUpdate.Elapsed += OnQuickUpdate;
            QuickUpdate.Enabled = true;
        }

        /// <summary>
        /// Initializes all of the commands for this plugin
        /// </summary>
        /// <param name="args">The initialize event arguments</param>
        public void OnInitialize(EventArgs args)
        {
            Commands = new Commands(Manager, Players);
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.SetWarpplate, "swp"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.DeleteWarpplate, "dwp"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.SetWarpplateDestination, "swpd"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.RemoveWarpplateDestination, "rwpd"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.WarpplateInformation, "wpi"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.ListWarpplates, "lwp"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.use", Commands.WarpplateAllow, "wpa"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.ReloadWarpplates, "rwp"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.SetWarpplateDelay, "swpdl"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.SetWarpplateWidth, "swpw"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.SetWarpplateHeight, "swph"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.SetWarpplateSize, "swps"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.set", Commands.SetWarpplateLabel, "swpl"));
            TShockAPI.Commands.ChatCommands.Add(new Command("warpplate.setdimensional", Commands.SetWarpplateDimension, "swpdim"));
        }
        
        /// <summary>
        /// Creates a new player object when a player joins
        /// </summary>
        /// <param name="args">The greet player arguments</param>
        public void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            lock (Players)
            {
                Players[args.Who] = new Player(args.Who, Manager);
            }
        }

        /// <summary>
        /// Nulls out the index of the players list when a player leaves
        /// </summary>
        /// <param name="args">The leave event arguments</param>
        private void OnLeave(LeaveEventArgs args)
        {
            lock (Players)
            {
                Players[args.Who] = null;
            }
        }

        /// <summary>
        /// Runs the checks for every player if they have enabled warpplate use and have permission
        /// </summary>
        /// <param name="sender">The timer object that invoked this method</param>
        /// <param name="args">The elapsed event args</param>
        private void OnQuickUpdate(object sender, ElapsedEventArgs args)
        {
            lock (Players)
            {
                for (int i = 0; i < Players.Length; i++)
                {
                    Player player = Players[i];
                    if (player == null || player.TSPlayer == null)
                    {
                        continue;
                    }

                    try
                    {
                        if (player.TSPlayer.Group.HasPermission("warpplate.use") && player.CanUseWarpplates)
                        {
                            player.Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        TShock.Log.Error(ex.ToString());
                    }
                }
            }
        }
    }
}