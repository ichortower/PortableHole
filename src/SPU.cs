using Microsoft.Xna.Framework;
using SpecialPowerUtilities;
using System;

namespace ichortower.PortableHole
{
    internal class SPUIntegration
    {
        public static void Setup()
        {
            var spuApi = PortableHole.instance.Helper.ModRegistry.GetApi
                    <ISpecialPowerAPI>("spiderbuttons.SpecialPowerUtilities");
            if (spuApi is null) {
                return;
            }
            bool res = spuApi.RegisterPowerCategory(
                    uniqueID: PortableHole.ModId,
                    displayName: () => TR.Get("PortableHole.DisplayName"),
                    iconTexture: $"Mods/{PortableHole.ModId}/ObjectSpriteSheet",
                    sourceRectPosition: new Point(32, 0),
                    sourceRectSize: new Point(16, 16));
            if (!res) {
                Log.Warn("Failed to register category with Special Power Utilities");
                return;
            }
            Log.Trace("Registered power category with Special Power Utilities");
        }
    }
}

namespace SpecialPowerUtilities
{
    public interface ISpecialPowerAPI
    {
        bool RegisterPowerCategory(string uniqueID, Func<string> displayName, string iconTexture, Point sourceRectPosition, Point sourceRectSize);
    }
}
