using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using MySql.Data.MySqlClient;
using TShockAPI.DB;
using TShockAPI;
using System.ComponentModel;
using System.Linq;
using System.Web;
using TerrariaApi.Server;

namespace PluginTemplate
{
    [ApiVersion(1, 16)]
    public class WarpplatePlugin : TerrariaPlugin
    {
        public static List<Player> Players = new List<Player>();
        public static WarpplateManager Warpplates;

        public override string Name
        {
            get { return "Warpplate"; }
        }
        public override string Author
        {
            get { return "Created by DarkunderdoG, modified by 2.0"; }
        }
        public override string Description
        {
            get { return "Warpplate"; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public override void Initialize()
        {
            Warpplates = new WarpplateManager(TShock.DB);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInit);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInit);
                ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            }

            base.Dispose(disposing);
        }

        private void OnPostInit(EventArgs args)
        {
            Warpplates.ReloadAllWarpplates();
        }

        public WarpplatePlugin(Main game)
            : base(game)
        {
            Order = 1;
        }

        public void OnInitialize(EventArgs args)
        {
            Commands.ChatCommands.Add(new Command("setwarpplate", setwarpplate, "swp"));
            Commands.ChatCommands.Add(new Command("setwarpplate", delwarpplate, "dwp"));
            Commands.ChatCommands.Add(new Command("setwarpplate", warpplatedest, "swpd"));
            Commands.ChatCommands.Add(new Command("setwarpplate", removeplatedest, "rwpd"));
            Commands.ChatCommands.Add(new Command("setwarpplate", wpi, "wpi"));
            Commands.ChatCommands.Add(new Command("setwarpplate", warpallow, "wpallow"));
            Commands.ChatCommands.Add(new Command("setwarpplate", reloadwarp, "reloadwarp"));
            Commands.ChatCommands.Add(new Command("setwarpplate", setwarpplatedelay, "swpdl"));
            Commands.ChatCommands.Add(new Command("setwarpplate", setwarpplatewidth, "swpw"));
            Commands.ChatCommands.Add(new Command("setwarpplate", setwarpplateheight, "swph"));
            Commands.ChatCommands.Add(new Command("setwarpplate", setwarpplatesize, "swps"));
            Commands.ChatCommands.Add(new Command("setwarpplate", setwarpplatelabel, "swpl"));
        }
        
        public void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            lock (Players)
                Players.Add(new Player(args.Who));
        }

        public class Player
        {
            public int Index { get; set; }
            public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
            public int warpplatetime { get; set; }
            public bool warpplateuse { get; set; }
            public bool warped { get; set; }
            public int warpcooldown { get; set; }
            public Player(int index)
            {
                Index = index;
                warpplatetime = 0;
                warpplateuse = true;
                warped = false;
                warpcooldown = 0;
            }
        }

        private DateTime LastCheck = DateTime.UtcNow;

        private void OnUpdate(EventArgs args)
        {
            if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 1)
            {
                LastCheck = DateTime.UtcNow;
                lock (Players)
                    foreach (Player player in Players)
                    {
                        if (player != null && player.TSPlayer != null)
                        {
                            if (player.TSPlayer.Group.HasPermission("warpplate") && player.warpplateuse)
                            {
                                if (player.warpcooldown != 0)
                                {
                                    player.warpcooldown--;
                                    continue;
                                }
                                string region = Warpplates.InAreaWarpplateName(player.TSPlayer.TileX, player.TSPlayer.TileY);
                                if (region == null || region == "")
                                {
                                    player.warpplatetime = 0;
                                    player.warped = false;
                                }
                                else
                                {
                                    if (player.warped)
                                        continue;
                                    var warpplateinfo = Warpplates.FindWarpplate(region);
                                    var warp = Warpplates.FindWarpplate(warpplateinfo.WarpDest);
                                    if (warp != null)
                                    {
                                        player.warpplatetime++;
                                        if ((warpplateinfo.Delay - player.warpplatetime) > 0)
                                            player.TSPlayer.SendMessage("You Will Be Warped To " + Warpplates.GetLabel(warpplateinfo.WarpDest) + " in " + (warpplateinfo.Delay - player.warpplatetime) + " Seconds");
                                        else
                                        {
                                            if (player.TSPlayer.Teleport((int)warp.WarpplatePos.X + 2, (int)warp.WarpplatePos.Y + 3))
                                                player.TSPlayer.SendMessage("You Have Been Warped To " + Warpplates.GetLabel(warpplateinfo.WarpDest) + " via a Warpplate");
                                            player.warpplatetime = 0;
                                            player.warped = true;
                                            player.warpcooldown = 3;
                                        }                                        
                                    }
                                }
                            }
                        }
                    }
            }
        }

        private void OnLeave(LeaveEventArgs args)
        {
            lock (Players)
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].Index == args.Who)
                    {
                        Players.RemoveAt(i);
                        break; //Found the player, break.
                    }
                }
            }
        }

        private static int GetPlayerIndex(int ply)
        {
            lock (Players)
            {
                int index = -1;
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].Index == ply)
                        index = i;
                }
                return index;
            }
        }

        private static void setwarpplatedelay(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /swpd [<warpplate name>] <delay in seconds>", Color.Red);
                args.Player.SendMessage("Set 0 for immediate warp", Color.Red);
                return;
            }
            Warpplate wp = Warpplates.FindWarpplate(region);
            if (wp == null)
            {
                args.Player.SendMessage("No such warpplate", Color.Red);
                return;
            }
            int Delay;
            if (Int32.TryParse(args.Parameters[args.Parameters.Count - 1], out Delay))
            {
                wp.Delay = Delay + 1;
                if (Warpplates.UpdateWarpplate(wp.Name))
                    args.Player.SendMessage(String.Format("Set delay of {0} to {1} seconds", wp.Name, Delay), Color.Green);
                else
                    args.Player.SendMessage("Something went wrong", Color.Red);
            }
            else
                args.Player.SendMessage("Bad number specified", Color.Red);
        }

        private static void setwarpplatewidth(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /swpw [<warpplate name>] <width in blocks>", Color.Red);
                return;
            }
            Warpplate wp = Warpplates.FindWarpplate(region);
            if (wp == null)
            {
                args.Player.SendMessage("No such warpplate", Color.Red);
                return;
            }
            int Width;
            if (Int32.TryParse(args.Parameters[args.Parameters.Count - 1], out Width))
            {
                Rectangle r;
                r = wp.Area;
                r.Width = Width; 
                wp.Area = r;
                if (Warpplates.UpdateWarpplate(wp.Name))
                    args.Player.SendMessage(String.Format("Set width of {0} to {1} blocks", wp.Name, Width), Color.Green);
                else
                    args.Player.SendMessage("Something went wrong", Color.Red);
            }
            else
                args.Player.SendMessage("Bad number specified", Color.Red);
        }

        private static void setwarpplatelabel(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /swpl [<warpplate name>] <label>", Color.Red);
                args.Player.SendMessage("Type /swpl [<warpplate name>] \"\" to set label to default (warpplate name)", Color.Red);
                return;
            }
            Warpplate wp = Warpplates.FindWarpplate(region);
            if (wp == null)
            {
                args.Player.SendMessage("No such warpplate", Color.Red);
                return;
            }
            string label = args.Parameters[args.Parameters.Count - 1];
            wp.Label = label;
            if (Warpplates.UpdateWarpplate(wp.Name))
                args.Player.SendMessage(String.Format("Set label of {0} to {1}", wp.Name, D(wp)), Color.Green);
            else
                args.Player.SendMessage("Something went wrong", Color.Red);
        }

        private static void setwarpplateheight(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /swph [<warpplate name>] <height in blocks>", Color.Red);
                return;
            }
            Warpplate wp = Warpplates.FindWarpplate(region);
            if (wp == null)
            {
                args.Player.SendMessage("No such warpplate", Color.Red);
                return;
            }
            int Height;
            if (Int32.TryParse(args.Parameters[args.Parameters.Count - 1], out Height))
            {
                Rectangle r;
                r = wp.Area;
                r.Height = Height;
                wp.Area = r;
                Warpplates.UpdateWarpplate(wp.Name);
                if (Warpplates.UpdateWarpplate(wp.Name))
                    args.Player.SendMessage(String.Format("Set height of {0} to {1} blocks", wp.Name, Height), Color.Green);
                else
                    args.Player.SendMessage("Something went wrong", Color.Red);
            }
            else
                args.Player.SendMessage("Bad number specified", Color.Red);
        }

        private static void setwarpplatesize(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 3)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 2)
                region = Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /swps [<warpplate name>] <width> <height>", Color.Red);
                return;
            }
            Warpplate wp = Warpplates.FindWarpplate(region);
            if (wp == null)
            {
                args.Player.SendMessage("No such warpplate", Color.Red);
                return;
            }
            int Width, Height;
            if (Int32.TryParse(args.Parameters[args.Parameters.Count - 2], out Width) &&
                Int32.TryParse(args.Parameters[args.Parameters.Count - 1], out Height))
            {
                Rectangle r;
                r = wp.Area;
                r.Width = Width;
                r.Height = Height;
                wp.Area = r;
                Warpplates.UpdateWarpplate(wp.Name);
                if (Warpplates.UpdateWarpplate(wp.Name))
                    args.Player.SendMessage(String.Format("Set size of {0} to {1}x{2}", wp.Name, Width, Height), Color.Green);
                else
                    args.Player.SendMessage("Something went wrong", Color.Red);
            }
            else
                args.Player.SendMessage("Bad number specified", Color.Red);
        }

        private static void setwarpplate(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /swp <warpplate name>", Color.Red);
                return;
            }
            if (Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY) != null)
            {
                args.Player.SendMessage("There Is Already A Warpplate Located Here. Find A New Place", Color.Red);
                return;
            }
            string regionName = String.Join(" ", args.Parameters);
            var x = ((((int)args.Player.X) / 16) - 1);
            var y = (((int)args.Player.Y) / 16);
            var width = 2;
            var height = 3;
            if (Warpplates.AddWarpplate(x, y, width, height, regionName, "", Main.worldID.ToString()))
            {
                args.Player.SendMessage("Warpplate Created: " + regionName, Color.Yellow);
                //args.Player.SendMessage("Now Set The Warpplate Destination By Using /swpd", Color.Yellow);
                Warpplates.ReloadAllWarpplates();
            }
            else
            {
                args.Player.SendMessage("Warpplate Already Created: " + regionName + " already exists", Color.Red);
            }
        }

        private static void delwarpplate(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /dwp <warpplate name>", Color.Red);
                return;
            }
            string regionName = String.Join(" ", args.Parameters);
            if (Warpplates.DeleteWarpplate(regionName))
            {
                args.Player.SendMessage("Deleted Warpplate: " + regionName, Color.Yellow);
                Warpplates.ReloadAllWarpplates();
            }
            else
                args.Player.SendMessage("Could not find specified Warpplate", Color.Red);
        }

        private static void warpplatedest(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /swpd <Warpplate Name> <Name Of Destination Warpplate>", Color.Red);
                return;
            }
            if (Warpplates.adddestination(args.Parameters[0], args.Parameters[1]))
            {
                args.Player.SendMessage("Destination " + args.Parameters[1] + " Added To Warpplate " + args.Parameters[0], Color.Yellow);
                Warpplates.ReloadAllWarpplates();
            }
            else
                args.Player.SendMessage("Could not find specified Warpplate or destination", Color.Red);
        }

        private static void removeplatedest(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /rwpd <Warpplate Name>", Color.Red);
                return;
            }
            if (Warpplates.removedestination(args.Parameters[0]))
            {
                args.Player.SendMessage("Removed Destination From Warpplate " + args.Parameters[0], Color.Yellow);
                Warpplates.ReloadAllWarpplates();
            }
            else
                args.Player.SendMessage("Could not find specified Warpplate or destination", Color.Red);
        }

        private static string S(string s)
        {
            return String.IsNullOrEmpty(s) ? "(none)" : s;
        }

        private static string D(Warpplate wp)
        {
            return String.IsNullOrEmpty(wp.Label) ? wp.Name + " (default)" : wp.Label;
        }

        private static void wpi(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count > 0)
                region = String.Join(" ", args.Parameters);
            else
                region = Warpplates.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            var warpplateinfo = Warpplates.FindWarpplate(region);
            if (warpplateinfo == null)
                args.Player.SendMessage("No Such Warpplate", Color.Red);
            else
            {
                args.Player.SendMessage("Name: " + warpplateinfo.Name + "; Label: " + D(warpplateinfo) 
                    + "Destination: " + S(warpplateinfo.WarpDest) + ";", Color.HotPink);
                args.Player.SendMessage("X: " + warpplateinfo.WarpplatePos.X + "; Y: " + warpplateinfo.WarpplatePos.Y + 
                    "; W: " + warpplateinfo.Area.Width + "; H: " + warpplateinfo.Area.Height + "; Delay: " + (warpplateinfo.Delay - 1), Color.HotPink);
            }
        }

        private static void reloadwarp(CommandArgs args)
        {
            Warpplates.ReloadAllWarpplates();
        }

        private static void warpallow(CommandArgs args)
        {
            if (!Players[GetPlayerIndex(args.Player.Index)].warpplateuse)
                args.Player.SendMessage("Warpplates Are Now Turned On For You");
            if (Players[GetPlayerIndex(args.Player.Index)].warpplateuse)
                args.Player.SendMessage("Warpplates Are Now Turned Off For You");
            Players[GetPlayerIndex(args.Player.Index)].warpplateuse = !Players[GetPlayerIndex(args.Player.Index)].warpplateuse;
        }

    }
}