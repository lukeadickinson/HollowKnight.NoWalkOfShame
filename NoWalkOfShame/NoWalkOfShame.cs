using Modding;
using System.Reflection;

namespace NoWalkOfShame
{
    public class NoWalkOfShame : Mod, ITogglableMod
    {
        // Store the currently loaded instance of the mod.
        public static NoWalkOfShame LoadedInstance { get; set; }

        public string WarpScene { get; set; }
        public string WarpGate { get; set; }
        public bool JustDied { get; set; }


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


        //This will set the warp one room behind. This is needed if the current room is not valid for warping and took dmg.
        private string ModHooks_BeforeSceneLoadHook(string arg)
        {
            Attempt_Set_Warp_Location();
            return arg;
        }

        private int ModHooks_AfterTakeDamageHook(int hazardType, int damageAmount)
        {
            Attempt_Set_Warp_Location();
            return damageAmount;
        }

        private void ModHooks_AfterPlayerDeadHook()
        {
            JustDied = true;
            if (WarpScene == null || WarpScene == "" || WarpGate == null || WarpGate == "")
            {
                return;
            }
            this.Log($"change_spawn location from dead -> to:  {WarpScene}, {WarpGate}");
            GameManager.instance.entryGateName = WarpGate;
            BenchWarpMini.ChangeToScene(WarpScene, WarpGate);
        }

        private string ModHooks_SetPlayerStringHook(string name, string res)
        {
            if (name == "respawnMarkerName")
            {
                Log($"Respawn set. Cleared Warp location (if there was one)");

                WarpScene = "";
                WarpGate = "";
            }
            return res;
        }

        private void ModHooks_AfterSavegameLoadHook(SaveGameData obj)
        {
            //clear any past state data
            WarpScene = "";
            WarpGate = "";
            JustDied = false;
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
                sceneText != "Room_Colosseum_Gold")

            {
                if (JustDied)
                {
                    JustDied = false;
                }
                else
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
