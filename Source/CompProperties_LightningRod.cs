using RimWorld;

namespace SSLightningRod
{
    public class CompProperties_LightningRod : CompProperties_Power
    {
        public float cooldownSpeed = 1f;
        public float chargeCapacity = 2000.00f;
        public float fakeZIndex = 4f;
        public CompProperties_LightningRod()
        {
            compClass = typeof(CompLightningRod);
        }
    }
}
