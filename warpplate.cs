using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace AdvancedWarpplates
{
    /// <summary>
    /// Contains the properties of a warpplate in the world
    /// </summary>
    public class Warpplate
    {
        public Rectangle Area { get; set; }
        public Vector2 WarpplatePos { get; set; }
        public string Name { get; set; }
        public string Destination { get; set; }
        public string WorldID { get; set; }
        public List<int> AllowedIDs { get; set; }
        public int Type { get; set; }
        public int Delay { get; set; }
        public string Label
        {
            get { return _label == null ? Name : _label; }
            set { _label = value; }
        }

        private string _label;

        public Warpplate()
        {
            WarpplatePos = Vector2.Zero;
            Area = Rectangle.Empty;
            Name = string.Empty;
            Destination = string.Empty;
            WorldID = string.Empty;
            AllowedIDs = new List<int>();
            Type = 0;
        }

        public Warpplate(Vector2 warpplatepos, Rectangle Warpplate, string name, string destination, bool disablebuild, string WarpplateWorldIDz, string label, int type)
            : this()
        {
            WarpplatePos = warpplatepos;
            Area = Warpplate;
            Name = name;
            Label = label;
            Destination = destination;
            WorldID = WarpplateWorldIDz;
            Delay = 4;
            Type = type;
        }

        /// <summary>
        /// Checks whether a given point is inside the warpplate's area
        /// </summary>
        /// <param name="point">The point to check</param>
        /// <returns>Whether a given point is inside the warpplate's area</returns>
        public bool InArea(Rectangle point)
        {
            if (Area.Contains(point.X, point.Y))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the destination name, or if there is no destination gets the default "(none)"
        /// </summary>
        /// <returns>Destination name or default</returns>
        public string GetDestinationOrDefault()
        {
            return Destination == null ? "(none)" : Destination;
        }

        /// <summary>
        /// Gets the label, or if there is no label gets the default
        /// </summary>
        /// <returns>Label or default label</returns>
        public string GetLabelOrDefault()
        {
            return _label == null ? Name + " (default)" : _label;
        }
    }
}
