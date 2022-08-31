using TShockAPI;

namespace AdvancedWarpplates
{
    /// <summary>
    /// Deals with carrying out checks to see if a player is standing on a warpplate
    /// </summary>
    public class Player
    {
        public int Index { get; set; }
        public TSPlayer TSPlayer { get { return TShock.Players[Index]; } }
        public int TimeStandingOnWarpplate { get; set; }
        public bool CanUseWarpplates { get; set; }
        public bool HasJustUsedWarpplate { get; set; }
        public int WarpplateUseCooldown { get; set; }
        private WarpplateManager Manager;

        public Player(int index, WarpplateManager manager)
        {
            Index = index;
            TimeStandingOnWarpplate = 0;
            CanUseWarpplates = true;
            HasJustUsedWarpplate = false;
            WarpplateUseCooldown = 0;
            Manager = manager;
        }

        /// <summary>
        /// Ticks down cooldown and carries out checks to see if they need to be warped by a warpplate
        /// </summary>
        public void Update()
        {
            if (WarpplateUseCooldown != 0)
            {
                WarpplateUseCooldown--;
                return;
            }

            string warpplateName = Manager.InAreaWarpplateName(TSPlayer.TileX, TSPlayer.TileY);
            if (warpplateName == null || warpplateName == "")
            {
                TimeStandingOnWarpplate = 0;
                HasJustUsedWarpplate = false;
            }
            else if(!HasJustUsedWarpplate)
            {
                CheckAndUseWarpplate(warpplateName);
            }
        }

        /// <summary>
        /// Carries out checks to see if they need to be warped by a warplate
        /// </summary>
        /// <param name="warpplateName">The warpplate to check against</param>
        private void CheckAndUseWarpplate(string warpplateName)
        {
            var warpplate = Manager.GetWarpplateByName(warpplateName);

            switch(warpplate.Type)
            {
                case 0:
                    var destinationWarpplate = Manager.GetWarpplateByName(warpplate.Destination);
                    if (destinationWarpplate == null) {
                        TShock.Log.Error($"{warpplate.Name}'s destination warpplate \"{warpplate.Destination}\" not found");
                        break;
                    }

                    CheckAndUseDestinationWarpplate(warpplate, destinationWarpplate);
                    break;
                case 1:
                    CheckAndUseDimensionWarpplate(warpplate);
                    break;
            }
        }

        /// <summary>
        /// Checks whether the user can warp and warps them to the destination of the warpplate
        /// </summary>
        /// <param name="warpplate">The warpplate they are attempting to use</param>
        /// <param name="destinationWarpplate">The destination warpplate they are attempting to go to</param>
        private void CheckAndUseDestinationWarpplate(Warpplate warpplate, Warpplate destinationWarpplate)
        {
            if (TSPlayer == null) {
                TShock.Log.ConsoleError("AdvancedWarpplates: Player is null");
                return;
            }

            if (warpplate == null) {
                TShock.Log.ConsoleError($"AdvancedWarpplates: {nameof(warpplate)} is null");
                return;
            }

            if (destinationWarpplate == null) {
                TShock.Log.ConsoleError($"AdvancedWarpplates: {nameof(destinationWarpplate)} is null");
                return;
            }

            TimeStandingOnWarpplate++;
            if ((warpplate.Delay - TimeStandingOnWarpplate) > 0)
                TSPlayer.SendInfoMessage("You will be warped to " + destinationWarpplate.Label + " in " + (warpplate.Delay - TimeStandingOnWarpplate) + " seconds");
            else
            {
                if (TSPlayer.Teleport((int)(destinationWarpplate.WarpplatePos.X * 16) + 2, (int)(destinationWarpplate.WarpplatePos.Y * 16) + 3))
                    TSPlayer.SendInfoMessage("You have been warped to " + destinationWarpplate.Label + " via a Warpplate");
                TimeStandingOnWarpplate = 0;
                HasJustUsedWarpplate = true;
                WarpplateUseCooldown = 3;
            }
        }

        /// <summary>
        /// Checks whether the user can warp and warps them to the dimension of the warpplate
        /// </summary>
        /// <param name="warpplate">The warpplate they are attempting to use</param>
        private void CheckAndUseDimensionWarpplate(Warpplate warpplate)
        {
            TimeStandingOnWarpplate++;
            if ((warpplate.Delay - TimeStandingOnWarpplate) > 0)
                TSPlayer.SendInfoMessage("Shifting to " + warpplate.Label + " in " + (warpplate.Delay - TimeStandingOnWarpplate) + " seconds");
            else
            {
                SendToDimension(warpplate.Destination);
                TimeStandingOnWarpplate = 0;
                HasJustUsedWarpplate = true;
                WarpplateUseCooldown = 3;
            }
        }

        /// <summary>
        /// Sends this player to a different Dimension (https://github.com/popstarfreas/Dimensions)
        /// </summary>
        /// <param name="dimension">The name of the dimension to send them to</param>
        private void SendToDimension(string dimension)
        {
            byte[] data = (new PacketFactory())
                        .SetType(67)
                        .PackInt16(2)
                        .PackString(dimension)
                        .GetByteData();
            TSPlayer.SendRawData(data);
        }
    }
}
