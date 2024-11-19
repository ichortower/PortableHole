using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ichortower.PortableHole
{
    internal sealed class ModConfig
    {
        public KeybindList SecondDoorKey = new(SButton.LeftShift);
        public bool DoNotDisturb = false;
    }
}
