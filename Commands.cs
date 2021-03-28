using System;
using Microsoft.Xna.Framework;
using TShockAPI;
using Terraria;
using System.Collections.Generic;
using System.Linq;

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

        public void MainCommand(CommandArgs args)
        {
            string cmd = "help";
            if (args.Parameters.Count > 0)
            {
                cmd = args.Parameters[0].ToLower();
            }
            switch (cmd)
            {
                case "set":
                    SetWarpplate(args);
                    break;

                case "del":
                case "delete":
                case "remove":
                    DeleteWarpplate(args);
                    break;

                case "setdest":
                case "setdestination":
                    SetWarpplateDestination(args);
                    break;

                case "deldest":
                case "deletedestination":
                    RemoveWarpplateDestination(args);
                    break;

                case "info":
                    WarpplateInformation(args);
                    break;

                case "list":
                    ListWarpplates(args);
                    break;

                case "allow":
                case "toggle":
                    WarpplateAllow(args);
                    break;

                case "reload":
                    ReloadWarpplates(args);
                    break;

                case "delay":
                    SetWarpplateDelay(args);
                    break;

                case "width":
                    SetWarpplateWidth(args);
                    break;

                case "height":
                    SetWarpplateHeight(args);
                    break;

                case "resize":
                case "size":
                    SetWarpplateSize(args);
                    break;

                case "label":
                case "display":
                    SetWarpplateLabel(args);
                    break;

                case "dim":
                case "dimension":
                    SetWarpplateDimension(args);
                    break;

                case "help":
                default:
                    {
                        int pageNumber = 1;
                        int pageParamIndex = 0;
                        if (args.Parameters.Count > 1)
                            pageParamIndex = 1;
                        if (!PaginationTools.TryParsePageNumber(args.Parameters, pageParamIndex, args.Player, out pageNumber))
                            return;

                        List<String> lines = new List<String>{
                            "set <name> - Set new warpplate at your position.",
                            "delete <name> - Deletes the given warpplate.",
                            "setdest <name> <destination> - Set destination for the given warpplate.",
                            "deldest <name> - Delete the destination for the given warpplate.",
                            "info <name> - Display info for the given warpplate.",
                            "list - List all warpplates in the world.",
                            "toggle - Enable/Disable activating warpplates for yourself.",
                            "reload - Reload warpplate information from database.",
                            "delay [<name>] <delay in seconds> - Set delay for warpplate.",
                            "width [<name>] <width in blocks> - Set width for warpplate.",
                            "height [<name>] <height in blocks> - Set height for warpplate.",
                            "resize [<name>] <width> <height> - Resize dimensions for warpplate.",
                            "label [<name>] <label name> - Set the label name for the warpplate"
                        };
                        if (args.Player.HasPermission("warpplate.setdimensional"))
                        {
                            lines.Add("dim [<name>] <dimension name> - Set dimension destination for the warpplate.");
                        }

                        PaginationTools.SendPage(
                            args.Player, pageNumber, lines,
                            new PaginationTools.Settings
                            {
                                HeaderFormat = "Warpplate Sub-Commands ({0}/{1}):",
                                FooterFormat = "Type {0}wp {{0}} for more sub-commands.".SFormat(TShock.Config.CommandSpecifier)
                            }
                        );
                        break;
                    }
            }
        }

        /// <summary>
        /// Updates or gives a warpplate a delay
        /// </summary>
        /// <param name="args">The command arguments</param>
        public async void SetWarpplateDelay(CommandArgs args)
        {
            // Use the given warpplate name or find the warpplate they are standing in
            string region = "";
            if (args.Parameters.Count == 3)
                region = args.Parameters[1];
            else if (args.Parameters.Count == 2)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper synatx: /wp delay [<warpplate name>] <delay in seconds>");
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
            if (args.Parameters.Count == 3)
                region = args.Parameters[1];
            else if (args.Parameters.Count == 2)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp width [<warpplate name>] <width in blocks>");
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
            if (args.Parameters.Count == 3)
                region = args.Parameters[1];
            else if (args.Parameters.Count == 2)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp label [<warpplate name>] <label>");
                args.Player.SendErrorMessage("Type /wp label [<warpplate name>] \"\" to set label to default (warpplate name)");
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
            if (args.Parameters.Count == 3)
                region = args.Parameters[1];
            else if (args.Parameters.Count == 2)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp dim [<warpplate name>] <dimension name>");
                args.Player.SendErrorMessage("Type /wp dim [<warpplate name>] \"\" to set dimension to none");
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
            if (args.Parameters.Count == 3)
                region = args.Parameters[1];
            else if (args.Parameters.Count == 2)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp height [<warpplate name>] <height in blocks>");
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
            if (args.Parameters.Count == 4)
                region = args.Parameters[1];
            else if (args.Parameters.Count == 3)
                region = Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY);
            else
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp resize [<warpplate name>] <width> <height>");
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
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp set <warpplate name>");
                return;
            }

            if (Manager.InAreaWarpplateName(args.Player.TileX, args.Player.TileY) != null)
            {
                args.Player.SendErrorMessage("There is already a Warpplate located here. Find a new place");
                return;
            }

            string regionName = String.Join(" ", args.Parameters.Skip(1));
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
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp delete <warpplate name>");
                return;
            }

            string regionName = String.Join(" ", args.Parameters.Skip(1));
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
            if (args.Parameters.Count < 3)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp setdest <Warpplate Name> <Name of Destination Warpplate>");
                return;
            }
            if (await Manager.SetDestination(args.Parameters[1], args.Parameters[2]))
            {
                args.Player.SendInfoMessage("Destination " + args.Parameters[2] + " added to Warpplate " + args.Parameters[1]);
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
            if (args.Parameters.Count < 2)
            {
                args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /wp delete <Warpplate Name>");
                return;
            }
            if (await Manager.RemoveDestination(args.Parameters[1]))
            {
                args.Player.SendInfoMessage("Removed Destination From Warpplate " + args.Parameters[1]);
            }
            else
                args.Player.SendErrorMessage("Could not find specified Warpplate or destination");
        }

        public void WarpplateInformation(CommandArgs args)
        {
            string warpplateName = "";
            if (args.Parameters.Count > 1)
            {
                warpplateName = string.Join(" ", args.Parameters.Skip(1));
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
                    + "; Destination: " + warpplate.GetDestinationOrDefault() + ";", Color.HotPink);
                args.Player.SendMessage("X: " + warpplate.WarpplatePos.X + "; Y: " + warpplate.WarpplatePos.Y +
                    "; W: " + warpplate.Area.Width + "; H: " + warpplate.Area.Height + "; Delay: " + (warpplate.Delay - 1), Color.HotPink);
            }
        }

        public void ListWarpplates(CommandArgs args)
        {
            int pageNumber;
            if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out pageNumber))
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
            args.Player.SendSuccessMessage("Warpplates reloaded!");
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
