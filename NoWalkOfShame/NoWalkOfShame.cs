using Modding;
using System.Reflection;
using UnityEngine;

namespace NoWalkOfShame
{
    public class NoWalkOfShame : Mod, ITogglableMod
    {
        // Store the currently loaded instance of the mod.
        public static NoWalkOfShame LoadedInstance { get; set; }

        public string WarpScene { get; set; }
        public string WarpGate { get; set; }
        public bool NeedToWarp { get; set; }

        // Override the `GetVersion` method to get the assembly version.
        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        // Code that should run on mod initialization.
        //
        // If your mod implements `ITogglableMod`, this method will be run on loaded the mod up again
        // so make sure to write it in a way that it will not fail if ran multiple times.
        public override void Initialize()
        {
            // Check if there is already a loaded mod instance.
            if (NoWalkOfShame.LoadedInstance != null) return;
            NoWalkOfShame.LoadedInstance = this;
            ModHooks.BeforeSceneLoadHook += ModHooks_BeforeSceneLoadHook;
            ModHooks.AfterPlayerDeadHook += ModHooks_AfterPlayerDeadHook;

            ModHooks.SetPlayerStringHook += ModHooks_SetPlayerStringHook;
            ModHooks.AfterTakeDamageHook += ModHooks_AfterTakeDamageHook;
            ModHooks.SetPlayerBoolHook += ModHooks_SetPlayerBoolHook;

            ModHooks.AfterSavegameLoadHook += ModHooks_AfterSavegameLoadHook;
        }

        // Code that should be run when the mod is disabled.
        public void Unload()
        {
            
            // Unhook the methods previously registered so no exceptions will happen.
            ModHooks.BeforeSceneLoadHook -= ModHooks_BeforeSceneLoadHook;
            ModHooks.AfterPlayerDeadHook -= ModHooks_AfterPlayerDeadHook;
            ModHooks.SetPlayerStringHook -= ModHooks_SetPlayerStringHook;
            ModHooks.AfterTakeDamageHook -= ModHooks_AfterTakeDamageHook;
            ModHooks.AfterSavegameLoadHook -= ModHooks_AfterSavegameLoadHook;

            // "Destroy" the loaded instance of the mod.
            NoWalkOfShame.LoadedInstance = null;
        }
        private bool ModHooks_SetPlayerBoolHook(string name, bool orig)
        {
            if (name == "atBench" && orig == false)
            {
                if (NeedToWarp)
                {
                    NeedToWarp = false;
                    this.Log($"warp to -> to:  {WarpScene}, {WarpGate}");
                    GameManager.instance.entryGateName = WarpGate;
                    BenchWarpMini.ChangeToScene(WarpScene, WarpGate);
                }
            }
            return orig;
        }

        //This will set the warp one room behind. This is needed if the current room is not valid for warping and took dmg.
        private string ModHooks_BeforeSceneLoadHook(string arg)
        {
            Attempt_Set_Warp_Location();
            return arg;
        }

        //only update warp location when taking dmg when if it already exists. Otherwise, forced warp locations will be overwritten and softlock sections of the game.
        private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
        {
            if (WarpScene != "")
            {
                Attempt_Set_Warp_Location();
            }
            return damageAmount;
        }

        private void ModHooks_AfterPlayerDeadHook()
        {
            if (WarpScene == null || WarpScene == "" || WarpGate == null || WarpGate == "")
            {
                //do nothing
            }
            else
            {
                NeedToWarp = true;
            }

        }

        private string ModHooks_SetPlayerStringHook(string name, string res)
        {
            if (name == "respawnMarkerName")
            {
                Log($"Respawn set. Cleared Warp location (if there was one)");

                WarpScene = "";
                WarpGate = "";
                NeedToWarp = false;
            }
            return res;
        }

        private void ModHooks_AfterSavegameLoadHook(SaveGameData obj)
        {
            Log($"ModHooks_AfterSavegameLoadHook. Cleared Warp location (if there was one)");
            //clear any past state data
            WarpScene = "";
            WarpGate = "";
            NeedToWarp = false;
        }

        private void Attempt_Set_Warp_Location()
        {
            string gateText = GameManager.instance.entryGateName;
            string sceneText = GameManager.instance.sceneName;

            //if in dream/godhome, clear the warping logic in case of problems
            if (BenchWarpMini.IsDreamRoom())
            {
                gateText = "";
                sceneText = "";
            }

            if (sceneText != null && sceneText != "" &&
                gateText != null && gateText != "" &&
                HeroController.instance.playerData.health != 0 &&
                !BenchWarpMini.IsDarkOrDreamRoom() &&
                sceneText != "Room_Colosseum_Bronze" &&
                sceneText != "Room_Colosseum_Silver" &&
                sceneText != "Room_Colosseum_Gold" &&
                gateText != "dreamGate")

            {
                if (!NeedToWarp)
                {
                    WarpScene = sceneText;
                    WarpGate = gateText;
                    Log($"Set Scene: {sceneText}");
                    Log($"Set Gate: {gateText}");
                }
            }
        }

    }
}
