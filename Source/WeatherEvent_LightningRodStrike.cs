using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SSLightningRod
{
    [HarmonyPatch(typeof(WeatherEvent_LightningStrike))]
    [HarmonyPatch("FireEvent")]
    [StaticConstructorOnStartup]
	public static class WeatherEvent_LightningRodStrike
    {
        static bool _state_ = false;
        public static bool ColonistsHaveLightningRodActive(out List<Building> activeRods, Map map)
        {
            bool result = false;
            activeRods = new List<Building>();
            for (int i = 0; i < map.listerBuildings.allBuildingsColonist.Count; i++)
            {
                CompLightningRod comp = map.listerBuildings.allBuildingsColonist[i].TryGetComp<CompLightningRod>();
                if (comp != null && comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode != 1)
                {
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
                        if (h == 0)
                        {
                            activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                            result = true;
                        }
                    }
                    else if (comp != null && !comp.notOverwhelmed && comp.PowerOn && comp.ToggleMode != 1)
                    {
                        int h = random.Next(comp.Powersavechance);
                        if (h == 0)
                        {
                            activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                            result = true;
                        }
                    }
                }
            }
            _state_ = result;
            return result;
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
        public static void Prefix(WeatherEvent_LightningStrike __instance)
        {
            WeatherEvent_LightningFlash flash = new WeatherEvent_LightningFlash(Traverse.Create(__instance).Field("map").GetValue<Map>());
            flash.FireEvent();
            Map map1 = Traverse.Create(__instance).Field("map").GetValue<Map>();
            Thing targetRod = null;
            List<Building> activeRods = new List<Building>();
            bool activeRodsDetected = ColonistsHaveLightningRodActive(out activeRods, map1);
            if (activeRodsDetected)
            {
                System.Random rand = new System.Random();
                int num = rand.Next(activeRods.Count);
                Building target = activeRods[num];
                targetRod = target;
                List<IntVec3> list = GenAdj.CellsOccupiedBy(target).ToList();
                int strikesHitBase = (target.TryGetComp<CompLightningRod>().StrikesHitBase);
                float num1 = target.TryGetComp<CompLightningRod>().FakeZIndex;
                int num2 = rand.Next((int)num1 - strikesHitBase);
                IntVec3 intvec = list[0];
                intvec.z += num2 + strikesHitBase;
                Traverse.Create(__instance).Field("strikeLoc").SetValue(intvec);
                target.TryGetComp<CompLightningRod>().Hit();
            }
            #region commented area
            /*
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
            return false;*/
            #endregion
        }
        /// <summary>
        /// To stop rods blowing themselves up.
        /// </summary>
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var intermediate = Transpilers.MethodReplacer(instructions, typeof(GenExplosion).GetMethod("DoExplosion", BindingFlags.Public | BindingFlags.Static), typeof(WeatherEvent_LightningRodStrike).GetMethod(nameof(DoExplosionLogic)));
            return Transpilers.MethodReplacer(intermediate, typeof(MoteMaker).GetMethod("ThrowSmoke"), typeof(WeatherEvent_LightningRodStrike).GetMethod(nameof(DoSmokeLogic)));
        }
        public static void DoExplosionLogic (IntVec3 center, Map map, float radius, DamageDef damType, Thing instigator, int damAmount = -1, SoundDef explosionSound = null, ThingDef source = null, ThingDef projectile = null, ThingDef postExplosionSpawnThingDef = null, float postExplosionSpawnChance = 0f, int postExplosionSpawnThingCount = 1, bool applyDamageToExplosionCellsNeighbors = false, ThingDef preExplosionSpawnThingDef = null, float preExplosionSpawnChance = 0f, int preExplosionSpawnThingCount = 1, float chanceToStartFire = 0f, bool dealMoreDamageAtCenter = false)
        {
            if (!_state_) GenExplosion.DoExplosion(center, map, radius, damType, instigator, damAmount, explosionSound, source, projectile, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, preExplosionSpawnChance = 0f, preExplosionSpawnThingCount, chanceToStartFire, dealMoreDamageAtCenter);
        }
        public static void DoSmokeLogic (Vector3 loc, Map map, float size)
        {
            if (!_state_) MoteMaker.ThrowSmoke(loc, map, size);
        }
	}
}
