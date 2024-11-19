using GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;

namespace ichortower.PortableHole
{
    internal class GMCMIntegration
    {
        public static void Setup()
        {
            var gmcmApi = PortableHole.instance.Helper.ModRegistry.GetApi
                    <IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcmApi is null) {
                return;
            }
            gmcmApi.Register(mod: PortableHole.instance.ModManifest,
                reset: () => {},
                save: () => {
                    PortableHole.instance.Helper.WriteConfig(PortableHole.Config);
                    HoleManager.SetDNDStatus();
                }
            );
            gmcmApi.AddKeybindList(
                mod: PortableHole.instance.ModManifest,
                name: () => TR.Get("gmcm.SecondDoorKey.name"),
                tooltip: () => TR.Get("gmcm.SecondDoorKey.tooltip"),
                getValue: () => PortableHole.Config.SecondDoorKey,
                setValue: (value) => {
                    PortableHole.Config.SecondDoorKey = value;
                }
            );
            gmcmApi.AddBoolOption(
                mod: PortableHole.instance.ModManifest,
                name: () => TR.Get("gmcm.DoNotDisturb.name"),
                tooltip: () => TR.Get("gmcm.DoNotDisturb.tooltip"),
                getValue: () => PortableHole.Config.DoNotDisturb,
                setValue: (value) => {
                    PortableHole.Config.DoNotDisturb = value;
                }
            );
            Log.Trace("Registered with Generic Mod Config Menu");
        }
    }
}

namespace GenericModConfigMenu
{
    public interface IGenericModConfigMenuApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);
        void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
    }
}
