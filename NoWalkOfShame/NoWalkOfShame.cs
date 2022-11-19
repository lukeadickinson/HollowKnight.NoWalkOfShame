using Modding;
using System.Reflection;

namespace NoWalkOfShame
{
    //TODO: add LGPL-2.1 license (copy from benchwarpmod)
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
            ModHooks.AfterSavegameLoadHook += ModHooks_AfterSavegameLoadHook;
        }

        // Code that should be run when the mod is disabled.
        public void Unload()
        {
            
            // Unhook the methods previously registered so no exceptions will happen.
            ModHooks.BeforeSceneLoadHook -= ModHooks_BeforeSceneLoadHook;
            ModHooks.AfterPlayerDeadHook -= ModHooks_AfterPlayerDeadHook;
            ModHooks.SetPlayerStringHook -= ModHooks_SetPlayerStringHook;
            ModHooks.AfterSavegameLoadHook -= ModHooks_AfterSavegameLoadHook;

            // "Destroy" the loaded instance of the mod.
            NoWalkOfShame.LoadedInstance = null;
        }

        private string ModHooks_BeforeSceneLoadHook(string arg)
        {
            string gateText = GameManager.instance.entryGateName;
            string sceneText = GameManager.instance.sceneName;
            if (sceneText != null && sceneText != "" && gateText != null && gateText != "" && HeroController.instance.playerData.health != 0)
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
            return arg;
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

    }
}
