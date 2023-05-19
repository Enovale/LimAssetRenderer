using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using UniverseLib;
using UniverseLib.Config;
using UniverseLib.UI;

namespace AssetRenderer
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static ManualLogSource PluginLog;
        
        public static UIBase UiBase { get; private set; }

        public Plugin()
        {
            PluginLog = Log;
        }

        public override void Load()
        {
            // Plugin startup logic
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            Initialize();

            //var harmony = new Harmony($"com.enovale.{MyPluginInfo.PLUGIN_GUID}");
            //harmony.PatchAll(typeof(BanPrevention));
            
            Universe.Init(5f, OnInitialized, UniverseLog, new()
            {
                Disable_EventSystem_Override = true,
                Force_Unlock_Mouse = true,
                Unhollowed_Modules_Folder = Path.Combine(Paths.BepInExRootPath, "interop")
            });
        }

        private void UniverseLog(string arg1, LogType arg2)
        {
            switch (arg2)
            {
                case LogType.Error:
                    PluginLog.LogError(arg1);
                    break;
                case LogType.Assert:
                    PluginLog.LogFatal(arg1);
                    break;
                case LogType.Warning:
                    PluginLog.LogWarning(arg1);
                    break;
                case LogType.Log:
                    PluginLog.LogInfo(arg1);
                    break;
                case LogType.Exception:
                    PluginLog.LogFatal(arg1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arg2), arg2, null);
            }
        }

        private void OnInitialized()
        {
            UiBase = UniversalUI.RegisterUI(MyPluginInfo.PLUGIN_GUID, UiUpdate);
            UiBase.SetOnTop();
            new RecordPanel(UiBase);
            //EncounterPanel.IsShown = false;
        }

        private void UiUpdate()
        {
        }

        private void Initialize()
        {
            PluginBootstrap.Setup();
        }
    }
}