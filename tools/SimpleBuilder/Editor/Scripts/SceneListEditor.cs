using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

public class SceneListEditor : EditorWindow
{
    private string assetName = "";
    private SceneList currentSceneList;
    private List<string> availableScenes;
    private List<string> selectedScenes;
    private ReorderableList reorderableSceneList;
    private bool showError = false;

    [MenuItem("Tools/Scene List Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneListEditor>("Scene List Editor");
        window.Focus();
    }

    public static void Open(SceneList sceneList)
    {
        var window = GetWindow<SceneListEditor>("Scene List Editor");
        window.LoadSceneList(sceneList);
        window.Focus();
    }

    private void OnEnable()
    {
        LoadAvailableScenes();
        selectedScenes = new List<string>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene List Creator", EditorStyles.boldLabel);

        if (currentSceneList != null)
        {
            assetName = currentSceneList.name;
        }
        else
        {
            assetName = EditorGUILayout.TextField("Asset Name", assetName);
        }

        if (string.IsNullOrEmpty(assetName))
        {
            EditorGUILayout.HelpBox("Asset name cannot be empty.", MessageType.Error);
            showError = true;
        }
        else
        {
            showError = false;
        }

        if (currentSceneList == null && GUILayout.Button("Create New Scene List"))
        {
            if (!showError)
            {
                CreateNewSceneList();
            }
        }

        GUILayout.Space(10);

        if (currentSceneList != null)
        {
            GUILayout.Label("Select Scenes and Reorder", EditorStyles.boldLabel);

            DrawSceneSelection();

            if (reorderableSceneList == null)
            {
                InitializeReorderableList();
            }

            reorderableSceneList.DoLayoutList();

            GUILayout.Space(10);

            if (GUILayout.Button("Save Scene List"))
            {
                if (!showError)
                {
                    SaveSceneList();
                    Debug.Log($"Scene list '{assetName}' saved.");
                }
            }
        }
    }

    private void LoadAvailableScenes()
    {
        availableScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            availableScenes.Add(scene.path);
        }
    }

    private void DrawSceneSelection()
    {
        GUILayout.Label("Available Scenes:", EditorStyles.boldLabel);

        foreach (var scene in availableScenes)
        {
            bool isSelected = selectedScenes.Contains(scene);
            bool newIsSelected = EditorGUILayout.ToggleLeft(scene, isSelected);

            if (newIsSelected && !isSelected)
            {
                selectedScenes.Add(scene);
            }
            else if (!newIsSelected && isSelected)
            {
                selectedScenes.Remove(scene);
            }
        }
    }

    private void LoadSceneList(SceneList sceneList)
    {
        currentSceneList = sceneList;
        assetName = sceneList.name;

        selectedScenes.Clear();
        if (currentSceneList.scenes != null)
        {
            selectedScenes.AddRange(currentSceneList.scenes);
        }

        InitializeReorderableList();
    }

    private void CreateNewSceneList()
    {
        string path = $"Assets/Resources/{assetName}.asset";

        currentSceneList = AssetDatabase.LoadAssetAtPath<SceneList>(path);

        if (currentSceneList == null)
        {
            currentSceneList = ScriptableObject.CreateInstance<SceneList>();
            currentSceneList.scenes = selectedScenes.ToArray();
            AssetDatabase.CreateAsset(currentSceneList, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"Scene list '{assetName}' created at '{path}'.");

            LoadSceneList(currentSceneList);
            EditorGUIUtility.PingObject(currentSceneList);
        }
        else
        {
            Debug.Log($"Scene list '{assetName}' already exists at '{path}'.");
        }
    }

    private void InitializeReorderableList()
    {
        reorderableSceneList = new ReorderableList(selectedScenes, typeof(string), true, true, false, true);

        reorderableSceneList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Selected Scenes");
        };

        reorderableSceneList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), selectedScenes[index]);
        };

        reorderableSceneList.onReorderCallback = (ReorderableList list) =>
        {
            EditorUtility.SetDirty(currentSceneList);
        };
    }

    private void SaveSceneList()
    {
        currentSceneList.scenes = selectedScenes.ToArray();
        EditorUtility.SetDirty(currentSceneList);
        AssetDatabase.SaveAssets();
    }
}
