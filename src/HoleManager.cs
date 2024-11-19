using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using xTile.Tiles;

namespace ichortower.PortableHole
{
    internal record struct Portal
    {
        // unique multiplayer id of farmer/hole to connect to
        public long UniqueFarmerId = -1;
        // where the portal is located (location, x, y)
        public string LocationId = "";
        public int TileX = -1;
        public int TileY = -1;
        // Index is 0 to 2. maybe more entrances will be added in the future,
        // but probably not.
        public int Index = 0;

        public Portal()
        {
        }

        public Portal(long uid, string loc, int x, int y, int index)
        {
            UniqueFarmerId = uid;
            LocationId = loc;
            TileX = x;
            TileY = y;
            Index = index;
        }
    }

    internal sealed class HoleManager
    {
        public static List<Portal> Entrances = new();
        public static Portal WhereICameFrom = new();
        public static Dictionary<long, bool> DoNotDisturb = new();

        private static string sheetId = $"zzzzz_{PortableHole.ModId}_tiles";
        // named indexes in the tilesheet where important stuff lies
        //private static int itemSpriteIndex = 0;
        private static int blankTileIndex = 1;
        private static int firstHoleIndex = 2;

        public static void Open(Portal entrance, bool broadcast = true)
        {
            if (string.IsNullOrEmpty(entrance.LocationId) ||
                    entrance.TileX < 0 || entrance.TileY < 0) {
                return;
            }
            GameLocation location = Game1.getLocationFromName(entrance.LocationId);
            if (location is null) {
                return;
            }

            // this looks wacky because Close normally removes the hole from
            // the entrance list, which we want. but that also wrecks the
            // indexes of other entrances, so copy non-matching ones out and
            // replace the list when done.
            List<Portal> remaining = new();
            for (int i = 0; i < Entrances.Count; ++i) {
                if (Entrances[i].UniqueFarmerId == entrance.UniqueFarmerId &&
                        IndexMatches(Entrances[i].Index, entrance.Index)) {
                    Close(i, broadcast, unref:false);
                }
                else {
                    remaining.Add(Entrances[i]);
                }
            }
            Entrances = remaining;

            InjectHoleTilesheet(location);
            RenderOpening(entrance, location);
            if (broadcast) {
                PortableHole.instance.Helper.Multiplayer.SendMessage(
                        entrance, "Open", modIDs: new[] {PortableHole.ModId});
            }

            Entrances.Add(entrance);
        }

        private static void RenderOpening(Portal entrance, GameLocation location)
        {
            Action setTile = delegate {
                location.setMapTile(entrance.TileX, entrance.TileY,
                        entrance.Index+firstHoleIndex, "Buildings", sheetId);
                location.setTileProperty(entrance.TileX, entrance.TileY,
                        "Buildings", "Action",
                        $"{PortableHole.EnterTileAction} {entrance.UniqueFarmerId} {entrance.Index}");
            };
            if (location != Game1.currentLocation) {
                setTile();
                return;
            }

            // do not try to sync or broadcast the effects here. the open event
            // is netsynced via SMAPI message and the other players will draw
            // their own if needed

            Random r = new Random();
            int sparkleCount = 5;
            int sparkleStep = 34;
            int smokeCount = 4;
            int smokeStep = 50;

            Action sparkles = delegate {
                location.localSound("yoba",
                        position: new Vector2(entrance.TileX, entrance.TileY),
                        pitch: 1100 + r.Next(0, 201));
                Color particleColor = entrance.Index switch {
                    1 => new Color(195, 245, 231),
                    2 => new Color(232, 199, 177),
                    _ => Color.White
                };
                for (int i = 0; i < sparkleCount; ++i) {
                    float rad = (float)(r.NextDouble() * 2 * Math.PI);
                    float dist = (float)(r.NextDouble() * 0.67f + 0.67f);
                    Vector2 pos = new(entrance.TileX + dist*MathF.Cos(rad),
                            entrance.TileY - dist*MathF.Sin(rad));
                    location.temporarySprites.Add(new(r.Next(10, 12), pos*64f, particleColor) {
                            layerDepth = 1f,
                            delayBeforeAnimationStart = i * sparkleStep,
                            interval = 1f * sparkleStep,
                            motion = new Vector2(-4f * dist * MathF.Cos(rad),
                                    4f * dist * MathF.Sin(rad)),
                            xStopCoordinate = entrance.TileX,
                            yStopCoordinate = entrance.TileY
                        });
                }
            };

            Action smokes = delegate {
                for (int i = 0; i < smokeCount; ++i) {
                    float rad = (float)(r.NextDouble() * 2 * Math.PI);
                    float dist = (float)(r.NextDouble() * 0.5f) + 0.5f;
                    Vector2 pos = new((float)entrance.TileX + .1f, (float)entrance.TileY + .1f);
                    location.temporarySprites.Add(new(5, pos*64f, Color.White * 0.5f) {
                            delayBeforeAnimationStart = 2 * i * smokeStep,
                            interval = 1f * smokeStep,
                            scale = 0.75f,
                            motion = new Vector2(2f * dist * MathF.Cos(rad),
                                    -2f * dist * MathF.Sin(rad)),
                            startSound = "sandyStep"
                        });
                }
            };

            // set an empty tile to block placement during the animation
            location.setMapTile(entrance.TileX, entrance.TileY,
                    blankTileIndex, "Buildings", sheetId);

            sparkles();
            DelayedAction.functionAfterDelay(delegate {
                smokes();
                setTile();
            }, sparkleCount*sparkleStep / 2);
        }

        public static void Close(Portal entrance, bool broadcast = true, bool unref = true)
        {
            if (string.IsNullOrEmpty(entrance.LocationId) ||
                    entrance.TileX < 0 || entrance.TileY < 0) {
                return;
            }
            int existing = Entrances.IndexOf(entrance);
            if (existing == -1) {
                Log.Warn($"Tried to close portal {entrance.ToString()} but couldn't find it");
                return;
            }
            Close(existing, broadcast, unref);
        }

        private static void Close(int index, bool broadcast = true, bool unref = true)
        {
            Portal entrance = Entrances[index];
            GameLocation location = Game1.getLocationFromName(entrance.LocationId);
            if (location != null) {
                location.removeMapTile(entrance.TileX, entrance.TileY, "Buildings");
                location.removeTileProperty(entrance.TileX, entrance.TileY, "Buildings", "Action");
            }
            if (broadcast) {
                PortableHole.instance.Helper.Multiplayer.SendMessage(
                        entrance, "Close", modIDs: new[] {PortableHole.ModId});
            }
            if (unref) {
                Entrances.RemoveAt(index);
            }
        }

        private static void InjectHoleTilesheet(GameLocation location)
        {
            TileSheet t = location.map.GetTileSheet(sheetId);
            if (t != null) {
                return;
            }
            Log.Trace($"Adding portable hole tilesheet to {location.Name}");
            t = new TileSheet(sheetId, location.map,
                    $"Maps/{PortableHole.ModId}_MapTiles",
                    new Size(5, 1),
                    new Size(16, 16));
            location.map.AddTileSheet(t);
            location.map.LoadTileSheets(Game1.mapDisplayDevice);
            // skipping check for _mapSeatsDirty because i know i'm not messing
            // with map seats
        }

        public static bool WarpToHole(GameLocation location, string[] args,
                Farmer player, Point tile)
        {
            if (args.Length < 2 || !long.TryParse(args[1], out long uid)) {
                Log.Error("Could not parse hole id from map action " +
                        $"'{string.Join(" ", args)}'");
                return true;
            }
            int index = 0;
            if (args.Length >= 3) {
                _ = int.TryParse(args[2], out index);
            }
            GameLocation hole = CreateHoleFor(uid);
            if (hole is null) {
                Log.Error($"Failed to create hole for uniqueId {uid}!");
                return true;
            }
            if (uid != player.UniqueMultiplayerID &&
                    DoNotDisturb.TryGetValue(uid, out bool deny) &&
                    deny == true) {
                Game1.showRedMessage(TR.Get("DoNotDisturb.toast"));
                return true;
            }
            // record where the player came from (for use with music, and as an
            // emergency destination if the portal is gone when leaving).
            // no need for player id or index
            WhereICameFrom = new Portal(-1, Game1.currentLocation.NameOrUniqueName,
                    Game1.player.TilePoint.X, Game1.player.TilePoint.Y, -1);
            // set the hole to the same location context as this map
            hole.locationContextId = location.locationContextId;
            LocationRequest req = Game1.getLocationRequest(hole.NameOrUniqueName);
            // x coordinate changes for secondary entrance, depends on
            // owner's map size. I think this runs before CanonizeMap so we
            // have to duplicate the mail check
            int xval = 3;
            if (index == 2) {
                Farmer owner = (uid == player.UniqueMultiplayerID
                        ? player : GetFarmerFromUid(uid));
                xval += (owner.mailReceived.Contains(PortableHole.MailSpaceId) ? 5 : 3);
            }
            Game1.warpFarmer(req, tileX:xval, tileY:5,
                    facingDirectionAfterWarp:2);
            return true;
        }

        public static bool LeaveMain(GameLocation location, string[] args,
                Farmer player, Point tile)
        {
            int idx = 0;
            if (Game1.player.mailReceived.Contains(PortableHole.MailDoorId)) {
                ++idx;
            }
            return Leave(location, index: idx);
        }

        public static bool LeaveSecondary(GameLocation location, string[] args,
                Farmer player, Point tile)
        {
            return Leave(location, index: 2);
        }

        private static bool Leave(GameLocation location, int index = 0)
        {
            long uid = PlayerIdForHole(location);
            int existing = Entrances.FindIndex(p => {
                return p.UniqueFarmerId == uid && IndexMatches(p.Index, index);
            });
            Portal dest = WhereICameFrom;
            if (existing >= 0) {
                dest = Entrances[existing];
            }
            LocationRequest req = Game1.getLocationRequest(dest.LocationId);
            req.OnWarp += delegate {
                if (uid == Game1.player.UniqueMultiplayerID) {
                    if (dest != WhereICameFrom) {
                        Close(dest, broadcast: true);
                    }
                    // set position again, since some locations override your
                    // target tile
                    Game1.player.Position = new(dest.TileX*64f, dest.TileY*64f);
                }
            };
            Game1.warpFarmer(req, dest.TileX, dest.TileY, Game1.player.FacingDirection);
            return true;
        }

        public static GameLocation CreateHoleFor(Farmer player)
        {
            string uniq = $"{PortableHole.ModId}_Hole_{player.UniqueMultiplayerID}";
            GameLocation hole = Game1.getLocationFromNameInLocationsList(uniq);
            if (!Game1.IsMasterGame && hole is null) {
                PortableHole.instance.Helper.Multiplayer.SendMessage(
                        player.UniqueMultiplayerID, "Create",
                        modIDs: new[] {PortableHole.ModId});
            }

            string size = (player.mailReceived.Contains(PortableHole.MailSpaceId)
                    ? "Large" : "Small");
            string usePath = $"Maps/{PortableHole.ModId}_Hole{size}";
            if (hole is null) {
                hole = new(usePath, uniq);
                hole.uniqueName.Value = uniq;
                Game1.locations.Add(hole);
            }
            return hole;
        }

        public static GameLocation CreateHoleFor(long farmerUniqueId)
        {
            return CreateHoleFor(GetFarmerFromUid(farmerUniqueId));
        }

        /*
         * Reloads the map (removing any overlays) and freshly applies ladder
         * overlays for active entrances.
         */
        public static void CanonizeMap(GameLocation hole)
        {
            long uid = PlayerIdForHole(hole);
            Farmer owner = GetFarmerFromUid(uid);
            string size = (owner.mailReceived.Contains(PortableHole.MailSpaceId)
                    ? "Large" : "Small");
            string useMap = $"Maps/{PortableHole.ModId}_Hole{size}";
            if (hole.mapPath.Value != useMap) {
                hole.mapPath.Set(useMap);
            }
            hole.loadMap(hole.mapPath.Value, force_reload:true);
            HashSet<string> amo = (HashSet<string>)
                    typeof(GameLocation).GetField("_appliedMapOverrides",
                        BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(hole);
            amo.Clear();
            // check which holes are open, and patch in ladders to match
            string[] actions = new string[] {PortableHole.LeaveMainTileAction,
                    PortableHole.LeaveMainTileAction,
                    PortableHole.LeaveSecondaryTileAction};
            // large hole is wider, so secondary entrance is further right
            int[] xcoords = new int[] {3, 3, (size == "Large" ? 8 : 6)};
            for (int i = 0; i <= 2; ++i) {
                int eIndex = Entrances.FindIndex(p => {
                    return p.UniqueFarmerId == uid && p.Index == i;
                });
                if (eIndex >= 0) {
                    hole.ApplyMapOverride($"{PortableHole.ModId}_Ladder",
                            override_key_name: $"ladder_{i}",
                            source_rect: null,
                            destination_rect: new Microsoft.Xna.Framework
                                    .Rectangle(xcoords[i], 1, 1, 4));
                    hole.setTileProperty(xcoords[i], 4, "Buildings", "Action", actions[i]);
                }
            }
        }

        /*
         * Sets the do not disturb value for the current player. This is synced
         * to other players, since it's not useful otherwise.
         */
        public static void SetDNDStatus()
        {
            KeyValuePair<long, bool> dnd = new(Game1.player.UniqueMultiplayerID,
                    PortableHole.Config.DoNotDisturb);
            HoleManager.DoNotDisturb[dnd.Key] = dnd.Value;
            PortableHole.instance.Helper.Multiplayer.SendMessage(
                    dnd, "DND", modIDs: new[] {PortableHole.ModId});
        }

        private static Farmer GetFarmerFromUid(long uid)
        {
            List<Farmer> whos = new(){Game1.player};
            foreach (Farmer f in Game1.otherFarmers.Values) {
                whos.Add(f);
            }
            return whos.Where(f => f.UniqueMultiplayerID == uid).ElementAt(0);
        }

        /*
         * special matching behavior for portal indexes: 0 and 1 are treated
         * as matching.
         */
        private static bool IndexMatches(int a, int b)
        {
            return a switch {
                var v when v == 0 || v == 1 => b == 0 || b == 1,
                _ => b == a
            };
        }

        public static long PlayerIdForHole(GameLocation hole)
        {
            string sid = hole.NameOrUniqueName.Split("_").Last();
            if (!long.TryParse(sid, out long uid)) {
                Log.Error($"Failed to parse location name '{hole.NameOrUniqueName}'");
                return Game1.player.UniqueMultiplayerID;
            }
            return uid;
        }

        public static void CloseAllHoles(bool broadcast = true)
        {
            for (int i = 0; i < Entrances.Count; ++i) {
                Close(i, broadcast, unref: false);
            }
            Entrances.Clear();
        }

    }
}
