using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime.Injection;
using MainUI;
using UnityEngine;
using UnityEngine.UI;

namespace AssetRenderer
{
    public class PluginBootstrap : MonoBehaviour
    {
        public static PluginBootstrap Instance;

        public static List<RecordInfo> ToRecord = new()
        {
            new(10403, 30f),
            /*
            new(10103, 30f),
            new(10204, 30f),
            new(10302, 30f),
            new(10403, 30f),
            new(10404, 30f),
            new(10503, 30f),
            new(10504, 30f),
            new(10603, 30f),
            new(10703, 30f),
            new(10802, 30f),
            new(10902, 30f),
            new(11002, 30f),
            new(11005, 30f),
            new(11104, 30f),
            new(11203, 30f),
            */
        };

        private static Queue<RecordInfo> _recordQueue = new(ToRecord);
        private static RecordInfo _currentRecord = null;

        private static string _recordFolder = "Record";
        private static int _fps = 60;
        private static int _scale = 1;
        private static int _maxFrames => (int)(_currentRecord.MaxSeconds * _fps);

        private static GameObject _cgObj;
        private static int _frames = 0;
        private static bool _recording = false;
        private static Il2CppSystem.Collections.IEnumerator coroutine;

        internal static void Setup()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PluginBootstrap>();

            GameObject obj = new(MyPluginInfo.PLUGIN_GUID + "bootstrap");
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<PluginBootstrap>();
        }

        private void Awake()
        {
            coroutine = RecordCoroutine().WrapToIl2Cpp();
        }

        private IEnumerator RecordCoroutine()
        {
            while (_recordQueue.Count > 0)
            {
                SetupNextRecord();

                while (_frames < _maxFrames)
                {
                    yield return new WaitForEndOfFrame();
                    Capture();
                }
            }
            _recording = false;
        }

        private void Capture()
        {
            if (_recording)
            {
                ScreenCapture.CaptureScreenshot(GetPath(), _scale);
            }
        }

        private string GetPath()
        {
            var folderPath = Path.Combine(_recordFolder, $"{_currentRecord.PersonalityId}");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.GetFullPath(Path.Combine(folderPath, $"out_{_frames++:000#}.png"));
        }

        private void Update()
        {
            if (_recording)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                _recording = !_recording;
                if (_recording)
                    StartCoroutine(coroutine);
                else if (!_recording)
                    StopCoroutine(coroutine);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                Plugin.ShowRecordPanel = !Plugin.ShowRecordPanel;
            }
        }

        private static void SetupNextRecord()
        {
            if (_recordQueue.TryDequeue(out var next))
            {
                _currentRecord = next;
                SetupPersonality(next.PersonalityId, next.Gacksung);
            }
        }

        private static void SetupPersonality(int personalityId, bool gacksung)
        {
            if (_cgObj)
            {
                DestroyImmediate(_cgObj);
            }

            var obj = SingletonBehavior<UIController>.Instance.MainCanvas.gameObject;
            var rootTransform = obj.transform;
            var children = rootTransform.GetChildCount();
            for (var i = 0; i < children; i++)
            {
                rootTransform.GetChild(i).gameObject.SetActive(false);
            }

            _cgObj = new GameObject("Personality");
            var tr = _cgObj.AddComponent<RectTransform>();
            tr.parent = rootTransform;
            tr.localPosition = Vector3.zero;
            tr.localScale = Vector3.one;
            var img = _cgObj.AddComponent<Image>();
            var personalityObj =
                ReimplementedCg.SetCgData(personalityId, gacksung, img,
                    SPINE_LOCATION.GackSung, 1);
            personalityObj.transform.SetConstrainProportionsScale(true);
            personalityObj.transform.localScale = Vector3.one;
            var position = personalityObj.transform.position;
            position = new(position.x, position.y, 50);
            personalityObj.transform.position = position;

            Application.targetFrameRate = _fps;
            Time.captureFramerate = _fps;
            _frames = 0;
        }
    }
}