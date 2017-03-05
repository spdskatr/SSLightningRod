using RimWorld;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace SSLightningRod
{
    class CompLightningRod : CompPowerTrader
    {
        private float LightningRodCooldown = 0f;
        public bool notOverwhelmed = true;
        public int ToggleMode = 1;
        public int strikesHitBase
        {
            get
            {
                if (properties.strikesHitBase)
                {
                    return 0;
                }
                return 1;
            }
        }
        public float cooldownSpeed
        {
            get
            {
                return properties.cooldownSpeed;
            }
        }
        public float chargeCapacity
        {
            get
            {
                return properties.chargeCapacity;
            }
        }
        public float fakeZIndex
        {
            get
            {
                return properties.fakeZIndex;
            }
        }
        public int powersavechance
        {
            get
            {
                return properties.oneInXChanceHitPowerSave;
            }
        }
        public float powerGainDischarge
        {
            get
            {
                return properties.powerGainDischarge;
            }
        }
        public float cooldownPercentPowerSave
        {
            get
            {
                return properties.cooldownPercentPowerSave;
            }
        }
        private CompProperties_LightningRod properties
        {
            get
            {
                return (CompProperties_LightningRod)props;
            }
        }
        public float powerOutputFromMode()
        {
            float num;
            switch (ToggleMode)
            {
                case 1:
                    num = 0;
                    break;
                case 2:
                    num = properties.basePowerConsumption * -1;
                    break;
                case 3:
                    num = properties.basePowerConsumption * -3;
                    break;
                default:
                    num = 0;
                    ToggleMode = 1;
                    break;
            }
            return num;
        }
        public override void CompTick()
        {
            base.CompTick();
            float num2 = (ToggleMode == 1) ? cooldownSpeed * cooldownPercentPowerSave / 100 : cooldownSpeed;
            float num = (ToggleMode == 3) ? cooldownSpeed * 4 : num2;
            LightningRodCooldown = (LightningRodCooldown <= 0f) ? 0f : LightningRodCooldown - num;
            powerOutputInt = (LightningRodCooldown > 0f) ? powerOutputFromMode() + powerGainDischarge : powerOutputFromMode();
            notOverwhelmed = (LightningRodCooldown < chargeCapacity);
        }
        public void Hit()
        {
            LightningRodCooldown += 500f;
            if (LightningRodCooldown > chargeCapacity)
            {
                notOverwhelmed = false;
            }
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder str = new StringBuilder();
            float powerConsumptionAsValue = powerOutputInt * -1;
            string str1 = (powerConsumptionAsValue >= 0) ? "Power consumption: " + powerConsumptionAsValue + " W" : "Power output: " + powerOutputInt + " W";
            str.AppendLine(str1);
            string str2 = (LightningRodCooldown <= 0) ? "Standby." : (notOverwhelmed) ? "Discharging." : "Overwhelmed." ;
            str.AppendLine(str2);
            if (!PowerOn)
            {
                str.AppendLine("PowerNotConnected".Translate());
            }
            else
            {
                string text = (PowerNet.CurrentEnergyGainRate() / WattsToWattDaysPerTick).ToString("F0");
                string text2 = PowerNet.CurrentStoredEnergy().ToString("F0");
                str.AppendLine("PowerConnectedRateStored".Translate(text, text2));
            }
            str.Append("Cooldown: " + Math.Round(LightningRodCooldown) + "/" + chargeCapacity);
            return str.ToString();
        }
        public string ModeDescs()
        {
            string returnstr = "";
            switch (ToggleMode)
            {
                case 1:
                    returnstr = "Power saving mode(Click to change modes): Does not consume power when idle, outputs a lot of power when struck, but only has a " + Math.Round((decimal)100 / powersavechance, 2) + "% chance to attract a lightning bolt and cools down " + cooldownPercentPowerSave + "% slower";
                    break;
                case 2:
                    returnstr = "Normal mode(Click to change modes): Consumes a bit of power when idle, outputs enough power to sustain itself in a storm and has a 100% chance to attract a lightning bolt. Can attract lightning at a " + Math.Round((decimal)100 / powersavechance, 2) + "% chance when overwhelmed.";
                    break;
                case 3:
                    returnstr = "Fast cooldown mode(Click to change modes): Cools down 4x faster than normal but consumes triple the amount of power. Has a 100% chance to attract a lightning bolt. Can attract lightning at a " + Math.Round((decimal)100 / powersavechance, 2) + "% chance when overwhelmed.";
                    break;
                default:
                    ToggleMode = 1;
                    returnstr = "Power saving mode(Click to change modes): Does not consume power when idle, outputs a lot of power when struck, but only has a " + Math.Round((decimal)100 / powersavechance, 2) + "% chance to attract a lightning bolt and cools down " + cooldownPercentPowerSave + "% slower";
                    break;
            }
            return returnstr;
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.LookValue(ref ToggleMode, "ToggleMode", 1);
            Scribe_Values.LookValue(ref LightningRodCooldown, "Cooldown", 0);
            Scribe_Values.LookValue(ref notOverwhelmed, "notOverwhelmed", true);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo c in base.CompGetGizmosExtra())
            {
                yield return c;
            }
            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_ChangeMode
                {
                    hotKey = KeyBindingDefOf.Misc8,
                    icon = ContentFinder<Texture2D>.Get("RotRight_Green", true),
                    defaultLabel = "Change Mode",
                    defaultDesc = ModeDescs(),
                    Mode = (() => ToggleMode),
                    toggleAction = delegate
                    {
                        if(LightningRodCooldown > 0)
                        {
                            Messages.Message("Cannot change mode now, rod still discharging.", MessageSound.RejectInput);
                        }
                        else
                        {
                            ToggleMode = ToggleMode + 1;
                        }
                    }
                };
            }
        }
    }
}
