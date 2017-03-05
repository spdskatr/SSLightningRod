using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SSLightningRod
{
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike))]
    [HarmonyPatch("FireEvent")]
    [StaticConstructorOnStartup]
	public class WeatherEvent_LightningRodStrike : WeatherEvent_LightningFlash
    {
		private IntVec3 strikeLoc = IntVec3.Invalid;

		private Mesh boltMesh;

		private static readonly Material LightningMat = MatLoader.LoadMat("Weather/LightningBolt");

        public bool ColonistsHaveLightningRodActive(out List<Building> activeRods, Map map)
        {
            bool result = false;
            activeRods = new List<Building>();
            for (int i = 0; i < map.listerBuildings.allBuildingsColonist.Count; i++)
            {
                CompLightningRod comp = map.listerBuildings.allBuildingsColonist[i].TryGetComp<CompLightningRod>();
                if (comp != null && comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode != 1)
                {
                    //Log.Message("Added building to activeRods.");
                    activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                    result = true;
                }
            }
            if (!result)
            {
                for (int i = 0; i < map.listerBuildings.allBuildingsColonist.Count; i++)
                {
                    CompLightningRod comp = map.listerBuildings.allBuildingsColonist[i].TryGetComp<CompLightningRod>();
                    System.Random random = new System.Random(i + Find.TickManager.TicksAbs);
                    if (comp != null && comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode == 1)
                    {
                        int h = random.Next(4);
                        //Log.Message(h.ToString());
                        if (h == 0)
                        {
                            //Log.Message("Added power saving building to activeRods.");
                            activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                            result = true;
                        }
                    }
                    else if (comp != null && !comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode != 1)
                    {
                        int h = random.Next(comp.powersavechance);
                        if (h == 0)
                        {
                            //Log.Message("Added overwhelmed building to activeRods.");
                            activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                            result = true;
                        }
                    }
                }
            }
            //Log.Message("Detecting lightning rod buildings from " + map.listerBuildings.allBuildingsColonist.Count.ToString() + " buildings on map " + map.ToString());
            return result;
        }

        public WeatherEvent_LightningRodStrike(Map map) : base(map)
		{
		}

		public WeatherEvent_LightningRodStrike(Map map, IntVec3 forcedStrikeLoc) : base(map)
		{
            strikeLoc = forcedStrikeLoc;
        }
        //no longer needed.
        /*public override void FireEvent()
		{
			base.FireEvent();
            Thing targetRod = null;
            bool notAbsorbed = true;
            List<Building> activeRods = new List<Building>();
            bool activeRodsDetected = ColonistsHaveLightningRodActive(out activeRods, map);
            if (activeRodsDetected)
            {
                System.Random rand = new System.Random();
                int num = rand.Next(activeRods.Count);
                Building target = activeRods[num];
                targetRod = target;
                List<IntVec3> list = GenAdj.CellsOccupiedBy(target).ToList();
                float num1 = target.TryGetComp<CompLightningRod>().fakeZIndex;
                int num2 = rand.Next((int)num1);
                strikeLoc = list[0];
                strikeLoc.z += num2;
                notAbsorbed = false;
            }
			else if (!strikeLoc.IsValid)
			{
                strikeLoc = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map) && !map.roofGrid.Roofed(sq), map, 1000);
			}
            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (notAbsorbed)
            {
                //Log.Message("Not absorbed.");
                Vector3 loc = strikeLoc.ToVector3Shifted();
                GenExplosion.DoExplosion(strikeLoc, map, 1.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
                for (int i = 0; i < 4; i++)
                {
                    MoteMaker.ThrowSmoke(loc, map, 1.5f);
                    MoteMaker.ThrowMicroSparks(loc, map);
                }
            }
            else if (targetRod != null)
            {
                targetRod.TryGetComp<CompLightningRod>().Hit();
            }
            MoteMaker.ThrowLightningGlow(strikeLoc.ToVector3Shifted(), map, 1.5f);
            SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc, map, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
		}*/

        [HarmonyPriority(Priority.Last)]
        public static bool Prefix(WeatherEvent_LightningRodStrike __instance)
        {
            WeatherEvent_LightningFlash flash = new WeatherEvent_LightningFlash(__instance.map);
            flash.FireEvent();
            IntVec3 strikeLoc1 = __instance.strikeLoc;
            Map map1 = __instance.map;
            Thing targetRod = null;
            bool notAbsorbed = true;
            List<Building> activeRods = new List<Building>();
            bool activeRodsDetected = __instance.ColonistsHaveLightningRodActive(out activeRods, map1);
            if (activeRodsDetected)
            {
                System.Random rand = new System.Random();
                int num = rand.Next(activeRods.Count);
                Building target = activeRods[num];
                targetRod = target;
                List<IntVec3> list = GenAdj.CellsOccupiedBy(target).ToList();
                int strikesHitBase = (target.TryGetComp<CompLightningRod>().strikesHitBase);
                float num1 = target.TryGetComp<CompLightningRod>().fakeZIndex;
                int num2 = rand.Next((int)num1 - strikesHitBase);
                strikeLoc1 = list[0];
                strikeLoc1.z += num2 + strikesHitBase;
                notAbsorbed = false;
            }
            else if (!strikeLoc1.IsValid)
            {
                strikeLoc1 = CellFinderLoose.RandomCellWith((IntVec3 sq) => sq.Standable(map1) && !map1.roofGrid.Roofed(sq), map1, 1000);
            }
            __instance.boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (notAbsorbed)
            {
                //Log.Message("Not absorbed.");
                Vector3 loc = strikeLoc1.ToVector3Shifted();
                GenExplosion.DoExplosion(strikeLoc1, map1, 1.9f, DamageDefOf.Flame, null, null, null, null, null, 0f, 1, false, null, 0f, 1);
                for (int i = 0; i < 4; i++)
                {
                    MoteMaker.ThrowSmoke(loc, map1, 1.5f);
                    MoteMaker.ThrowMicroSparks(loc, map1);
                }
            }
            else if (targetRod != null)
            {
                targetRod.TryGetComp<CompLightningRod>().Hit();
            }
            MoteMaker.ThrowLightningGlow(strikeLoc1.ToVector3Shifted(), map1, 1.5f);
            SoundInfo info = SoundInfo.InMap(new TargetInfo(strikeLoc1, map1, false), MaintenanceType.None);
            SoundDefOf.Thunder_OnMap.PlayOneShot(info);
            __instance.strikeLoc = strikeLoc1;
            return false;
        }
        public override void WeatherEventDraw()
		{
			Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, LightningBrightness), 0);
		}
	}
}
