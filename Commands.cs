using System;
using Microsoft.Xna.Framework;
using TShockAPI;
using Terraria;

namespace AdvancedWarpplates
{
    /// <summary>
    /// Deals with any commands a player might execute
    /// </summary>
    public class Commands
    {
        private WarpplateManager Manager;
        private Player[] Players;

        public Commands(WarpplateManager manager, Player[] players)
        {
            Manager = manager;
            Players = players;
        }

        /// <summary>
        /// Updates or gives a warpplate a delay
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateDelay(CommandArgs args)
        {
            // Use the given warpplate name or find the warpplate they are standing in
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swpdl [<warpplate name>] <delay in seconds>");
                args.Player.SendErrorMessage("Set 0 for immediate warp");
                return;
            }

            // Check if the warpplate exists
            Warpplate warpplate = Manager.GetWarpplateByName(region);
            if (warpplate == null)
            {
                args.Player.SendErrorMessage("No such warpplate");
                return;
            }

            // Update the delay of the given warpplate
            int Delay;
            if (int.TryParse(args.Parameters[args.Parameters.Count - 1], out Delay))
            {
                warpplate.Delay = Delay + 1;
                if (await Manager.UpdateWarpplate(warpplate.Name))
                    args.Player.SendSuccessMessage("Set delay of {0} to {1} seconds", warpplate.Name, Delay);
                else
                    args.Player.SendErrorMessage("Something went wrong");
            }
            else
                args.Player.SendErrorMessage("Bad number specified");
        }

        /// <summary>
        /// Resizes a warpplate's width
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateWidth(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swpw [<warpplate name>] <width in blocks>");
                return;
            }

            Warpplate warpplate = Manager.GetWarpplateByName(region);
            if (warpplate == null)
            {
                args.Player.SendErrorMessage("No such warpplate");
                return;
            }

            int Width;
            if (Int32.TryParse(args.Parameters[args.Parameters.Count - 1], out Width))
            {
                Rectangle r;
                r = warpplate.Area;
                r.Width = Width;
                warpplate.Area = r;
                if (await Manager.UpdateWarpplate(warpplate.Name))
                    args.Player.SendSuccessMessage("Set width of {0} to {1} blocks", warpplate.Name, Width);
                else
                    args.Player.SendErrorMessage("Something went wrong");
            }
            else
                args.Player.SendErrorMessage("Invalid number: " + args.Parameters[args.Parameters.Count - 1]);
        }

        /// <summary>
        /// Updates a warpplates label
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateLabel(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swpl [<warpplate name>] <label>");
                args.Player.SendErrorMessage("Type /swpl [<warpplate name>] \"\" to set label to default (warpplate name)");
                return;
            }
            Warpplate warpplate = Manager.GetWarpplateByName(region);
            if (warpplate == null)
            {
                args.Player.SendErrorMessage("No such warpplate");
                return;
            }
            string label = args.Parameters[args.Parameters.Count - 1];
            warpplate.Label = label;
            if (await Manager.UpdateWarpplate(warpplate.Name))
                args.Player.SendSuccessMessage(String.Format("Set label of {0} to {1}", warpplate.Name, warpplate.GetLabelOrDefault()));
            else
                args.Player.SendErrorMessage("Something went wrong");
        }

        /// <summary>
        /// Updates a warpplates dimension destination
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateDimension(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swpdim [<warpplate name>] <dimension name>");
                args.Player.SendErrorMessage("Type /swpdim [<warpplate name>] \"\" to set dimension to none");
                return;
            }

            Warpplate warpplate = Manager.GetWarpplateByName(region);
            if (warpplate == null)
            {
                args.Player.SendErrorMessage("No such warpplate");
                return;
            }

            // Get Dimension Name from parameters
            string dimension = args.Parameters[args.Parameters.Count - 1];
            warpplate.Destination = dimension;

            // Set type to 0 (non-dimensional) if the specified name was empty
            if (dimension.Length == 0)
            {
                warpplate.Type = 0;
            }
            else
            {
                warpplate.Type = 1;
            }

            // Update warpplate in DB
            if (await Manager.SetDestination(warpplate.Name, dimension, warpplate.Type))
            {
                if (warpplate.Type == 0)
                {
                    args.Player.SendSuccessMessage(String.Format("Removed Dimension Destination from {0}", warpplate.Name));
                }
                else
                {
                    args.Player.SendSuccessMessage(String.Format("Set Dimension Destination of {0} to {1}", warpplate.Name, dimension));
                }
            }
            else
            {
                args.Player.SendErrorMessage("Something went wrong");
            }
        }

        /// <summary>
        /// Resizes a warpplates height
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateHeight(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 2)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 1)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swph [<warpplate name>] <height in blocks>");
                return;
            }

            Warpplate warpplate = Manager.GetWarpplateByName(region);
            if (warpplate == null)
            {
                args.Player.SendErrorMessage("No such warpplate");
                return;
            }

            int Height;
            if (Int32.TryParse(args.Parameters[args.Parameters.Count - 1], out Height))
            {
                Rectangle area;
                area = warpplate.Area;
                area.Height = Height;
                warpplate.Area = area;

                await Manager.UpdateWarpplate(warpplate.Name);

                if (await Manager.UpdateWarpplate(warpplate.Name))
                {
                    args.Player.SendSuccessMessage("Set height of {0} to {1} blocks", warpplate.Name, Height);
                }
                else
                {
                    args.Player.SendErrorMessage("Something went wrong");
                }
            }
            else
            {
                args.Player.SendErrorMessage("Bad number specified");
            }
        }

        /// <summary>
        /// Resizes a warpplate
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateSize(CommandArgs args)
        {
            string region = "";
            if (args.Parameters.Count == 3)
                region = args.Parameters[0];
            else if (args.Parameters.Count == 2)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swps [<warpplate name>] <width> <height>");
                return;
            }

            Warpplate warpplate = Manager.GetWarpplateByName(region);
            if (warpplate == null)
            {
                args.Player.SendErrorMessage("No such warpplate");
                return;
            }

            int Width, Height;
            if (Int32.TryParse(args.Parameters[args.Parameters.Count - 2], out Width) &&
                Int32.TryParse(args.Parameters[args.Parameters.Count - 1], out Height))
            {
                Rectangle area;
                area = warpplate.Area;
                area.Width = Width;
                area.Height = Height;
                warpplate.Area = area;

                await Manager.UpdateWarpplate(warpplate.Name);

                if (await Manager.UpdateWarpplate(warpplate.Name))
                {
                    args.Player.SendSuccessMessage("Set size of {0} to {1}x{2}", warpplate.Name, Width, Height);
                }
                else
                {
                    args.Player.SendErrorMessage("Something went wrong");
                }
            }
            else
                args.Player.SendErrorMessage("Bad number specified");
        }

        /// <summary>
        /// Adds a new warpplate with a given name
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplate(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swp <warpplate name>");
                return;
            }

            if (Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY) != null)
            {
                args.Player.SendErrorMessage("There is already a Warpplate located here. Find a new place");
                return;
            }

            string regionName = String.Join(" ", args.Parameters);
            var x = ((((int)args.Player.X) / 16) - 1);
            var y = (((int)args.Player.Y) / 16);
            var width = 2;
            var height = 3;

            if (await Manager.AddWarpplate(x, y, width, height, regionName, "", Main.worldID.ToString()))
            {
                args.Player.SendSuccessMessage("Warpplate created: " + regionName);
            }
            else
            {
                args.Player.SendErrorMessage("Warpplate already created: " + regionName + " already exists");
            }
        }

        /// <summary>
        /// Deletes a warpplate with a given name
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void DeleteWarpplate(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /dwp <warpplate name>");
                return;
            }

            string regionName = String.Join(" ", args.Parameters);
            if (await Manager.DeleteWarpplate(regionName))
            {
                args.Player.SendInfoMessage("Deleted Warpplate: " + regionName);
            }
            else
                args.Player.SendErrorMessage("Could not find specified Warpplate");
        }

        /// <summary>
        /// Sets the destination of a warpplate
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateDestination(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /swpd <Warpplate Name> <Name Of Destination Warpplate>");
                return;
            }
            if (await Manager.SetDestination(args.Parameters[0], args.Parameters[1]))
            {
                args.Player.SendInfoMessage("Destination " + args.Parameters[1] + " added to Warpplate " + args.Parameters[0]);
            }
            else
                args.Player.SendErrorMessage("Could not find specified Warpplate or destination");
        }

        /// <summary>
        /// Removes a warpplates destination
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void RemoveWarpplateDestination(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /rwpd <Warpplate Name>");
                return;
            }
            if (await Manager.RemoveDestination(args.Parameters[0]))
            {
                args.Player.SendInfoMessage("Removed Destination From Warpplate " + args.Parameters[0]);
            }
            else
                args.Player.SendErrorMessage("Could not find specified Warpplate or destination");
        }

        public void WarpplateInformation(CommandArgs args)
        {
            string warpplateName = "";
            if (args.Parameters.Count > 0)
            {
                warpplateName = string.Join(" ", args.Parameters);
            }
            else
            {
                warpplateName = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            }

            var warpplate = Manager.GetWarpplateByName(warpplateName);
            if (warpplate == null)
            {
                args.Player.SendErrorMessage("No such Warpplate");
            }
            else
            {
                args.Player.SendMessage("Name: " + warpplate.Name + "; Label: " + warpplate.GetLabelOrDefault()
                    + "Destination: " + warpplate.GetDestinationOrDefault() + ";", Color.HotPink);
                args.Player.SendMessage("X: " + warpplate.WarpplatePos.X + "; Y: " + warpplate.WarpplatePos.Y +
                    "; W: " + warpplate.Area.Width + "; H: " + warpplate.Area.Height + "; Delay: " + (warpplate.Delay - 1), Color.HotPink);
            }
        }

        public void ListWarpplates(CommandArgs args)
        {
            int pageNumber;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 0, args.Player, out pageNumber))
                return;

            var wpNames = Manager.ListAllWarpplates();

            PaginationTools.SendPage(args.Player, pageNumber, PaginationTools.BuildLinesFromTerms(wpNames),
                new PaginationTools.Settings
                {
                    HeaderFormat = "Warpplates ({0}/{1}):",
                    FooterFormat = "Type {0}lwp {{0}} for more.".SFormat(TShock.Config.CommandSpecifier),
                    NothingToDisplayString = "There are currently no warpplates defined."
                });
        }

        public async void ReloadWarpplates(CommandArgs args)
        {
            await Manager.ReloadAllWarpplates();
        }

        public void WarpplateAllow(CommandArgs args)
        {
            if (!Players[args.Player.Index].CanUseWarpplates)
            {
                args.Player.SendSuccessMessage("Warpplates are now turned on for you");
            }
            else
            {
                args.Player.SendSuccessMessage("Warpplates are now turned off for you");
            }
                
            Players[args.Player.Index].CanUseWarpplates = !Players[args.Player.Index].CanUseWarpplates;
        }
    }
}
