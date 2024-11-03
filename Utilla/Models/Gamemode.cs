using System;
using System.Linq;

namespace Utilla.Models
{
    /// <summary>
    /// The base gamemode for a gamemode to inherit.
    /// </summary>
    /// <remarks>
    /// None should not be used from an external program.
    /// </remarks>
    public enum BaseGamemode
    {
        None,
        Infection,
        Casual,
        Hunt,
        Paintbrawl,
        Ambush,
        Ghost,
        Guardian
    }

    public class Gamemode
    {
        public const string GamemodePrefix = "MODDED_";

        /// <summary>
        /// The title of the Gamemode visible through the gamemode selector
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// The internal ID of the Gamemode
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// The GamemodeString used in the CustomProperties of the Room
        /// </summary>
        public string GamemodeString { get; }

        /// <summary>
        /// The BaseGamemode being inherited
        /// </summary>
        public BaseGamemode BaseGamemode { get; }
        public Type GameManager { get; }

        public Gamemode(string id, string displayName, BaseGamemode baseGamemode = BaseGamemode.None)
        {
            ID = id;

            DisplayName = displayName;

            BaseGamemode = baseGamemode;

            GamemodeString = (ID.Contains(GamemodePrefix) ? string.Empty : GamemodePrefix) + ID + (baseGamemode == BaseGamemode.None || Enum.GetNames(typeof(BaseGamemode)).Any(gm => ID.ToUpper().Contains(gm.ToUpper())) ? string.Empty : baseGamemode.ToString().ToUpper());
        }

        public Gamemode(string id, string displayName, Type gameManager)
        {
            this.ID = id;
            this.DisplayName = displayName;
            this.BaseGamemode = BaseGamemode.None;
            this.GameManager = gameManager;

            GamemodeString = GamemodePrefix + ID;
        }

        /// <remarks>This should only be used interally to create base game gamemodes</remarks>
        internal Gamemode(string id, string displayName)
        {
            this.ID = id;
            this.DisplayName = displayName;
            this.BaseGamemode = BaseGamemode.None;

            GamemodeString = ID;
        }

        public static implicit operator ModeSelectButtonInfoData(Gamemode gamemode)
        {
            return new ModeSelectButtonInfoData()
            {
                Mode = gamemode.ID,
                ModeTitle = gamemode.DisplayName,
                NewMode = false,
                CountdownTo = null
            };
        }

        public static implicit operator Gamemode(ModeSelectButtonInfoData modeSelectButtonInfo)
        {
            return new Gamemode(modeSelectButtonInfo.Mode, modeSelectButtonInfo.ModeTitle);
        }
    }
}
