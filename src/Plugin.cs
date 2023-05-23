using BepInEx;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using Nutils;
using NuclearPasta.DualWieldingPlugin;

namespace NuclearPasta.TheMonks
{
    [BepInPlugin(MOD_ID, "The Monks", "1.0.0")]
    public class TheMonksMod : BaseUnityPlugin
    {
        private const string MOD_ID = "nuclearpasta.themonks";

        public static readonly PlayerFeature<bool> DualWield = PlayerBool("themonks/dual_wield");

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);

            // Put your custom hooks here!
            On.Player.Grabability += new DualWieldingMod(DualWield).DoubleSpear;
            //On.Player.Grabability += Player_DualWield;
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}