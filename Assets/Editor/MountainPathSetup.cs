using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

public class MountainPathSetup
{
    [MenuItem("Tools/Setup Mountain Path 1")]
    public static void Setup()
    {
        // 1. Ensure scenes are in Build Settings
        var scenes = EditorBuildSettings.scenes.ToList();
        string hutPath = "Assets/Scenes/MountainHutScene.unity";
        string path1Path = "Assets/Scenes/MountainPath1Scene.unity";

        if (!scenes.Any(s => s.path == hutPath))
        {
            scenes.Add(new EditorBuildSettingsScene(hutPath, true));
        }
        if (!scenes.Any(s => s.path == path1Path))
        {
            scenes.Add(new EditorBuildSettingsScene(path1Path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();

        // 2. Setup MountainHutScene (bottom exit)
        var hutScene = EditorSceneManager.OpenScene(hutPath, OpenSceneMode.Single);
        var roots = hutScene.GetRootGameObjects();
        
        var bottomWall = roots.FirstOrDefault(g => g.name == "Obstacle_Wall_GardenBottom");
        if (bottomWall != null)
        {
            var col = bottomWall.GetComponent<BoxCollider2D>();
            if (col != null) col.isTrigger = true; // Make it pass-through trigger
        }

        var existingExit = roots.FirstOrDefault(g => g.name == "ExitToMountainPath1");
        if (existingExit != null) Object.DestroyImmediate(existingExit);

        var exitGo = new GameObject("ExitToMountainPath1");
        exitGo.transform.position = new Vector3(0.1f, -4.7f, 0f);
        var exitCol = exitGo.AddComponent<BoxCollider2D>();
        exitCol.isTrigger = true;
        exitCol.size = new Vector2(4.5f, 0.8f);
        var trigger = exitGo.AddComponent<SceneTransitionTrigger>();
        trigger.targetSceneName = "MountainPath1Scene";

        EditorSceneManager.MarkSceneDirty(hutScene);
        EditorSceneManager.SaveScene(hutScene);

        // 3. Setup MountainPath1Scene (top exit back to hut)
        var path1Scene = EditorSceneManager.OpenScene(path1Path, OpenSceneMode.Single);
        var pathRoots = path1Scene.GetRootGameObjects();
        
        var bg = pathRoots.FirstOrDefault(g => g.name == "MountainHutBackground" || g.name == "MountainPathBackground");
        if (bg != null)
        {
            bg.name = "MountainPathBackground";
            var sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.45f, 0.7f, 0.4f, 1f); // outdoor green tint
            }
        }

        var pathBottomWall = pathRoots.FirstOrDefault(g => g.name == "Obstacle_Wall_GardenBottom");
        if (pathBottomWall != null)
        {
            var col = pathBottomWall.GetComponent<BoxCollider2D>();
            if (col != null) col.isTrigger = true;
        }

        var hutExit = pathRoots.FirstOrDefault(g => g.name == "ExitToMountainHut");
        if (hutExit != null) Object.DestroyImmediate(hutExit);

        var hutExitGo = new GameObject("ExitToMountainHut");
        hutExitGo.transform.position = new Vector3(0.1f, 4.7f, 0f);
        var hutCol = hutExitGo.AddComponent<BoxCollider2D>();
        hutCol.isTrigger = true;
        hutCol.size = new Vector2(4.5f, 0.8f);
        var hutTrigger = hutExitGo.AddComponent<SceneTransitionTrigger>();
        hutTrigger.targetSceneName = "MountainHutScene";

        EditorSceneManager.MarkSceneDirty(path1Scene);
        EditorSceneManager.SaveScene(path1Scene);

        Debug.Log("MountainPathSetup: Mountain Path 1 triggers and pass-through walls updated successfully!");
    }
}
