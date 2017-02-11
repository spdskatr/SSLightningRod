using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;
using SSLightningRod;
using RawCodeDetour;

namespace SSDetours
{
    /// <summary>
    /// Utility methods for HugsLib detection. [SS Lightning Rod]
    /// </summary>
    internal static class TestForHugsLibUtility
    {
        internal static bool HugsLibDetector()
        {
            bool detected = false;
            foreach (ModMetaData mod in ModsConfig.ActiveModsInLoadOrder)
            {
                if (mod.Name == "HugsLib")
                {
                    //Log.Message("SS Lightning Rod: HugsLib detected. Using HugsLib detour.");
                    detected = true;
                }
            }

            return detected;
        }
        public static bool tryTestForHugsLibDetour(MethodInfo a, MethodInfo b)
        {
            try
            {
                if (!HugsLib.Source.Detour.DetourProvider.TryCompatibleDetour(a, b))
                {
                    Log.Error("SS Lightning Rod Detours: Detour failed! (HugsLib Detour)");
                    return false;
                }
                return true;
            }
            catch (TypeLoadException)
            {
                Log.Warning("SS Lightning Rod Detours: HugsLib doesn't exist, although we thought it did. Switching to RawCode detour.");
                if (!Detours.TryDetourFromTo(a, b))
                {
                    Log.Error("SS Lightning Rod Detours: Detour failed! (RawCode Detour)");
                    return false;
                }
                return true;
            }
        }

    }
    [StaticConstructorOnStartup]
    internal static class DetourInjector
    {
        internal static MethodInfo originalInterval = typeof(WeatherEvent_LightningStrike).GetMethod(
                    "FireEvent",
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    new Type[] { },
                    null);

        internal static MethodInfo modifiedInterval = typeof(WeatherEvent_LightningRodStrike).GetMethod(
                "FireEvent",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { },
                    null);

        static DetourInjector()
        {
            LongEventHandler.QueueLongEvent(Inject, "LibraryStartup", false, null);
        }

        public static void Inject()
        {
            string Info = " from original " + originalInterval.DeclaringType.FullName + "." + originalInterval.Name + " to modified " + modifiedInterval.DeclaringType.FullName + "." + modifiedInterval.Name;
            if (TestForHugsLibUtility.HugsLibDetector())
            {
                Log.Message("SS Lightning Rod Detours: Detouring via HugsLib" + Info);
                if(!TestForHugsLibUtility.tryTestForHugsLibDetour(originalInterval, modifiedInterval)) Log.Error("SS Lightning Rod Detours: Detour failed!.");
            }
            else
            {
                Log.Message("SS Lightning Rod Detours: Using in-built RawCode detour" + Info);
                if (Detours.TryDetourFromTo(originalInterval, modifiedInterval))
                {
                    Log.Error("SS Lightning Rod Detours: Detour failed! (RawCode Detour)");
                }
            }
        }
    }
}