using BepInEx;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using Nutils.hook;
using Nutils;
using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using MoreSlugcats;
using Noise;
using On;
using RWCustom;
using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using IL;
using MonoMod.RuntimeDetour;
using static Menu.Remix.MixedUI.OpTextBox;
using UnityEngine.PlayerLoop;
using DevInterface;

namespace NuclearPasta.TheMonks
{
    [BepInPlugin("nuclearpasta.themonks", "The Monks", "1.0.0")]

    public class TheMonksMod : BaseUnityPlugin
    {

        public readonly static SlugcatStats.Name TheFasting = new SlugcatStats.Name("nc.Fasting");
        public readonly static SlugcatStats.Name TheMeditative = new SlugcatStats.Name("nc.Meditative");
        public readonly static SlugcatStats.Name TheDisciple = new SlugcatStats.Name("nc.Disciple");
        //Fasting values
        public readonly static int FastingRequiredFood = 3;
        public readonly static int FastingFullFood = 6;
        //Meditative values
        public readonly static int MeditativeRequiredFood = 3;
        public readonly static int MeditativeFullFood = 6;
        //Disciple values
        public readonly static int DiscipleRequiredFood = 3;
        public readonly static int DiscipleFullFood = 6;


        public static int MeditateTimer = 0;
        public static int MushroomTimer = 0;


        //important storage bools
        public static bool DoesPlayerExist;
        public static bool IsPlayerAlive;
        public static bool IsCycleEnded;
        public static bool IsCycleTimerFull;
        public void OnEnable()
        {
            On.Player.Update += Player_DoesPlayerExist;

            On.Player.Grabability += DoubleSpear;
            On.Player.Update += Player_SpearRecharging;

            On.Player.Update += Player_OnMushrooms;
            On.Player.Update += Player_KarmaticFeeding;

            On.Player.Update += Player_CycleTimerCheck;

        }


        private void Player_CycleTimerCheck(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (self.room.world.rainCycle.timer == self.room.world.rainCycle.cycleLength)
            {
                IsCycleEnded = true;
            }

            if (self.room.world.rainCycle.timer == 0)
            {
                IsCycleTimerFull = true;
            }

            if(IsCycleEnded)
            {
                Logger.LogDebug("Cycle has ended");
                Console.WriteLine("Cycle has ended");
                Debug.Log("Cycle has ended");
            }
            orig(self, eu);
        }

        private void Player_KarmaticFeeding(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (Input.GetKey(KeyCode.LeftControl) && self.input[0].x == 0 && self.input[0].y == 0 && self.SlugCatClass == TheMeditative)
            {
                if (self.FoodInStomach == MeditativeFullFood)
                {
                    self.mushroomEffect = 1.0f;
                    MeditateTimer++;
                    if (MeditateTimer == 120)
                    {
                        self.SubtractFood(MeditativeFullFood);
                        (self.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma -= 1;
                        MeditateTimer = 0;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.LeftShift) && self.input[0].x == 0 && self.input[0].y == 0 && self.SlugCatClass == TheMeditative)
            {
                if (self.FoodInStomach >= 0 && self.FoodInStomach <= MeditativeFullFood && self.Karma < self.KarmaCap)
                {
                    self.mushroomEffect = 1.0f;
                    MeditateTimer++;
                    if (MeditateTimer == 120)
                    {
                        self.AddFood(MeditativeFullFood);
                        (self.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma += 1;
                        MeditateTimer = 0;
                    }
                }
            }
            orig(self, eu);
        }

        private void Player_OnMushrooms(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (Input.GetKey(KeyCode.LeftControl) && self.input[0].x == 0 && self.input[0].y == 0 && self.SlugCatClass == TheDisciple && IsCycleEnded == false)
            {
                if (self.FoodInStomach > 0)
                {
                    self.mushroomEffect = 1.0f;
                    MushroomTimer++;
                    if(MushroomTimer == 160)
                    {
                        self.SubtractFood(6);
                        self.room.world.rainCycle.timer -= 200;
                        MushroomTimer = 0;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.RightShift) && self.input[0].x == 0 && self.input[0].y == 0 && self.SlugCatClass == TheDisciple && IsCycleEnded == false)
            {

                if (self.FoodInStomach > 0)
                {
                    self.mushroomEffect = 1.0f;
                    MushroomTimer++;
                    if (MushroomTimer == 160)
                    {
                        self.AddFood(1);
                        self.room.world.rainCycle.timer += 200;
                        MushroomTimer = 0;
                    }
                }

            }
            orig(self, eu);
        }


        private void Player_SpearRecharging(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (self.SlugCatClass == TheFasting && IsPlayerAlive == true && (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt)))
            {
                foreach (UpdatableAndDeletable physicalObject in self.room.updateList)
                {
                    ElectricSpear electricspear = physicalObject as ElectricSpear;
                    bool flag = electricspear != null;
                    if (flag)
                    {
                        for (int i = 0; i < electricspear.grabbedBy.Count; i++)
                        {
                            bool flag2 = electricspear.grabbedBy[i].grabber is Player && electricspear.abstractSpear.electricCharge <= 0 && (electricspear.grabbedBy[i].grabber as Player).SlugCatClass == TheMeditative;
                            if (flag2)
                            {
                                electricspear.Recharge();
                            }
                        }
                    }
                }
            }
            orig(self, eu);
        }

        private Player.ObjectGrabability DoubleSpear(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {

            if (obj is Spear && self.SlugCatClass == TheFasting)
            {
                return (Player.ObjectGrabability)1;
            }
            return orig(self, obj);

        }


        private void Player_DoesPlayerExist(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (self != null)
            {
                DoesPlayerExist = true;

                if (DoesPlayerExist == true && self.Stunned == false && self.Consious == true)
                {
                    IsPlayerAlive = true;
                }
            }
            orig(self, eu);
        }
    }
}