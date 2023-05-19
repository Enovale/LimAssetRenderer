using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AssetRenderer.Helper;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Il2CppInterop.Runtime;
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
        private static int _threadCount = 0;
        private static object _threadLock = new();
        private static Queue<(int Frame, int Personality, byte[] RawData)> _screenshotQueue = new();
        private static Queue<Texture2D> _destroyQueue = new();

        internal static void Setup()
        {
            ClassInjector.RegisterTypeInIl2Cpp<PluginBootstrap>();

            GameObject obj = new(MyPluginInfo.PLUGIN_GUID + "bootstrap");
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<PluginBootstrap>();
            var thread = new Thread(EncodeAndSave);
            thread.Start();
            /*
            for (var i = 0; i < 80; i++)
            {
                var thread = new Thread(EncodeAndSave);
                thread.Start();
            }
            */
        }

        private static void EncodeAndSave()
        {
            var thread = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());

            while (true)
            {
                while (_threadCount < 60 && _screenshotQueue.TryDequeue(out var result))
                {
                    var path = GetPath(result.Frame, result.Personality);
                    var data = result.RawData;
                    new Thread(() =>
                    {
                        var thread2 = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
                        using var stream = new MemoryStream();
                        BitmapEncoder.WriteBitmap(stream, 1920, 1080, data);
                        stream.Flush();
                        File.WriteAllBytes(path, stream.ToArray());
                        stream.Dispose();
                        _threadCount--;
                        IL2CPP.il2cpp_thread_detach(thread2);
                    }).Start();
                    Plugin.PluginLog.LogInfo($"Started thread {_threadCount}");
                    _threadCount++;
                }
                
                //if (_recording)
                //    Plugin.PluginLog.LogInfo("Sleeping!");
                Thread.Sleep(1);
            }
            
            IL2CPP.il2cpp_thread_detach(thread);
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
                var screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(_scale);
                var watch = new Stopwatch();
                watch.Start();
                _screenshotQueue.Enqueue((_frames++, _currentRecord.PersonalityId, screenshotTexture.GetRawTextureData()));
                watch.Stop();
                Plugin.PluginLog.LogInfo(watch.ElapsedMilliseconds);
                _destroyQueue.Enqueue(screenshotTexture);
                //ScreenCapture.CaptureScreenshot(GetPath(_frames++, _currentRecord.PersonalityId), _scale);
            }
        }

        private static string GetPath(int frame, int personalityId)
        {
            var folderPath = Path.Combine(_recordFolder, $"{personalityId}");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.GetFullPath(Path.Combine(folderPath, $"out_{frame:000#}.png"));
        }

        private void Update()
        {
            while (Input.GetKeyDown(KeyCode.D) && _destroyQueue.TryDequeue(out var tex))
            {
                DestroyImmediate(tex);
                GC.Collect();
            }
            
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