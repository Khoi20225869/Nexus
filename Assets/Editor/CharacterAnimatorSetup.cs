using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CharacterAnimatorSetup
{
    private const string ControllerPath = "Assets/Animators/Character Controller.controller";
    private const string ClipRoot = "Assets/Animators/Character Animation";
    private const string SetupKey = "Nexus.CharacterAnimatorSetup.Done";

    private const string MoveX = "MoveX";
    private const string MoveY = "MoveY";
    private const string Speed = "Speed";
    private const string Attack = "Attack";
    private const string Hurt = "Hurt";
    private const string Dead = "Dead";

    [InitializeOnLoadMethod]
    private static void SetupOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (SessionState.GetBool(SetupKey, false))
            {
                return;
            }

            BuildAnimatorController();
            SessionState.SetBool(SetupKey, true);
        };
    }

    [MenuItem("Tools/Nexus/Setup Character Animator")]
    public static void BuildAnimatorController()
    {
        FixClipLoopTime();

        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
        }

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        ResetParameters(controller);
        EnsureParameter(controller, MoveX, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, MoveY, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, Speed, AnimatorControllerParameterType.Float);
        EnsureParameter(controller, Attack, AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, Hurt, AnimatorControllerParameterType.Trigger);
        EnsureParameter(controller, Dead, AnimatorControllerParameterType.Bool);

        var root = controller.layers[0].stateMachine;
        root.name = "Base Layer";
        root.anyStatePosition = new Vector3(50f, 20f, 0f);
        root.entryPosition = new Vector3(-220f, 220f, 0f);

        var idle = AddDirectionalState(controller, root, "Idle", "Idle", new Vector3(120f, 120f, 0f));
        var walk = AddDirectionalState(controller, root, "Walk", "Walk", new Vector3(320f, 120f, 0f));
        var run = AddDirectionalState(controller, root, "Run", "Run", new Vector3(520f, 120f, 0f));
        var attack = AddDirectionalState(controller, root, "Attack", "Attack", new Vector3(320f, 300f, 0f));
        var walkAttack = AddDirectionalState(controller, root, "Walk Attack", "Walk Attack", new Vector3(520f, 300f, 0f));
        var runAttack = AddDirectionalState(controller, root, "Run Attack", "Run Attack", new Vector3(720f, 300f, 0f));
        var hurt = AddDirectionalState(controller, root, "Hurt", "Hurt", new Vector3(120f, 300f, 0f));
        var death = AddDirectionalState(controller, root, "Death", "Death", new Vector3(120f, 480f, 0f));

        root.defaultState = idle;

        AddTransition(idle, walk, false, Cond(AnimatorConditionMode.Greater, 0.1f, Speed));
        AddTransition(idle, run, false, Cond(AnimatorConditionMode.Greater, 0.6f, Speed));
        AddTransition(idle, attack, false, Cond(AnimatorConditionMode.If, 0f, Attack));

        AddTransition(walk, idle, false, Cond(AnimatorConditionMode.Less, 0.1f, Speed));
        AddTransition(walk, run, false, Cond(AnimatorConditionMode.Greater, 0.6f, Speed));
        AddTransition(walk, walkAttack, false, Cond(AnimatorConditionMode.If, 0f, Attack));

        AddTransition(run, walk, false, Cond(AnimatorConditionMode.Less, 0.6f, Speed));
        AddTransition(run, idle, false, Cond(AnimatorConditionMode.Less, 0.1f, Speed));
        AddTransition(run, runAttack, false, Cond(AnimatorConditionMode.If, 0f, Attack));

        AddReturnBySpeedTransitions(attack, idle, walk, run);
        AddReturnBySpeedTransitions(walkAttack, idle, walk, run);
        AddReturnBySpeedTransitions(runAttack, idle, walk, run);
        AddReturnBySpeedTransitions(hurt, idle, walk, run);

        var anyToHurt = root.AddAnyStateTransition(hurt);
        anyToHurt.hasExitTime = false;
        anyToHurt.duration = 0f;
        anyToHurt.AddCondition(AnimatorConditionMode.If, 0f, Hurt);

        var anyToDeath = root.AddAnyStateTransition(death);
        anyToDeath.hasExitTime = false;
        anyToDeath.duration = 0f;
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, Dead);

        AddTransition(death, idle, false, Cond(AnimatorConditionMode.IfNot, 0f, Dead));

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Character Animator setup completed.");
    }

    private static void FixClipLoopTime()
    {
        var guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { ClipRoot });
        for (var i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                continue;
            }

            var shouldLoop = path.Contains("/Idle/") || path.Contains("/Walk/") || path.Contains("/Run/");
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            if (settings.loopTime == shouldLoop)
            {
                continue;
            }

            settings.loopTime = shouldLoop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
        }
    }

    private static AnimatorState AddDirectionalState(
        AnimatorController controller,
        AnimatorStateMachine root,
        string stateName,
        string clipGroupName,
        Vector3 position)
    {
        var state = root.AddState(stateName, position);
        var tree = new BlendTree
        {
            name = stateName + " Blend Tree",
            blendType = BlendTreeType.SimpleDirectional2D,
            blendParameter = MoveX,
            blendParameterY = MoveY,
            useAutomaticThresholds = false
        };

        AssetDatabase.AddObjectToAsset(tree, controller);
        state.motion = tree;

        tree.AddChild(LoadClip(clipGroupName, "Default"), new Vector2(0f, -1f));
        tree.AddChild(LoadClip(clipGroupName, "Top"), new Vector2(0f, 1f));
        tree.AddChild(LoadClip(clipGroupName, "Left"), new Vector2(-1f, 0f));
        tree.AddChild(LoadClip(clipGroupName, "Right"), new Vector2(1f, 0f));
        return state;
    }

    private static AnimationClip LoadClip(string group, string direction)
    {
        var path = $"{ClipRoot}/{group}/{group} {direction}.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip != null)
        {
            return clip;
        }

        if (direction == "Left" || direction == "Right")
        {
            var fallback = $"{ClipRoot}/{group}/{group}  {direction}.anim";
            clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(fallback);
        }

        if (clip == null)
        {
            throw new InvalidOperationException($"Missing clip: {group} {direction}");
        }

        return clip;
    }

    private static void AddReturnBySpeedTransitions(AnimatorState from, AnimatorState idle, AnimatorState walk, AnimatorState run)
    {
        AddTransition(from, idle, true, Cond(AnimatorConditionMode.Less, 0.1f, Speed));
        AddTransition(from, walk, true,
            Cond(AnimatorConditionMode.Greater, 0.1f, Speed),
            Cond(AnimatorConditionMode.Less, 0.6f, Speed));
        AddTransition(from, run, true, Cond(AnimatorConditionMode.Greater, 0.6f, Speed));
    }

    private static void AddTransition(AnimatorState from, AnimatorState to, bool hasExitTime, params AnimatorCondition[] conditions)
    {
        var t = from.AddTransition(to);
        t.duration = 0f;
        t.hasExitTime = hasExitTime;
        if (hasExitTime)
        {
            t.exitTime = 1f;
        }

        for (var i = 0; i < conditions.Length; i++)
        {
            t.AddCondition(conditions[i].mode, conditions[i].threshold, conditions[i].parameter);
        }
    }

    private static AnimatorCondition Cond(AnimatorConditionMode mode, float threshold, string parameter)
    {
        return new AnimatorCondition
        {
            mode = mode,
            threshold = threshold,
            parameter = parameter
        };
    }

    private static void ResetParameters(AnimatorController controller)
    {
        for (var i = controller.parameters.Length - 1; i >= 0; i--)
        {
            controller.RemoveParameter(controller.parameters[i]);
        }
    }

    private static void EnsureParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        for (var i = 0; i < controller.parameters.Length; i++)
        {
            if (controller.parameters[i].name == name && controller.parameters[i].type == type)
            {
                return;
            }
        }

        controller.AddParameter(name, type);
    }
}
