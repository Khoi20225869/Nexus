using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public sealed class SpriteGridAutoSlicerWindow : EditorWindow
{
    private static readonly Vector2Int DefaultCellSize = new Vector2Int(64, 64);

    private readonly List<Texture2D> targetTextures = new List<Texture2D>();
    private Vector2Int cellSize = DefaultCellSize;
    private bool autoSliceOnDrop = true;
    private bool openSpriteEditorAfterSlice = true;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Nexus/Sprite Grid Auto Slicer")]
    private static void OpenWindow()
    {
        var window = GetWindow<SpriteGridAutoSlicerWindow>("Sprite Grid Slicer");
        window.minSize = new Vector2(420f, 300f);
        window.SyncFromSelection();
    }

    [MenuItem("Assets/Nexus/Slice Sprite Sheet 64x64", true)]
    private static bool ValidateSliceSelectedTexture()
    {
        return ExtractTextures(Selection.objects).Count > 0;
    }

    [MenuItem("Assets/Nexus/Slice Sprite Sheet 64x64")]
    private static void SliceSelectedTexture()
    {
        var textures = ExtractTextures(Selection.objects);
        SliceTextures(textures, DefaultCellSize, true);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Auto slice multiple sprite sheets by grid cell size.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(8f);

        cellSize = EditorGUILayout.Vector2IntField("Cell Size", cellSize);
        autoSliceOnDrop = EditorGUILayout.Toggle("Auto Slice On Drop", autoSliceOnDrop);
        openSpriteEditorAfterSlice = EditorGUILayout.Toggle("Open Sprite Editor", openSpriteEditorAfterSlice);

        EditorGUILayout.Space(8f);
        DrawDropArea();
        EditorGUILayout.Space(8f);
        DrawTextureList();
        EditorGUILayout.Space(8f);

        using (new EditorGUI.DisabledScope(targetTextures.Count == 0))
        {
            if (GUILayout.Button("Slice All"))
            {
                SliceTextures(targetTextures, cellSize, openSpriteEditorAfterSlice);
            }
        }

        if (GUILayout.Button("Use Current Selection"))
        {
            SyncFromSelection();
        }

        using (new EditorGUI.DisabledScope(targetTextures.Count == 0))
        {
            if (GUILayout.Button("Clear List"))
            {
                targetTextures.Clear();
            }
        }
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    private void DrawDropArea()
    {
        var rect = GUILayoutUtility.GetRect(0f, 84f, GUILayout.ExpandWidth(true));
        GUI.Box(rect, "Drag multiple textures or sprites here");

        var evt = Event.current;
        if (!rect.Contains(evt.mousePosition))
        {
            return;
        }

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            var draggedTextures = ExtractTextures(DragAndDrop.objectReferences);
            DragAndDrop.visualMode = draggedTextures.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

            if (evt.type == EventType.DragPerform && draggedTextures.Count > 0)
            {
                DragAndDrop.AcceptDrag();
                AddTextures(draggedTextures);
                Repaint();

                if (autoSliceOnDrop)
                {
                    SliceTextures(draggedTextures, cellSize, openSpriteEditorAfterSlice);
                }
            }

            evt.Use();
        }
    }

    private void DrawTextureList()
    {
        EditorGUILayout.LabelField("Queued Textures", EditorStyles.boldLabel);

        if (targetTextures.Count == 0)
        {
            EditorGUILayout.HelpBox("Chua co texture nao trong danh sach.", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(120f));
        for (var i = 0; i < targetTextures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(targetTextures[i], typeof(Texture2D), false);
            if (GUILayout.Button("X", GUILayout.Width(28f)))
            {
                targetTextures.RemoveAt(i);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void SyncFromSelection()
    {
        targetTextures.Clear();
        AddTextures(ExtractTextures(Selection.objects));
        Repaint();
    }

    private void AddTextures(IList<Texture2D> textures)
    {
        for (var i = 0; i < textures.Count; i++)
        {
            if (textures[i] != null && !targetTextures.Contains(textures[i]))
            {
                targetTextures.Add(textures[i]);
            }
        }
    }

    private static List<Texture2D> ExtractTextures(Object[] objects)
    {
        var textures = new List<Texture2D>();
        for (var i = 0; i < objects.Length; i++)
        {
            var texture = ExtractTexture(objects[i]);
            if (texture != null && !textures.Contains(texture))
            {
                textures.Add(texture);
            }
        }

        return textures;
    }

    private static Texture2D ExtractTexture(Object obj)
    {
        var texture = obj as Texture2D;
        if (texture != null)
        {
            return AssetDatabase.Contains(texture) ? texture : null;
        }

        var sprite = obj as Sprite;
        if (sprite != null)
        {
            var path = AssetDatabase.GetAssetPath(sprite);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        return null;
    }

    private static void SliceTextures(IList<Texture2D> textures, Vector2Int requestedCellSize, bool openSpriteEditor)
    {
        if (textures == null || textures.Count == 0)
        {
            EditorUtility.DisplayDialog("Sprite Grid Slicer", "Chua co texture de cat.", "OK");
            return;
        }

        var slicedCount = 0;
        Texture2D lastSlicedTexture = null;

        for (var i = 0; i < textures.Count; i++)
        {
            if (SliceTexture(textures[i], requestedCellSize))
            {
                slicedCount++;
                lastSlicedTexture = textures[i];
            }
        }

        if (slicedCount == 0)
        {
            EditorUtility.DisplayDialog("Sprite Grid Slicer", "Khong co texture nao cat duoc.", "OK");
            return;
        }

        Debug.Log("Sliced " + slicedCount + " texture(s) with grid " + requestedCellSize.x + "x" + requestedCellSize.y + ".");

        if (openSpriteEditor && lastSlicedTexture != null)
        {
            var textureToOpen = lastSlicedTexture;
            EditorApplication.delayCall += () =>
            {
                Selection.activeObject = textureToOpen;
                EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor");
            };
        }
    }

    private static bool SliceTexture(Texture2D texture, Vector2Int requestedCellSize)
    {
        if (texture == null)
        {
            return false;
        }

        if (requestedCellSize.x <= 0 || requestedCellSize.y <= 0)
        {
            Debug.LogWarning("Sprite Grid Slicer: invalid cell size.");
            return false;
        }

        var assetPath = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogWarning("Sprite Grid Slicer: texture is not inside Assets.");
            return false;
        }

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("Sprite Grid Slicer: missing TextureImporter for " + assetPath);
            return false;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.SaveAndReimport();

        var importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (importedTexture == null)
        {
            Debug.LogWarning("Sprite Grid Slicer: failed to import " + assetPath);
            return false;
        }

        if (importedTexture.width < requestedCellSize.x || importedTexture.height < requestedCellSize.y)
        {
            Debug.LogWarning("Sprite Grid Slicer: texture smaller than cell size: " + assetPath);
            return false;
        }

        var factory = new SpriteDataProviderFactories();
        factory.Init();

        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importedTexture);
        if (dataProvider == null)
        {
            Debug.LogWarning("Sprite Grid Slicer: failed to create data provider for " + assetPath);
            return false;
        }

        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = BuildSpriteRects(importedTexture, requestedCellSize, Path.GetFileNameWithoutExtension(assetPath));
        dataProvider.SetSpriteRects(spriteRects.ToArray());

        var nameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (nameFileIdDataProvider != null)
        {
            var nameFileIds = new List<SpriteNameFileIdPair>(spriteRects.Count);
            for (var i = 0; i < spriteRects.Count; i++)
            {
                nameFileIds.Add(new SpriteNameFileIdPair(spriteRects[i].name, spriteRects[i].spriteID));
            }

            nameFileIdDataProvider.SetNameFileIdPairs(nameFileIds);
        }

        dataProvider.Apply();
        importer.SaveAndReimport();

        Selection.activeObject = importedTexture;
        EditorGUIUtility.PingObject(importedTexture);
        Debug.Log("Sliced " + spriteRects.Count + " sprites from " + assetPath + " using " + requestedCellSize.x + "x" + requestedCellSize.y + " grid.");
        return true;
    }

    private static List<SpriteRect> BuildSpriteRects(Texture2D texture, Vector2Int cell, string textureName)
    {
        var rects = new List<SpriteRect>();
        var columns = texture.width / cell.x;
        var rows = texture.height / cell.y;

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                var y = texture.height - ((row + 1) * cell.y);
                var x = column * cell.x;
                var spriteRect = new SpriteRect
                {
                    name = textureName + "_" + row + "_" + column,
                    rect = new Rect(x, y, cell.x, cell.y),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = Vector4.zero,
                    spriteID = GUID.Generate()
                };

                rects.Add(spriteRect);
            }
        }

        return rects;
    }
}
