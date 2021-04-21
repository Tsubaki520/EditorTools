using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using UniRx;

namespace Wanin.XinStar.Assets.Editor
{
    public class LayerEditor : EditorWindow
    {
        private enum TargetUI { None, NGUI, UGUI }

        private const string NGUI_Layer_Name = "NGUI";
        private const string UGUI_Layer_Name = "Default";

        private TargetUI targetMode = TargetUI.None;
        private string pathValue = string.Empty;
        private Vector2 scrollPos = Vector2.zero;
        private List<PathData> pathList = null;
        private List<bool> selectedList = null;
        private bool updateMissing = true;

        private static EditorWindow window;

        [MenuItem("Tools/粒子特效的Sorting Layer設定")]
        private static void ShowWindow()
        {
            window = GetWindow<LayerEditor>("LayerEditor");
        }

        private void OnEnable()
        {
            pathList = new List<PathData>();
            selectedList = new List<bool>();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            SetSearchGUI();

            EditorGUILayout.Space();
            SetSearchButton();

            ShowSearchOption();
            EditorGUILayout.Space();

            ShowSearchList();
            EditorGUILayout.Space();

            SetUpdateButton();
            EditorGUILayout.Space();
        }

        #region == 內部使用 ==
        private void SetSearchGUI()
        {
            window.maxSize = new Vector2(700f, 500f);
            window.minSize = window.maxSize;
            EditorGUIUtility.labelWidth = 50f;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("選取路徑:", pathValue);

                if (GUILayout.Button("新增", GUILayout.Width(60f)))
                {
                    string path = EditorUtility.OpenFolderPanel("選擇資料夾", Application.dataPath, string.Empty);
                    pathValue = path;
                }

                if (GUILayout.Button("移除", GUILayout.Width(60f)))
                {
                    pathValue = string.Empty;
                    ResetList();
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ResetList()
        {
            pathList.Clear();
            selectedList.Clear();
        }

        private void SetSearchButton()
        {
            if (GUILayout.Button("搜尋"))
            {
                ResetList();

                if (!string.IsNullOrEmpty(pathValue))
                {
                    Search().Subscribe(
                        (list) =>
                        {
                            pathList = list;

                            foreach (var path in list)
                            {
                                selectedList.Add(false);
                            }
                        },
                        (e) => EditorUtility.DisplayDialog("錯誤", $"例外: {e.Message}", "確定"),
                        () => { });
                }
            }
        }

        private IObservable<List<PathData>> Search()
        {
            return Observable.FromCoroutine<List<PathData>>((observer, cancellationToken) => Search(pathValue, observer, cancellationToken));
        }

        private IEnumerator Search(string rootPaths, IObserver<List<PathData>> observer, CancellationToken cancellationToken)
        {
            var results = new List<PathData>();

            // 把絕對路徑改為相對路徑，把路徑反斜線改為正斜線
            string[] relativePaths = { rootPaths.Replace(Application.dataPath, "Assets").Replace("\\", "/") };

            // 取得所有場景檔(scene)
            string[] scenesToSearch = AssetDatabase
                .FindAssets("t:SceneAsset", relativePaths)     // 取得所有Scene
                .Select(x => AssetDatabase.GUIDToAssetPath(x)) // 轉換GUID為路徑
                .ToArray();

            // 搜尋場景檔(scene)
            int count = 0;
            int size = scenesToSearch.Length;
            string message = $"搜尋場景中 {{0}} / {size}";

            foreach (var scenePath in scenesToSearch)
            {
                yield return SearchScene(scenePath, results);

                bool cancelled = EditorUtility.DisplayCancelableProgressBar("搜尋中", string.Format(message, count), (float)count++ / size);
                if (cancellationToken.IsCancellationRequested || cancelled)
                {
                    EditorUtility.ClearProgressBar();
                    yield break;
                }
            }

            // 取得所有預置物(prefab)
            string[] prefabsToSearch = AssetDatabase
                .FindAssets("t:Prefab", relativePaths)     // 取得所有Prefab
                .Select(x => AssetDatabase.GUIDToAssetPath(x)) // 轉換GUID為路徑
                .ToArray();

            // 搜尋場景檔(scene)
            count = 0;
            size = prefabsToSearch.Length;
            message = $"搜尋預置物中 {{0}} / {size}";

            foreach (var prefabPath in prefabsToSearch)
            {
                yield return SearchPrefab(prefabPath, results);

                bool cancelled = EditorUtility.DisplayCancelableProgressBar("搜尋中", string.Format(message, count), (float)count++ / size);
                if (cancellationToken.IsCancellationRequested || cancelled)
                {
                    EditorUtility.ClearProgressBar();
                    yield break;
                }
            }

            yield return null;
            EditorUtility.ClearProgressBar();
            observer.OnNext(results);
            observer.OnCompleted();
        }

        private IEnumerator SearchScene(string scenePath, List<PathData> results)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);

            if (!scene.IsValid() && AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            if (!scene.IsValid())
            {
                Debug.Log($"Scene is not valed: {scenePath}");
                EditorSceneManager.CloseScene(scene, true);
                yield break;
            }

            var roots = scene.GetRootGameObjects();
            var renderers = new List<ParticleSystemRenderer>();

            foreach (var go in roots)
            {
                var temp = go.GetComponentsInChildren<ParticleSystemRenderer>();
                renderers.AddRange(temp);
            }

            if (renderers.Count > 0)
            {
                PathData pathData = new PathData();

                foreach (var renderer in renderers)
                {
                    string rendererPath = TransformTool.GetPath(renderer.transform);
                    pathData.FilePath = scenePath;
                    pathData.RendererPath = rendererPath;
                    results.Add(pathData);
                }
            }
            EditorSceneManager.CloseScene(scene, true);
        }

        private IEnumerator SearchPrefab(string prefabPath, List<PathData> results)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var renderers = go.GetComponentsInChildren<ParticleSystemRenderer>();

            if (renderers.Length > 0)
            {
                PathData pathData = new PathData();

                foreach (var renderer in renderers)
                {
                    string rendererPath = TransformTool.GetPath(renderer.transform);
                    pathData.FilePath = prefabPath;
                    pathData.RendererPath = rendererPath;
                    results.Add(pathData);
                }
            }
            yield return null;
        }

        private void ShowSearchOption()
        {
            if (pathList.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUIUtility.labelWidth = 100f;
                    GUILayout.Label("請選擇 Layer 更新模式", GUILayout.Width(200f));
                    targetMode = (TargetUI)EditorGUILayout.EnumPopup(targetMode);
                    updateMissing = EditorGUILayout.Toggle("是否需要修正遺失", updateMissing);
                    EditorGUIUtility.labelWidth = 50f;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("全選", GUILayout.Width(60f)))
                    {
                        for (int i = 0; i < selectedList.Count; i++)
                        {
                            selectedList[i] = true;
                        }
                    }

                    if (GUILayout.Button("取消全選", GUILayout.Width(60f)))
                    {
                        for (int i = 0; i < selectedList.Count; i++)
                        {
                            selectedList[i] = false;
                        }
                    }

                    if (GUILayout.Button("反向選取", GUILayout.Width(60f)))
                    {
                        for (int i = 0; i < selectedList.Count; i++)
                        {
                            selectedList[i] = !selectedList[i];
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ShowSearchList()
        {
            if (pathList.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
                {
                    for (int i = 0; i < pathList.Count; i++)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.textArea);
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(20f));
                            {
                                GUI.backgroundColor = Color.white;
                                selectedList[i] = EditorGUILayout.ToggleLeft(pathList[i].RendererPath, selectedList[i]);

                                GUI.backgroundColor = Color.green;
                                if (GUILayout.Button("Select", GUILayout.Width(60f)))
                                {
                                    selectedList[i] = !selectedList[i];
                                }

                                GUI.backgroundColor = Color.red;
                                if (GUILayout.Button("Remove", GUILayout.Width(60f)))
                                {
                                    pathList.RemoveAt(i);
                                    selectedList.RemoveAt(i);
                                }
                                GUI.backgroundColor = Color.white;
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
        }

        private void SetUpdateButton()
        {
            if (pathList.Count > 0)
            {
                if (GUILayout.Button("修改Sorting Layer"))
                {
                    if (targetMode == TargetUI.None)
                    {
                        EditorUtility.DisplayDialog("錯誤", $"未設定層級: {targetMode}", "確定");
                        return;
                    }

                    if (selectedList != null)
                    {
                        DoAction(pathList, (x) =>
                        {
                            switch(targetMode)
                            {
                                case TargetUI.NGUI:
                                    x.sortingLayerName = NGUI_Layer_Name;
                                    break;

                                case TargetUI.UGUI:
                                    x.sortingLayerName = UGUI_Layer_Name;
                                    break;
                            }
                            return string.Empty;
                        }).Subscribe(
                               (x) => { },
                               (e) => EditorUtility.DisplayDialog("錯誤", "例外:" + e.Message, "確定"),
                               () => EditorUtility.DisplayDialog("訊息", "完成", "確定"));
                    }
                }
            }
        }

        private IObservable<Unit> DoAction(List<PathData> rendererList, Func<ParticleSystemRenderer, string> action)
        {
            return Observable.FromCoroutine<Unit>((observer, cancellationToken) => DoAction(rendererList, action, observer, cancellationToken));
        }

        private IEnumerator DoAction(List<PathData> pathList, Func<ParticleSystemRenderer, string> action, IObserver<Unit> observer, CancellationToken cancellationToken)
        {
            int size = pathList.Count;
            int count = 0;
            string message = $"執行動作中 {{0}} / {size}";

            for (int i = 0; i < pathList.Count; i++)
            {
                if (selectedList[i])
                {
                    var type = AssetDatabase.GetMainAssetTypeAtPath(pathList[i].FilePath);

                    if (type == typeof(SceneAsset))
                    {
                        yield return DoActionToScene(pathList[i], action);
                    }

                    if (type == typeof(GameObject))
                    {
                        yield return DoActionToPrefab(pathList[i], action);
                    }

                    yield return null;
                    bool cancelled = EditorUtility.DisplayCancelableProgressBar("執行中", string.Format(message, count), (float)count++ / size);

                    if (cancelled || cancellationToken.IsCancellationRequested)
                        yield break;
                }
            }

            EditorUtility.ClearProgressBar();
            observer.OnNext(Unit.Default);
            observer.OnCompleted();
        }

        private IEnumerator DoActionToScene(PathData pathData, Func<ParticleSystemRenderer, string> action)
        {
            var scenePath = pathData.FilePath;
            var scene = SceneManager.GetSceneByPath(scenePath);

            if (!scene.IsValid() && AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath))
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            if (!scene.IsValid())
            {
                Debug.Log($"Scene is not valid: {scenePath}");
                EditorSceneManager.CloseScene(scene, true);
            }

            var roots = scene.GetRootGameObjects();

            foreach (var go in roots)
            {
                var renderers = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
                
                CheckMissingObject(go);

                foreach (var renderer in renderers)
                {
                    if (TransformTool.GetPath(renderer.transform) == pathData.RendererPath)
                    {
                        string error = action.Invoke(renderer);

                        if (!string.IsNullOrEmpty(error))
                        {
                            Debug.LogWarning($"Error: {error} at 【{scenePath}】{TransformTool.GetPath(renderer.transform)}");
                        }
                    }
                }
            }
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, true);
            yield break;
        }

        private IEnumerator DoActionToPrefab(PathData pathData, Func<ParticleSystemRenderer, string> action)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(pathData.FilePath);
            var renderers = go.GetComponentsInChildren<ParticleSystemRenderer>(true);

            CheckMissingObject(go);

            foreach (var renderer in renderers)
            {
                if (TransformTool.GetPath(renderer.transform) == pathData.RendererPath)
                {
                    string error = action.Invoke(renderer);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogWarning($"Error: {error} at 【{pathData.FilePath}】{TransformTool.GetPath(renderer.transform)}");
                    }
                }
            }
            AssetDatabase.SaveAssets();
            yield break;
        }

        private void CheckMissingObject(GameObject gameObject)
        {
            var hasMissing = false;
            var components = gameObject.GetComponentsInChildren<Component>(true);

            foreach (var c in components)
            {
                if (!c)
                {
                    hasMissing = true;
                    Debug.LogWarning($"Missing Component : {c.transform.parent.gameObject.name}");
                }
            }

            if (hasMissing && updateMissing)
            {
                gameObject.AddComponent<UIUnityRenderer>();
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
            }
        }
        #endregion
    }

    public struct PathData
    {
        public string FilePath;
        public string RendererPath;
    }
}