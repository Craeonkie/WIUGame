using UnityEngine;
using UnityEditor;

public class RemoveAnimationEvents : EditorWindow
{
    [MenuItem("Tools/Remove Animation Events")]
    public static void ShowWindow()
    {
        GetWindow<RemoveAnimationEvents>("Remove Anim Events");
    }

    private void OnGUI()
    {
        GUILayout.Label("Select animation clips in Project window", EditorStyles.boldLabel);

        if (GUILayout.Button("Remove Events from Selected Clips"))
        {
            RemoveEventsFromSelection();
        }

        if (GUILayout.Button("Remove Events from All Clips in Folder"))
        {
            RemoveEventsFromFolder();
        }
    }

    private void RemoveEventsFromSelection()
    {
        int removedCount = 0;

        foreach (var guid in Selection.assetGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip != null)
            {
                var events = AnimationUtility.GetAnimationEvents(clip);
                if (events.Length > 0)
                {
                    AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
                    removedCount += events.Length;
                    Debug.Log($"Removed {events.Length} events from: {clip.name}");
                }
            }
        }

        Debug.Log($"Done! Removed {removedCount} total animation events.");
    }

    private void RemoveEventsFromFolder()
    {
        string folderPath = EditorUtility.OpenFolderPanel("Select Animation Folder", "Assets", "");
        if (string.IsNullOrEmpty(folderPath)) return;

        // Convert to relative path
        if (!folderPath.StartsWith(Application.dataPath))
        {
            Debug.LogError("Folder must be inside Assets folder!");
            return;
        }
        folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);

        int removedCount = 0;
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folderPath });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip != null)
            {
                var events = AnimationUtility.GetAnimationEvents(clip);
                if (events.Length > 0)
                {
                    AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
                    removedCount += events.Length;
                }
            }
        }

        Debug.Log($"Done! Removed {removedCount} total animation events from folder.");
    }
}