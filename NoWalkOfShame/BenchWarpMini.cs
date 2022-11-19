using GlobalEnums;
using Modding;
using UnityEngine;

namespace NoWalkOfShame
{
    public class BenchWarpMini
    {
        //These methods were copied and slightly edited from https://github.com/homothetyhk/HollowKnight.BenchwarpMod/.
        //Credit to homothetyhk for original work. 

        public static bool IsDarkOrDreamRoom()
        {
            return IsDarkRoom() || IsDreamRoom();
        }

        public static bool IsDarkRoom()
        {
            return (!PlayerData.instance.hasLantern && GameManager.instance.sm.darknessLevel == 2);
        }

        public static bool IsDreamRoom()
        {
            return GameManager.instance.sm.mapZone switch
            {
                //GlobalEnums.MapZone.DREAM_WORLD
                //or GlobalEnums.MapZone.GODS_GLORY
                //or GlobalEnums.MapZone.GODSEEKER_WASTE
                //or GlobalEnums.MapZone.WHITE_PALACE => true,
                //               _ => false,

                GlobalEnums.MapZone.DREAM_WORLD
                or GlobalEnums.MapZone.GODS_GLORY
                or GlobalEnums.MapZone.GODSEEKER_WASTE => true,
                               _ => false,
            };
        }

        public static void ChangeToScene(string sceneName, string gateName, float delay = 0f)
        {
            UIManager.instance.UIClosePauseMenu();
            Time.timeScale = 1f;
            GameManager.instance.FadeSceneIn();
            GameManager.instance.isPaused = false;
            GameCameras.instance.ResumeCameraShake();
            if (HeroController.SilentInstance != null)
            {
                HeroController.instance.UnPause();
            }
            MenuButtonList.ClearAllLastSelected();
            TimeController.GenericTimeScale = 1f;
            GameManager.instance.actorSnapshotUnpaused.TransitionTo(0f);
            GameManager.instance.ui.AudioGoToGameplay(.2f);
            if (HeroController.SilentInstance != null)
            {
                HeroController.instance.UnPause();
            }
            MenuButtonList.ClearAllLastSelected();
            PlayerData.instance.atBench = false; // kill bench storage
            if (HeroController.SilentInstance != null)
            {
                if (HeroController.instance.cState.onConveyor || HeroController.instance.cState.onConveyorV || HeroController.instance.cState.inConveyorZone)
                {
                    HeroController.instance.GetComponent<ConveyorMovementHero>()?.StopConveyorMove();
                    HeroController.instance.cState.inConveyorZone = false;
                    HeroController.instance.cState.onConveyor = false;
                    HeroController.instance.cState.onConveyorV = false;
                }

                HeroController.instance.cState.nearBench = false;
            }

            SceneLoad load = ReflectionHelper.GetField<GameManager, SceneLoad>(GameManager.instance, "sceneLoad");
            if (load != null)
            {
                load.Finish += () =>
                {
                    LoadScene(sceneName, gateName, delay);
                };
            }
            else
            {
                LoadScene(sceneName, gateName, delay);
            }
        }

        private static void LoadScene(string sceneName, string gateName, float delay)
        {
            GameManager.instance.StopAllCoroutines();
            ReflectionHelper.SetField<GameManager, SceneLoad>(GameManager.instance, "sceneLoad", null);

            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                IsFirstLevelForPlayer = false,
                SceneName = sceneName,
                HeroLeaveDirection = GetGatePosition(gateName),
                EntryGateName = gateName,
                EntryDelay = delay,
                PreventCameraFadeOut = false,
                WaitForSceneTransitionCameraFade = true,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = false
            });
        }
        private static GatePosition GetGatePosition(string name)
        {
            if (name.Contains("top"))
            {
                return GatePosition.top;
            }

            if (name.Contains("bot"))
            {
                return GatePosition.bottom;
            }

            if (name.Contains("left"))
            {
                return GatePosition.left;
            }

            if (name.Contains("right"))
            {
                return GatePosition.right;
            }

            if (name.Contains("door"))
            {
                return GatePosition.door;
            }

            return GatePosition.unknown;
        }

    }
}
