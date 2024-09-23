using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SceneList))]
public class SceneListCustomEditor : Editor
{
    private List<string> availableScenes;
    private List<string> selectedScenes;
    private ReorderableList reorderableSceneList;

    private void OnEnable()
    {
        LoadAvailableScenes();
        SceneList sceneList = (SceneList)target;
        selectedScenes = new List<string>(sceneList.scenes);
        InitializeReorderableList();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Scene List Editor", EditorStyles.boldLabel);

        DrawSceneSelection();

        GUILayout.Space(10);

        reorderableSceneList.DoLayoutList();

        GUILayout.Space(10);

        if (GUILayout.Button("Save Scene List"))
        {
            SaveSceneList();
            Debug.Log($"Scene list '{target.name}' saved.");
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

    private void InitializeReorderableList()
    {
        reorderableSceneList = new ReorderableList(selectedScenes, typeof(string), true, true, false, true);

        reorderableSceneList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Selected Scenes (Reorderable)");
        };

        reorderableSceneList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += 2;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), selectedScenes[index]);
        };

        reorderableSceneList.onReorderCallback = (ReorderableList list) =>
        {
            EditorUtility.SetDirty(target);
        };
    }

    private void SaveSceneList()
    {
        SceneList sceneList = (SceneList)target;
        sceneList.scenes = selectedScenes.ToArray();
        EditorUtility.SetDirty(sceneList);
        AssetDatabase.SaveAssets();
    }
}