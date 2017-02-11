using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace SSLightningRod
{
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
                if (comp != null && comp.notOverwhelmed && comp.PowerOn)
                {
                    //Log.Message("Added building to activeRods.");
                    activeRods.Add(map.listerBuildings.allBuildingsColonist[i]);
                    result = true;
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

        
        public override void FireEvent()
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
		}

		public override void WeatherEventDraw()
		{
			Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather), Quaternion.identity, FadedMaterialPool.FadedVersionOf(LightningMat, LightningBrightness), 0);
		}
	}
}
