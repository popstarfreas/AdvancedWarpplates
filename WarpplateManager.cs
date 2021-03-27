using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TShockAPI;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace AdvancedWarpplates
{
    /// <summary>
    /// Deals with keeping track of and providing access to the warpplates of the current world
    /// </summary>
    public class WarpplateManager
    {
        private WarpplateDB Database;
        private List<Warpplate> Warpplates = new List<Warpplate>();

        public WarpplateManager(IDbConnection db)
        {
            Database = new WarpplateDB(db);
        }

        /// <summary>
        /// Gets a Warpplate with the given name
        /// </summary>
        /// <param name="name">The name of the warpplate to find</param>
        /// <returns>The warpplate found or null</returns>
        public Warpplate GetWarpplateByName(string name)
        {
            return Warpplates.FirstOrDefault(warpplate => warpplate.Name.Equals(name));
        }

        /// <summary>
        /// Gets if there is a warpplate  that surrounds the given co-ordinates
        /// </summary>
        /// <param name="x">The x co-ordinate</param>
        /// <param name="y">The y co-ordinate</param>
        /// <returns>Whether or not there is a warpplate surrounding the given co-ordinates</returns>
        public bool InArea(int x, int y)
        {
            foreach (Warpplate Warpplate in Warpplates)
            {
                if (x >= Warpplate.Area.Left && x <= Warpplate.Area.Right &&
                    y >= Warpplate.Area.Top && y <= Warpplate.Area.Bottom)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the name of the warpplate that surrounds the given co-ordinates
        /// </summary>
        /// <param name="x">The x co-ordinate</param>
        /// <param name="y">The y co-ordinate</param>
        /// <returns>The name of the warpplate or null</returns>
        public string InAreaWarpplateName(int x, int y)
        {
            foreach (Warpplate Warpplate in Warpplates)
            {
                if (x >= Warpplate.Area.Left && x <= Warpplate.Area.Right &&
                    y >= Warpplate.Area.Top && y <= Warpplate.Area.Bottom)
                {
                    return Warpplate.Name;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all the Warpplates names from world
        /// </summary>
        /// <param name="worldid">World name to get Warpplates from</param>
        /// <returns>List of Warpplates with only their names</returns>
        public List<String> ListAllWarpplates()
        {
            /*var WarpplatesTemp = new List<Warpplate>();
            try
            {
                foreach (Warpplate wp in Warpplates)
                {
                    WarpplatesTemp.Add(new Warpplate { Name = wp.Name });
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
            return WarpplatesTemp;
            */

            var warpplateNames = new List<String>();
            try
            {
                foreach(Warpplate wp in Warpplates)
                {
                    warpplateNames.Add(wp.Name);
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }
            return warpplateNames;
        }

        /// <summary>
        /// Gets the label of the given warpplate
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetLabel(String name)
        {
            Warpplate warpplate = GetWarpplateByName(name);
            if (String.IsNullOrEmpty(warpplate.Label))
                return name;
            else
                return warpplate.Label;
        }

        /// <summary>
        /// Adds a warpplate to the current world
        /// </summary>
        /// <param name="x">The x co-ordinate of the warpplate</param>
        /// <param name="y">The y co-orindate of the warpplate</param>
        /// <param name="width">The width of the warpplate</param>
        /// <param name="height">The height of the warpplate</param>
        /// <param name="warpplateName">The name of the warpplate</param>
        /// <param name="destinationName">The name of the destination warpplate</param>
        /// <param name="worldid">The id of the world the warpplate is to be in</param>
        /// <returns>Whether the warpplate was added successfully</returns>
        public async Task<bool> AddWarpplate(int x, int y, int width, int height, string warpplateName, string destinationName, string worldid)
        {
            // If warpplate does not exist, then it can be added
            if (GetWarpplateByName(warpplateName) == null)
            {
                bool success = await Database.AddWarpplate(x, y, width, height, warpplateName, destinationName, worldid);
                if (success)
                {
                    Warpplates.Add(new Warpplate(new Vector2(x, y), new Rectangle(x, y, width, height), warpplateName, worldid, true, destinationName, "", 0));
                }

                return success;
            }

            return false;
        }

        /// <summary>
        /// Deletes a warpplate from the world
        /// </summary>
        /// <param name="name">The name of the warpplate to delete</param>
        /// <returns>Whether the warpplate was deleted successfully</returns>
        public async Task<bool> DeleteWarpplate(string name)
        {
            Warpplate warpplate = GetWarpplateByName(name);
            if (warpplate != null)
            {
                bool success = await Database.DeleteWarpplate(warpplate);
                if (success)
                {
                    Warpplates.Remove(warpplate);
                }

                return success;
            }
            return false;
        }

        /// <summary>
        /// Updates a warpplates information
        /// </summary>
        /// <param name="name">The name of the warpplate to update</param>
        /// <returns>Whether or not the update was successful</returns>
        public async Task<bool> UpdateWarpplate(string name)
        {
            var warpplate = GetWarpplateByName(name);
            if (warpplate != null)
            {
                return await Database.UpdateWarpplate(warpplate);
            }
            return false;
        }

        /// <summary>
        /// Removes a warpplates destination
        /// </summary>
        /// <param name="name">The name of the warpplate to remove the destination from</param>
        /// <returns>Whether or not the removal of destination was successful</returns>
        public async Task<bool> RemoveDestination(string warpplateName)
        {
            Warpplate warpplate = GetWarpplateByName(warpplateName);
            if (warpplate != null)
            {
                bool success = await Database.RemoveDestination(warpplate);
                if (success)
                {
                    warpplate.Destination = "";
                }

                return success;
            }

            return false;
        }

        /// <summary>
        /// Sets a warpplates destination
        /// </summary>
        /// <param name="name">The name of the warpplate to set the destination of</param>
        /// <returns>Whether or not setting the destination was successful</returns>
        public async Task<bool> SetDestination(string warpplateName, string warpplateDestination, int type = 0)
        {
            Warpplate warpplate = GetWarpplateByName(warpplateName);
            if (warpplate != null)
            {
                return await Database.SetDestination(warpplate, warpplateDestination, type);
            }

            return false;
        }

        /// <summary>
        /// Completely clears all warpplates and reloads them and their information from the database
        /// </summary>
        /// <returns>void</returns>
        public async Task ReloadAllWarpplates()
        {
            Warpplates.Clear();
            Warpplates = await Database.ReloadAllWarpplates();
        }
    }
}
