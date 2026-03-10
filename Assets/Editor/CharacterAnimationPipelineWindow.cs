using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public sealed class CharacterAnimationPipelineWindow : EditorWindow
{
    private readonly List<DefaultAsset> characterFolders = new List<DefaultAsset>();

    private AnimatorController baseController;
    private DefaultAsset outputFolder;
    private Vector2Int cellSize = new Vector2Int(64, 64);
    private Vector2 scrollPosition;

    [MenuItem("Tools/Nexus/Character Animation Pipeline")]
    private static void OpenWindow()
    {
        var window = GetWindow<CharacterAnimationPipelineWindow>("Anim Pipeline");
        window.minSize = new Vector2(560f, 380f);
        window.SyncFromSelection();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Slice character sheets, generate clips, then create Animator Override Controllers.", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space(8f);

        baseController = (AnimatorController)EditorGUILayout.ObjectField("Base Controller", baseController, typeof(AnimatorController), false);
        outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);
        cellSize = EditorGUILayout.Vector2IntField("Cell Size", cellSize);

        EditorGUILayout.Space(8f);
        DrawDropArea();
        EditorGUILayout.Space(8f);
        DrawFolderList();
        EditorGUILayout.Space(8f);

        if (GUILayout.Button("Use Current Selection"))
        {
            SyncFromSelection();
        }

        using (new EditorGUI.DisabledScope(baseController == null || characterFolders.Count == 0))
        {
            if (GUILayout.Button("Build All"))
            {
                BuildAll();
            }
        }

        using (new EditorGUI.DisabledScope(characterFolders.Count == 0))
        {
            if (GUILayout.Button("Clear List"))
            {
                characterFolders.Clear();
            }
        }
    }

    private void DrawDropArea()
    {
        var rect = GUILayoutUtility.GetRect(0f, 84f, GUILayout.ExpandWidth(true));
        GUI.Box(rect, "Drag character folders here");

        var evt = Event.current;
        if (!rect.Contains(evt.mousePosition))
        {
            return;
        }

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            var folders = ExtractFolders(DragAndDrop.objectReferences);
            DragAndDrop.visualMode = folders.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

            if (evt.type == EventType.DragPerform && folders.Count > 0)
            {
                DragAndDrop.AcceptDrag();
                AddFolders(folders);
                Repaint();
            }

            evt.Use();
        }
    }

    private void DrawFolderList()
    {
        EditorGUILayout.LabelField("Character Folders", EditorStyles.boldLabel);

        if (characterFolders.Count == 0)
        {
            EditorGUILayout.HelpBox("Chua co folder nhan vat nao trong danh sach.", MessageType.Info);
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MinHeight(190f));
        for (var i = 0; i < characterFolders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(characterFolders[i], typeof(DefaultAsset), false);
            if (GUILayout.Button("X", GUILayout.Width(28f)))
            {
                characterFolders.RemoveAt(i);
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void SyncFromSelection()
    {
        characterFolders.Clear();
        AddFolders(ExtractFolders(Selection.objects));
        Repaint();
    }

    private void AddFolders(IList<DefaultAsset> folders)
    {
        for (var i = 0; i < folders.Count; i++)
        {
            if (folders[i] != null && !characterFolders.Contains(folders[i]))
            {
                characterFolders.Add(folders[i]);
            }
        }
    }

    private static List<DefaultAsset> ExtractFolders(Object[] objects)
    {
        var folders = new List<DefaultAsset>();
        for (var i = 0; i < objects.Length; i++)
        {
            var folder = objects[i] as DefaultAsset;
            if (folder == null)
            {
                continue;
            }

            var path = AssetDatabase.GetAssetPath(folder);
            if (!AssetDatabase.IsValidFolder(path) || folders.Contains(folder))
            {
                continue;
            }

            folders.Add(folder);
        }

        return folders;
    }

    private void BuildAll()
    {
        if (cellSize.x <= 0 || cellSize.y <= 0)
        {
            EditorUtility.DisplayDialog("Anim Pipeline", "Cell Size phai lon hon 0.", "OK");
            return;
        }

        var outputRoot = ResolveOutputRoot();
        if (string.IsNullOrEmpty(outputRoot))
        {
            EditorUtility.DisplayDialog("Anim Pipeline", "Output Folder khong hop le.", "OK");
            return;
        }

        var templates = BuildClipTemplates(baseController);
        if (templates.Count == 0)
        {
            EditorUtility.DisplayDialog("Anim Pipeline", "Base Controller khong co clip hop le.", "OK");
            return;
        }

        var builtCount = 0;
        for (var i = 0; i < characterFolders.Count; i++)
        {
            var folderPath = AssetDatabase.GetAssetPath(characterFolders[i]);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                continue;
            }

            if (BuildCharacter(folderPath, outputRoot, templates))
            {
                builtCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Character Animation Pipeline built " + builtCount + " character set(s).");
    }

    private string ResolveOutputRoot()
    {
        if (outputFolder != null)
        {
            var selectedPath = AssetDatabase.GetAssetPath(outputFolder);
            if (AssetDatabase.IsValidFolder(selectedPath))
            {
                return selectedPath;
            }
        }

        return EnsureFolder("Assets", "Animators/Generated");
    }

    private bool BuildCharacter(string characterFolderPath, string outputRoot, IList<ClipTemplateInfo> templates)
    {
        var characterName = Path.GetFileName(characterFolderPath);
        var characterOutputRoot = EnsureFolder(outputRoot, characterName);
        var clipsOutputFolder = EnsureFolder(characterOutputRoot, "Clips");

        var spritesByAction = new Dictionary<string, List<Sprite>>();
        for (var i = 0; i < templates.Count; i++)
        {
            var actionName = templates[i].actionName;
            if (spritesByAction.ContainsKey(actionName))
            {
                continue;
            }

            var texturePath = characterFolderPath + "/" + actionName + ".png";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogWarning("Anim Pipeline: missing texture " + texturePath);
                return false;
            }

            if (!SliceTexture(texture, cellSize))
            {
                return false;
            }

            spritesByAction[actionName] = LoadSprites(texturePath);
            if (spritesByAction[actionName].Count == 0)
            {
                Debug.LogWarning("Anim Pipeline: no sprites found after slicing " + texturePath);
                return false;
            }
        }

        var generatedClips = new Dictionary<string, AnimationClip>();
        for (var i = 0; i < templates.Count; i++)
        {
            var template = templates[i];
            List<Sprite> targetSprites;
            if (!spritesByAction.TryGetValue(template.actionName, out targetSprites))
            {
                Debug.LogWarning("Anim Pipeline: missing action sprites for " + characterFolderPath + " / " + template.actionName);
                return false;
            }

            var clipPath = clipsOutputFolder + "/" + template.templateClip.name + ".anim";
            var generatedClip = CreateAnimationClipAsset(clipPath, template, targetSprites);
            if (generatedClip == null)
            {
                Debug.LogWarning("Anim Pipeline: failed to create clip " + clipPath);
                return false;
            }

            generatedClips[template.templateClip.name] = generatedClip;
        }

        CreateOrReplaceOverrideController(characterOutputRoot, characterName, generatedClips);
        return true;
    }

    private void CreateOrReplaceOverrideController(string characterOutputRoot, string characterName, IDictionary<string, AnimationClip> generatedClips)
    {
        var overridePath = characterOutputRoot + "/" + characterName + " Override.overrideController";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(overridePath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(overridePath);
        }

        var overrideController = new AnimatorOverrideController();
        overrideController.runtimeAnimatorController = baseController;

        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        var baseClips = GetDistinctBaseClips(baseController);
        for (var i = 0; i < baseClips.Count; i++)
        {
            var originalClip = baseClips[i];
            AnimationClip replacementClip;
            if (!generatedClips.TryGetValue(originalClip.name, out replacementClip))
            {
                replacementClip = originalClip;
            }

            overrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(originalClip, replacementClip));
        }

        overrideController.ApplyOverrides(overrides);
        AssetDatabase.CreateAsset(overrideController, overridePath);
        Debug.Log("Anim Pipeline created override: " + overridePath);
    }

    private static AnimationClip CreateAnimationClipAsset(string clipPath, ClipTemplateInfo template, IList<Sprite> targetSprites)
    {
        if (targetSprites == null || targetSprites.Count == 0)
        {
            return null;
        }

        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(clipPath);
        }

        var clip = new AnimationClip
        {
            frameRate = template.templateClip.frameRate,
            name = template.templateClip.name
        };

        var keyframes = new ObjectReferenceKeyframe[template.keyframes.Length];
        for (var i = 0; i < template.keyframes.Length; i++)
        {
            var spriteIndex = template.spriteIndices[i];
            var resolvedIndex = Mathf.Clamp(spriteIndex, 0, targetSprites.Count - 1);
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = template.keyframes[i].time,
                value = targetSprites[resolvedIndex]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, template.binding, keyframes);

        var settings = AnimationUtility.GetAnimationClipSettings(template.templateClip);
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, clipPath);
        return AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
    }

    private static List<ClipTemplateInfo> BuildClipTemplates(AnimatorController controller)
    {
        var templates = new List<ClipTemplateInfo>();
        var baseClips = GetDistinctBaseClips(controller);
        for (var i = 0; i < baseClips.Count; i++)
        {
            var clip = baseClips[i];
            var template = BuildClipTemplate(clip);
            if (template != null)
            {
                templates.Add(template);
            }
        }

        return templates;
    }

    private static ClipTemplateInfo BuildClipTemplate(AnimationClip clip)
    {
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        if (bindings == null || bindings.Length == 0)
        {
            return null;
        }

        var binding = bindings[0];
        var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        if (keyframes == null || keyframes.Length == 0)
        {
            return null;
        }

        var firstSprite = keyframes[0].value as Sprite;
        if (firstSprite == null)
        {
            return null;
        }

        var sourceTexturePath = AssetDatabase.GetAssetPath(firstSprite);
        var actionName = Path.GetFileNameWithoutExtension(sourceTexturePath);
        var sourceSprites = LoadSprites(sourceTexturePath);
        var sourceSpriteIndexMap = new Dictionary<Sprite, int>();
        for (var i = 0; i < sourceSprites.Count; i++)
        {
            if (!sourceSpriteIndexMap.ContainsKey(sourceSprites[i]))
            {
                sourceSpriteIndexMap.Add(sourceSprites[i], i);
            }
        }

        var spriteIndices = new int[keyframes.Length];
        for (var i = 0; i < keyframes.Length; i++)
        {
            var sprite = keyframes[i].value as Sprite;
            int spriteIndex;
            if (sprite == null || !sourceSpriteIndexMap.TryGetValue(sprite, out spriteIndex))
            {
                return null;
            }

            spriteIndices[i] = spriteIndex;
        }

        return new ClipTemplateInfo
        {
            templateClip = clip,
            actionName = actionName,
            binding = binding,
            keyframes = keyframes,
            spriteIndices = spriteIndices
        };
    }

    private static List<AnimationClip> GetDistinctBaseClips(AnimatorController controller)
    {
        var clips = new List<AnimationClip>();
        var controllerClips = controller.animationClips;
        for (var i = 0; i < controllerClips.Length; i++)
        {
            if (controllerClips[i] != null && !clips.Contains(controllerClips[i]))
            {
                clips.Add(controllerClips[i]);
            }
        }

        return clips;
    }

    private static List<Sprite> LoadSprites(string texturePath)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .OrderByDescending(sprite => sprite.rect.y)
            .ThenBy(sprite => sprite.rect.x)
            .ToList();

        return sprites;
    }

    private static bool SliceTexture(Texture2D texture, Vector2Int requestedCellSize)
    {
        var assetPath = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogWarning("Anim Pipeline: invalid texture path.");
            return false;
        }

        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("Anim Pipeline: missing TextureImporter for " + assetPath);
            return false;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.SaveAndReimport();

        var importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (importedTexture == null)
        {
            Debug.LogWarning("Anim Pipeline: failed to import " + assetPath);
            return false;
        }

        if (importedTexture.width < requestedCellSize.x || importedTexture.height < requestedCellSize.y)
        {
            Debug.LogWarning("Anim Pipeline: texture smaller than cell size: " + assetPath);
            return false;
        }

        var factory = new SpriteDataProviderFactories();
        factory.Init();

        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importedTexture);
        if (dataProvider == null)
        {
            Debug.LogWarning("Anim Pipeline: failed to create sprite data provider for " + assetPath);
            return false;
        }

        dataProvider.InitSpriteEditorDataProvider();

        var spriteRects = BuildSpriteRects(importedTexture, requestedCellSize, Path.GetFileNameWithoutExtension(assetPath));
        dataProvider.SetSpriteRects(spriteRects.ToArray());

        var nameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        if (nameFileIdDataProvider != null)
        {
            var pairs = new List<SpriteNameFileIdPair>(spriteRects.Count);
            for (var i = 0; i < spriteRects.Count; i++)
            {
                pairs.Add(new SpriteNameFileIdPair(spriteRects[i].name, spriteRects[i].spriteID));
            }

            nameFileIdDataProvider.SetNameFileIdPairs(pairs);
        }

        dataProvider.Apply();
        importer.SaveAndReimport();
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
                rects.Add(new SpriteRect
                {
                    name = textureName + "_" + (row * columns + column),
                    rect = new Rect(x, y, cell.x, cell.y),
                    alignment = SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = Vector4.zero,
                    spriteID = GUID.Generate()
                });
            }
        }

        return rects;
    }

    private static string EnsureFolder(string root, string nestedPath)
    {
        var normalizedRoot = root.Replace("\\", "/");
        var current = normalizedRoot;
        var parts = nestedPath.Split(new[] { '/', '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }

        return current;
    }

    private sealed class ClipTemplateInfo
    {
        public AnimationClip templateClip;
        public string actionName;
        public EditorCurveBinding binding;
        public ObjectReferenceKeyframe[] keyframes;
        public int[] spriteIndices;
    }
}
