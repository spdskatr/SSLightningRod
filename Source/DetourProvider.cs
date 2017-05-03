using System;
using System.Reflection;
using Verse;
using RimWorld;
using SSLightningRod;

namespace SSDetours
{
    [StaticConstructorOnStartup]
    public static class DetourInjector
    {
        static DetourInjector()
        {
            LongEventHandler.QueueLongEvent(Inject, "Running patches", false, null);
        }

        public static void Inject()
        {
            string Info = "WeatherEvent_LightningStrike.FireEvent";
            try
            {
                ((Action)(() => {
                    Harmony.HarmonyInstance hinstance = Harmony.HarmonyInstance.Create("com.spdskatr.lightningrod.detours");
                    Log.Message("SS Lightning Rod Detours: Using Harmony to Prefix and Transpiler patch " + Info);
                    hinstance.PatchAll(Assembly.GetExecutingAssembly());
                    return;
                }))();
            }
            catch (TypeLoadException) //These lines shouldn't be activated in normal circumstances
            {
                Log.Error("SS Lightning Rod Detours: Tried to use Harmony to patch method, but Harmony was not found. Mod will not work if this error comes up.");
            }
        }
    }
}