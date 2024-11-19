using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Enums;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Powers;
using StardewValley.GameData.Shops;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using xTile;

namespace ichortower.PortableHole
{
    internal sealed class Events
    {
        public static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Data/Objects")) {
                Dictionary<string, ObjectData> manifest = PortableHole.instance
                        .Helper.ModContent.Load<Dictionary<string, ObjectData>>
                        ("assets/objects.json");
                e.Edit(asset => {
                    var dict = asset.AsDictionary<string, ObjectData>();
                    foreach (var kvp in manifest) {
                        string key = kvp.Key.Replace("{{ModId}}", PortableHole.ModId);
                        ObjectData m = kvp.Value;
                        m.DisplayName = TR.Parse(m.DisplayName);
                        m.Description = TR.Parse(m.Description);
                        m.Texture = m.Texture.Replace("{{ModId}}", PortableHole.ModId);
                        dict.Data[key] = m;
                    }
                });
            }
            else if (e.Name.IsEquivalentTo("Data/Powers")) {
                Dictionary<string, PowersData> manifest = PortableHole.instance
                        .Helper.ModContent.Load<Dictionary<string, PowersData>>
                        ("assets/powers.json");
                e.Edit(asset => {
                    var dict = asset.AsDictionary<string, PowersData>();
                    foreach (var kvp in manifest) {
                        string key = kvp.Key.Replace("{{ModId}}", PortableHole.ModId);
                        PowersData m = kvp.Value;
                        m.DisplayName = TR.Parse(m.DisplayName);
                        m.Description = TR.Parse(m.Description);
                        m.TexturePath = m.TexturePath.Replace(
                                "{{ModId}}", PortableHole.ModId);
                        m.UnlockedCondition = m.UnlockedCondition.Replace(
                                "{{ModId}}", PortableHole.ModId);
                        dict.Data[key] = m;
                    }
                });
            }
            else if (e.Name.IsEquivalentTo("Data/Shops")) {
                List<ShopExtra> manifest = PortableHole.instance.Helper
                        .ModContent.Load<List<ShopExtra>>("assets/shops.json");
                e.Edit(asset => {
                    var dict = asset.AsDictionary<string, ShopData>();
                    foreach (ShopExtra entry in manifest) {
                        if (!dict.Data.ContainsKey(entry.Shop)) {
                            Log.Warn($"Discarding entry: shop '{entry.Shop}' not found");
                            continue;
                        }
                        string targetId = entry.Before ?? entry.After ?? "";
                        int anchor = (targetId == "" ? -1 : 0);
                        if (anchor != -1) {
                            anchor = dict.Data[entry.Shop].Items.FindIndex(
                                    i => i.Id == targetId);
                        }
                        entry.Item.Id = entry.Item.Id.Replace(
                                "{{ModId}}", PortableHole.ModId);
                        entry.Item.ItemId = entry.Item.ItemId.Replace(
                                "{{ModId}}", PortableHole.ModId);
                        entry.Item.Condition = entry.Item.Condition.Replace(
                                "{{ModId}}", PortableHole.ModId);
                        if (anchor == -1) {
                            dict.Data[entry.Shop].Items.Add(entry.Item);
                        }
                        else {
                            if (!string.IsNullOrEmpty(entry.After)) {
                                ++anchor;
                            }
                            dict.Data[entry.Shop].Items.Insert(anchor, entry.Item);
                        }
                    }
                });
            }
            else if (e.Name.IsEquivalentTo($"Mods/{PortableHole.ModId}/ObjectSpriteSheet")) {
                e.LoadFromModFile<Texture2D>("assets/object-tilesheet.png",
                        AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo($"Maps/{PortableHole.ModId}_MapTiles")) {
                e.LoadFromModFile<Texture2D>("assets/object-tilesheet.png",
                        AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo($"Maps/{PortableHole.ModId}_HoleSmall")) {
                e.LoadFromModFile<Map>("assets/maps/HoleSmall.tmx",
                        AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo($"Maps/{PortableHole.ModId}_HoleLarge")) {
                e.LoadFromModFile<Map>("assets/maps/HoleLarge.tmx",
                        AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo($"Maps/{PortableHole.ModId}_Ladder")) {
                e.LoadFromModFile<Map>("assets/maps/Ladder.tmx",
                        AssetLoadPriority.Medium);
            }
            else if (e.Name.IsEquivalentTo("Strings/Locations")) {
                e.Edit(asset => {
                    var dict = asset.AsDictionary<string, string>();
                    dict.Data[$"{PortableHole.ModId}_MapName"] =
                            TR.Get("HoleLocation.DisplayName");
                });
            }
        }

        public static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            HoleManager.SetDNDStatus();
        }

        public static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            HoleManager.CloseAllHoles(broadcast: false);
        }

        public static void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            GMCMIntegration.Setup();
        }

        public static void OnLoadStageChanged(object sender, LoadStageChangedEventArgs e)
        {
            if (e.NewStage != LoadStage.SaveLoadedBasicInfo) {
                return;
            }
            foreach (GameLocation l in SaveGame.loaded.locations) {
                if (l.NameOrUniqueName.StartsWith(PortableHole.ModId)) {
                    string sid = l.NameOrUniqueName.Split("_").Last();
                    GameLocation creat = HoleManager.CreateHoleFor(long.Parse(sid));
                    if (creat is null) {
                        Log.Error($"Failed to restore hole for farmer {sid}");
                    }
                }
            }
        }


        public static void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != PortableHole.ModId) {
                return;
            }
            if (e.Type == "Open") {
                Portal target = e.ReadAs<Portal>();
                HoleManager.Open(target, broadcast: false);
            }
            else if (e.Type == "Close") {
                Portal target = e.ReadAs<Portal>();
                HoleManager.Close(target, broadcast: false);
            }
            else if (e.Type == "Create") {
                long uid = e.ReadAs<long>();
                _ = HoleManager.CreateHoleFor(uid);
            }
            else if (e.Type == "DND") {
                KeyValuePair<long, bool> state = e.ReadAs<KeyValuePair<long, bool>>();
                HoleManager.DoNotDisturb[state.Key] = state.Value;
            }
        }

        public static void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            HoleManager.SetDNDStatus();
        }

    }

    // data model for how to load shops.json
    internal class ShopExtra
    {
        public string Before = null;
        public string After = null;
        public string Shop = null;
        public ShopItemData Item = null;
    }

    // data model for Button's SPU
    internal class PowerTab
    {
        public string TabDisplayName = null;
        public string IconPath = null;
        public Microsoft.Xna.Framework.Rectangle IconSourceRect = new();
        // obsolete
        public string SectionName = null;
    }
}
