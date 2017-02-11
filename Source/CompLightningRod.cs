using RimWorld;
using System;
using System.Text;
using Verse;

namespace SSLightningRod
{
    class CompLightningRod : CompPowerTrader
    {
        private float LightningRodCooldown = 0.00f;
        public bool notOverwhelmed = true;
        private float cooldownSpeed
        {
            get
            {
                return properties.cooldownSpeed;
            }
        }
        private float chargeCapacity
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
        private CompProperties_LightningRod properties
        {
            get
            {
                return (CompProperties_LightningRod)props;
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if (LightningRodCooldown <= 0f)
            {
                LightningRodCooldown = 0f;
                powerOutputInt = properties.basePowerConsumption * -1;
            }
            if (LightningRodCooldown > 0f)
            {
                LightningRodCooldown -= cooldownSpeed;
                powerOutputInt = properties.basePowerConsumption / 2;
            }
            if (LightningRodCooldown < chargeCapacity)
            {
                notOverwhelmed = true;
            }
            if (LightningRodCooldown >= chargeCapacity)
            {
                notOverwhelmed = false;
            }
        }
        public void Hit()
        {
            LightningRodCooldown += 500.00f;
            if (LightningRodCooldown > chargeCapacity)
            {
                notOverwhelmed = false;
            }
        }
        public override string CompInspectStringExtra()
        {
            StringBuilder str = new StringBuilder();
            float powerConsumptionAsValue = powerOutputInt * -1;
            if (LightningRodCooldown <= 0) str.AppendLine("Standby mode.");
            if (!notOverwhelmed) str.AppendLine("Overwhelmed.");
            if (LightningRodCooldown > 0f && notOverwhelmed) str.AppendLine("Discharging.");
            if (powerConsumptionAsValue >= 0)str.AppendLine("Power consumption: " + powerConsumptionAsValue + " W");
            if (powerConsumptionAsValue < 0)str.AppendLine("Power output: " + powerOutputInt + " W");
            if (!PowerOn)
            {
                str.AppendLine("PowerNotConnected".Translate());
            }
            else
            {
                string text = (PowerNet.CurrentEnergyGainRate() / WattsToWattDaysPerTick).ToString("F0");
                string text2 = PowerNet.CurrentStoredEnergy().ToString("F0");
                str.AppendLine("PowerConnectedRateStored".Translate(new object[]
                {
        text,
        text2
                }));
            }
            str.Append("Cooldown: " + Math.Round(LightningRodCooldown) + "/" + chargeCapacity);
            return str.ToString();
        }
    }
}
