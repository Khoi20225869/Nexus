using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public sealed class BatchRenameWindow : EditorWindow
{
    private readonly List<Object> targets = new List<Object>();

    private string prefix = string.Empty;
    private string baseName = string.Empty;
    private string suffix = string.Empty;
    private string findText = string.Empty;
    private string replaceText = string.Empty;
    private bool useNumbering = true;
    private int startNumber = 1;
    private int numberPadding = 2;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Nexus/Batch Rename")]
    private static void OpenWindow()
    {
        var window = GetWindow<BatchRenameWindow>("Batch Rename");
        window.minSize = new Vector2(460f, 360f);
        window.SyncFromSelection();
    }

    [MenuItem("Assets/Nexus/Batch Rename", true)]
    private static bool ValidateRenameSelected()
    {
        return Selection.objects != null && Selection.objects.Length > 0;
    }

    [MenuItem("Assets/Nexus/Batch Rename")]
    private static void OpenFromSelection()
    {
        var window = GetWindow<BatchRenameWindow>("Batch Rename");
        window.minSize = new Vector2(460f, 360f);
        window.SyncFromSelection();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Rename multiple assets in one pass.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(8f);

        prefix = EditorGUILayout.TextField("Prefix", prefix);
        baseName = EditorGUILayout.TextField("Base Name", baseName);
        suffix = EditorGUILayout.TextField("Suffix", suffix);
        findText = EditorGUILayout.TextField("Find", findText);
        replaceText = EditorGUILayout.TextField("Replace", replaceText);
        useNumbering = EditorGUILayout.Toggle("Add Numbering", useNumbering);

        using (new EditorGUI.DisabledScope(!useNumbering))
        {
            startNumber = EditorGUILayout.IntField("Start Number", startNumber);
            numberPadding = EditorGUILayout.IntField("Number Padding", numberPadding);
        }

        EditorGUILayout.Space(8f);
        DrawDropArea();
        EditorGUILayout.Space(8f);
        DrawTargetList();
        EditorGUILayout.Space(8f);

        if (GUILayout.Button("Use Current Selection"))
        {
            SyncFromSelection();
        }

        using (new EditorGUI.DisabledScope(targets.Count == 0))
        {
            if (GUILayout.Button("Rename All"))
            {
                RenameAll();
            }
        }

        using (new EditorGUI.DisabledScope(targets.Count == 0))
        {
            if (GUILayout.Button("Clear List"))
            {
                targets.Clear();
            }
        }
    }

    private void DrawDropArea()
    {
        var rect = GUILayoutUtility.GetRect(0f, 84f, GUILayout.ExpandWidth(true));
        GUI.Box(rect, "Drag assets here");

        var evt = Event.current;
        if (!rect.Contains(evt.mousePosition))
        {
            return;
        }

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            var draggedTargets = ExtractAssets(DragAndDrop.objectReferences);
            DragAndDrop.visualMode = draggedTargets.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

            if (evt.type == EventType.DragPerform && draggedTargets.Count > 0)
            {
                DragAndDrop.AcceptDrag();
                AddTargets(draggedTargets);
                Repaint();
            }

            evt.Use();
        }
    }

    private void DrawTargetList()
    {
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        if (targets.Count == 0)
        {
            EditorGUILayout.HelpBox("Chua co asset nao trong danh sach.", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(180f));
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            var previewName = BuildNewName(target, i);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(target, typeof(Object), false);
            EditorGUILayout.LabelField("-> " + previewName);
            if (GUILayout.Button("X", GUILayout.Width(28f)))
            {
                targets.RemoveAt(i);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void SyncFromSelection()
    {
        targets.Clear();
        AddTargets(ExtractAssets(Selection.objects));

        Repaint();
    }

    private void AddTargets(IList<Object> assets)
    {
        for (var i = 0; i < assets.Count; i++)
        {
            if (assets[i] != null && !targets.Contains(assets[i]))
            {
                targets.Add(assets[i]);
            }
        }
    }

    private static List<Object> ExtractAssets(Object[] objects)
    {
        var assets = new List<Object>();
        for (var i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null && AssetDatabase.Contains(objects[i]) && !assets.Contains(objects[i]))
            {
                assets.Add(objects[i]);
            }
        }

        return assets;
    }

    private void RenameAll()
    {
        if (targets.Count == 0)
        {
            return;
        }

        AssetDatabase.StartAssetEditing();
        try
        {
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null)
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(target);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var newName = BuildUniqueName(path, BuildNewName(target, i));
                var error = AssetDatabase.RenameAsset(path, newName);
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogWarning("Batch Rename failed for " + path + ": " + error);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        SyncFromSelection();
    }

    private string BuildNewName(Object target, int index)
    {
        var currentName = target != null ? target.name : string.Empty;
        var workingName = string.IsNullOrEmpty(baseName) ? currentName : baseName;

        if (!string.IsNullOrEmpty(findText))
        {
            workingName = workingName.Replace(findText, replaceText);
        }

        workingName = prefix + workingName + suffix;

        if (useNumbering)
        {
            var number = Mathf.Max(0, startNumber + index);
            var padding = Mathf.Max(1, numberPadding);
            workingName += number.ToString().PadLeft(padding, '0');
        }

        return string.IsNullOrWhiteSpace(workingName) ? "RenamedAsset" : workingName.Trim();
    }

    private static string BuildUniqueName(string assetPath, string desiredName)
    {
        var folder = System.IO.Path.GetDirectoryName(assetPath);
        var extension = System.IO.Path.GetExtension(assetPath);
        var normalizedFolder = string.IsNullOrEmpty(folder) ? string.Empty : folder.Replace("\\", "/");
        var desiredPath = normalizedFolder + "/" + desiredName + extension;
        var uniquePath = AssetDatabase.GenerateUniqueAssetPath(desiredPath);
        return System.IO.Path.GetFileNameWithoutExtension(uniquePath);
    }
}
