using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ichortower.PortableHole
{
    internal sealed class Patches
    {
        public static void Apply()
        {
            Harmony harmony = new(PortableHole.ModId);
            PatchMethod(harmony, typeof(StardewValley.Farmer),
                    nameof(StardewValley.Farmer.couldInventoryAcceptThisItem),
                    new Type[]{typeof(StardewValley.Item)},
                    nameof(Patches.Farmer_couldInventoryAcceptThisItem_Postfix));
            PatchMethod(harmony, typeof(StardewValley.Farmer),
                    nameof(StardewValley.Farmer.GetItemReceiveBehavior), null,
                    nameof(Patches.Farmer_GetItemReceiveBehavior_Postfix));
            PatchMethod(harmony, typeof(StardewValley.Farmer),
                    nameof(StardewValley.Farmer.OnItemReceived), null,
                    nameof(Patches.Farmer_OnItemReceived_Postfix));
            PatchMethod(harmony, typeof(StardewValley.Object),
                    nameof(StardewValley.Object.actionWhenPurchased), null,
                    nameof(Patches.Object_actionWhenPurchased_Postfix));
            PatchMethod(harmony, typeof(StardewValley.Object),
                    nameof(StardewValley.Object.placementAction), null,
                    nameof(Patches.Object_placementAction_Prefix));
            PatchMethod(harmony, typeof(StardewValley.Object),
                    nameof(StardewValley.Object.drawPlacementBounds), null,
                    nameof(Patches.Object_drawPlacementBounds_Prefix));
            PatchMethod(harmony, typeof(StardewValley.Locations.MineShaft),
                    "clearInactiveMines", null,
                    nameof(Patches.MineShaft_clearInactiveMines_Prefix));
            PatchMethod(harmony, typeof(StardewValley.GameLocation),
                    nameof(StardewValley.GameLocation.HandleMusicChange), null,
                    nameof(Patches.GameLocation_HandleMusicChange_Prefix));
            PatchMethod(harmony, typeof(StardewValley.Utility),
                    nameof(StardewValley.Utility.playerCanPlaceItemHere), null,
                    nameof(Patches.Utility_playerCanPlaceItemHere_Postfix));
            PatchMethod(harmony, typeof(StardewValley.GameLocation),
                    nameof(StardewValley.GameLocation.MakeMapModifications), null,
                    nameof(Patches.GameLocation_MakeMapModifications_Postfix));
        }

        private static void PatchMethod(Harmony harmony, Type t, string name,
                Type[] argTypes, string patch)
        {
            string[] parts = patch.Split("_");
            string last = parts[parts.Length-1];
            if (last != "Prefix" && last != "Postfix" && last != "Transpiler") {
                Log.Error($"Skipping patch method '{patch}': bad type '{last}'");
                return;
            }
            try {
                MethodInfo m;
                if (argTypes is null) {
                    m = t.GetMethod(name,
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static);
                }
                else {
                    m = t.GetMethod(name,
                            BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance | BindingFlags.Static,
                            null, argTypes, null);
                }
                HarmonyMethod func = new(typeof(Patches), patch);
                if (last == "Prefix") {
                    harmony.Patch(original: m, prefix: func);
                }
                else if (last == "Postfix") {
                    harmony.Patch(original: m, postfix: func);
                }
                else if (last == "Transpiler") {
                    harmony.Patch(original: m, transpiler: func);
                }
                Log.Trace($"Patched method '{t.Name}.{m.Name}' ({last})");
            }
            catch (Exception e) {
                Log.Error($"Patch failed ({patch}): {e}");
            }
        }

        /*
         * Prevent requiring open space for our upgrade items by always
         * returning true.
         */
        public static void Farmer_couldInventoryAcceptThisItem_Postfix(
                Item item,
                ref bool __result)
        {
            if (item.QualifiedItemId == $"(O){PortableHole.ItemSpaceId}" ||
                    item.QualifiedItemId == $"(O){PortableHole.ItemDoorId}") {
                __result = true;
            }
        }

        /*
         * Define item receive behavior for our upgrade items, so they don't
         * show "inventory full" if you buy them at full inventory.
         * Also makes extra sure you don't need the space, I suppose.
         */
        public static void Farmer_GetItemReceiveBehavior_Postfix(
                Item item,
                ref bool needsInventorySpace,
                ref bool showNotification)
        {
            if (item.QualifiedItemId == $"(O){PortableHole.ItemSpaceId}" ||
                    item.QualifiedItemId == $"(O){PortableHole.ItemDoorId}") {
                needsInventorySpace = false;
                showNotification = false;
            }
        }

        /*
         * Hook into Object.actionWhenPurchased for our upgrade items. If that
         * method returns true, the item poofs immediately instead of being
         * held (recipes and purchaseable key items, like the key to the town,
         * behave this way).
         */
        public static void Object_actionWhenPurchased_Postfix(
                ref bool __result,
                StardewValley.Object __instance,
                string shopId)
        {
            if (__instance.QualifiedItemId == $"(O){PortableHole.ItemSpaceId}") {
                Game1.player.mailReceived.Add(PortableHole.MailSpaceId);
                __result = true;
            }
            else if (__instance.QualifiedItemId == $"(O){PortableHole.ItemDoorId}") {
                Game1.player.mailReceived.Add(PortableHole.MailDoorId);
                __result = true;
            }
        }

        /*
         * Hook into Farmer.OnItemReceived for our items. When they are added
         * to the inventory, set the mail flag indicating that the player has
         * acquired them.
         */
        public static void Farmer_OnItemReceived_Postfix(
                Farmer __instance,
                Item item,
                int countAdded,
                Item mergedIntoStack)
        {
            if (item.QualifiedItemId == $"(O){PortableHole.ItemHoleId}") {
                __instance.mailReceived.Add(PortableHole.MailHoleId);
            }
            else if (item.QualifiedItemId == $"(O){PortableHole.ItemSpaceId}") {
                __instance.mailReceived.Add(PortableHole.MailSpaceId);
            }
            else if (item.QualifiedItemId == $"(O){PortableHole.ItemDoorId}") {
                __instance.mailReceived.Add(PortableHole.MailDoorId);
            }
        }

        /*
         * Replace the green/red placement tile draw function for our item.
         * Skipping prefixes are no fun, but neither would be the transpiler
         * for this.
         * The idea is to draw the hole sprite instead of the item/cloth.
         * Unfortunately, the way this works in vanilla is to read the item
         * data and use the SpriteIndex there, so trying to temporarily change
         * ParentSheetIndex has no effect.
         * Rather than transpile into Object.draw, this just replaces the draw
         * path for our object specifically. Luckily we can omit a lot of the
         * conditions that we know are false.
         */
        public static bool Object_drawPlacementBounds_Prefix(
                StardewValley.Object __instance,
                SpriteBatch spriteBatch,
                GameLocation location)
        {
            if (__instance.QualifiedItemId != $"(O){PortableHole.ItemHoleId}") {
                return true;
            }
            Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
            int x = (int)Game1.GetPlacementGrabTile().X * 64;
            int y = (int)Game1.GetPlacementGrabTile().Y * 64;
            if (Game1.isCheckingNonMousePlacement)
            {
                Vector2 v = Utility.GetNearbyValidPlacementPosition(
                        Game1.player, location, __instance, x, y);
                x = (int)v.X;
                y = (int)v.Y;
            }
            Vector2 tile = new Vector2(x / 64, y / 64);
            if (__instance.Equals(Game1.player.ActiveObject))
            {
                __instance.TileLocation = tile;
            }
            bool canPlaceHere = Utility.playerCanPlaceItemHere(location,
                    __instance, x, y, Game1.player);
            Game1.isCheckingNonMousePlacement = false;
            Vector2 tbox = new(tile.X * 64f - (float)Game1.viewport.X,
                    tile.Y * 64f - (float)Game1.viewport.Y);
            spriteBatch.Draw(Game1.mouseCursors, tbox,
                    new Microsoft.Xna.Framework.Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16),
                    Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
            Microsoft.Xna.Framework.Rectangle bounds = __instance.GetBoundingBoxAt(x, y);
            ParsedItemData pid = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
            int offset = 2;
            if (Game1.player.mailReceived.Contains(PortableHole.MailDoorId)) {
                ++offset;
                if (PortableHole.Config.SecondDoorKey.IsDown()) {
                    ++offset;
                }
            }
            spriteBatch.Draw(pid.GetTexture(), tbox, pid.GetSourceRect(offset: offset), Color.White * 0.5f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(__instance.isPassable() ? bounds.Top : bounds.Center.Y)/10000f);
            return false;
        }



        public static bool Object_placementAction_Prefix(
                StardewValley.Object __instance,
                ref bool __result,
                GameLocation location,
                int x, int y,
                Farmer who = null)
        {
            if (__instance.QualifiedItemId != $"(O){PortableHole.ItemHoleId}") {
                return true;
            }
            __result = false;
            long uid = (who ?? Game1.player).UniqueMultiplayerID;
            // for farmhands this may return null. we don't actually care though
            _ = HoleManager.CreateHoleFor(uid);
            int index = 0;
            if (Game1.player.mailReceived.Contains(PortableHole.MailDoorId)) {
                ++index;
                if (PortableHole.Config.SecondDoorKey.IsDown()) {
                    ++index;
                }
            }
            HoleManager.Open(new Portal(uid, location.NameOrUniqueName,
                    (int)(x/64), (int)(y/64), index));
            return false;
        }

        /*
         * Prevent a portable hole from being placeable inside any player's
         * portable hole.
         */
         // note: omitted unused parameters
        public static void Utility_playerCanPlaceItemHere_Postfix(
                ref bool __result,
                GameLocation location,
                Item item)
        {
            if (location.NameOrUniqueName.StartsWith(PortableHole.ModId) &&
                    item.QualifiedItemId == $"(O){PortableHole.ItemHoleId}") {
                __result = false;
            }
        }

        /*
         * Prevent mine levels from being cleared/reset while in the hole.
         * This stops the 10-minute update from clearing, as well as the
         * immediate clear when leaving a MineShaft for a not-MineShaft.
         */
        public static bool MineShaft_clearInactiveMines_Prefix()
        {
            if (Game1.currentLocation.NameOrUniqueName.StartsWith(PortableHole.ModId)) {
                return false;
            }
            return true;
        }

        /*
         * Prevent music from changing when entering the hole (matters in the
         * mines and some other locations with special music handling).
         * Also reduce volume somewhat.
         *
         * When leaving the hole, restore the volume. Prevent music from
         * changing as well, but only if the player is returning to the map
         * they entered from.
         */
        public static bool GameLocation_HandleMusicChange_Prefix(
                GameLocation oldLocation,
                GameLocation newLocation)
        {
            if (newLocation != null && newLocation.NameOrUniqueName
                    .StartsWith(PortableHole.ModId)) {
                SmoothVolume(0.6f);
                return false;
            }
            if (oldLocation != null && oldLocation.NameOrUniqueName
                    .StartsWith(PortableHole.ModId)) {
                SmoothVolume(1f);
                if (HoleManager.WhereICameFrom.LocationId == newLocation.NameOrUniqueName) {
                    return false;
                }
            }
            return true;
        }

        public static void GameLocation_MakeMapModifications_Postfix(
                GameLocation __instance,
                bool force = false)
        {
            if (!__instance.NameOrUniqueName.StartsWith($"{PortableHole.ModId}")) {
                return;
            }
            HoleManager.CanonizeMap(__instance);
        }

        private static void SmoothVolume(float end, int duration = 240)
        {
            if (!Game1.game1.IsMainInstance) {
                return;
            }
            float musicStart = Game1.musicPlayerVolume;
            float ambientStart = Game1.ambientPlayerVolume;
            int count = 8;
            for (int i = 0; i < count; ++i) {
                int amt = (duration/count) * (i+1);
                DelayedAction.functionAfterDelay(delegate {
                    Game1.musicCategory.SetVolume(Utility.Lerp(
                            musicStart, end * Game1.options.musicVolumeLevel,
                            (float)amt/(float)duration));
                    Game1.ambientCategory.SetVolume(Utility.Lerp(
                            ambientStart, end * Game1.options.ambientVolumeLevel,
                            (float)amt/(float)duration));
                }, amt);
            }
        }

    }
}
