using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ichortower.PortableHole
{
    internal sealed class PortableHole : Mod
    {
        public static PortableHole instance;
        public static string ModId;
        public static string ItemHoleId;
        public static string ItemSpaceId;
        public static string ItemDoorId;
        public static string MailHoleId;
        public static string MailSpaceId;
        public static string MailDoorId;
        public static string EnterTileAction;
        public static string LeaveMainTileAction;
        public static string LeaveSecondaryTileAction;

        public static ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            instance = this;
            ModId = instance.ModManifest.UniqueID;
            ItemHoleId = $"{ModId}_PortableHole";
            ItemSpaceId = $"{ModId}_AstralPylon";
            ItemDoorId = $"{ModId}_DimensionalCoupling";
            MailHoleId = $"{ModId}_AcquiredHole";
            MailSpaceId = $"{ModId}_SpaceUpgrade";
            MailDoorId = $"{ModId}_DoorUpgrade";
            EnterTileAction = $"{ModId}_Enter";
            LeaveMainTileAction = $"{ModId}_LeaveMain";
            LeaveSecondaryTileAction = $"{ModId}_LeaveSecondary";

            Config = helper.ReadConfig<ModConfig>();

            helper.Events.Content.AssetRequested += Events.OnAssetRequested;
            helper.Events.GameLoop.DayStarted += Events.OnDayStarted;
            helper.Events.GameLoop.DayEnding += Events.OnDayEnding;
            helper.Events.GameLoop.GameLaunched += Events.OnGameLaunched;
            helper.Events.Multiplayer.ModMessageReceived += Events.OnModMessageReceived;
            helper.Events.Multiplayer.PeerConnected += Events.OnPeerConnected;
            helper.Events.Specialized.LoadStageChanged += Events.OnLoadStageChanged;

            GameLocation.RegisterTileAction(EnterTileAction, HoleManager.WarpToHole);
            GameLocation.RegisterTileAction(LeaveMainTileAction, HoleManager.LeaveMain);
            GameLocation.RegisterTileAction(LeaveSecondaryTileAction, HoleManager.LeaveSecondary);

            Patches.Apply();
        }
    }

    internal sealed class Log
    {
        public static void Trace(string text) {
            PortableHole.instance.Monitor.Log(text, LogLevel.Trace);
        }
        public static void Debug(string text) {
            PortableHole.instance.Monitor.Log(text, LogLevel.Debug);
        }
        public static void Info(string text) {
            PortableHole.instance.Monitor.Log(text, LogLevel.Info);
        }
        public static void Warn(string text) {
            PortableHole.instance.Monitor.Log(text, LogLevel.Warn);
        }
        public static void Error(string text) {
            PortableHole.instance.Monitor.Log(text, LogLevel.Error);
        }
        public static void Alert(string text) {
            PortableHole.instance.Monitor.Log(text, LogLevel.Alert);
        }
        public static void Verbose(string text) {
            PortableHole.instance.Monitor.VerboseLog(text);
        }
    }

    internal sealed class TR
    {
        public static string Get(string key) {
            return PortableHole.instance.Helper.Translation.Get(key);
        }

        public static string Parse(string val) {
            int s = 0;
            int start = 0;
            while ((start = val.IndexOf("{{", s)) != -1) {
                int e = val.IndexOf("}}", start+2);
                string name = val.Substring(start+2, e-(start+2)).Split(":")[1];
                string rep = TR.Get(name);
                val = val.Substring(0, start) + rep + val.Substring(e+2);
                s = start+rep.Length;
            }
            return val;
        }
    }
}
