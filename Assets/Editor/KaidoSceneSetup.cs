using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;

public class KaidoSceneSetup
{
    [MenuItem("Tools/Setup Kaido Scene")]
    public static void Setup()
    {
        string hutPath = "Assets/Scenes/MountainHutScene.unity";
        string kaidoPath = "Assets/Scenes/KaidoScene.unity";

        // 1. Copy MountainPath1Scene or MountainHutScene to KaidoScene if it doesn't exist
        if (!System.IO.File.Exists(kaidoPath))
        {
            System.IO.File.Copy("Assets/Scenes/MountainPath1Scene.unity", kaidoPath, true);
            AssetDatabase.Refresh();
        }

        // 2. Update Build Settings
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s => s.path == hutPath))
        {
            scenes.Add(new EditorBuildSettingsScene(hutPath, true));
        }
        if (!scenes.Any(s => s.path == kaidoPath))
        {
            scenes.Add(new EditorBuildSettingsScene(kaidoPath, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();

        // 3. Setup MountainHutScene bottom exit to point to KaidoScene
        var hutScene = EditorSceneManager.OpenScene(hutPath, OpenSceneMode.Single);
        var roots = hutScene.GetRootGameObjects();
        var bottomWall = roots.FirstOrDefault(g => g.name == "Obstacle_Wall_GardenBottom");
        if (bottomWall != null)
        {
            var col = bottomWall.GetComponent<BoxCollider2D>();
            if (col != null) col.isTrigger = true;
        }

        var exit = roots.FirstOrDefault(g => g.name == "ExitToMountainPath1" || g.name == "ExitToKaido");
        if (exit != null) Object.DestroyImmediate(exit);

        var exitGo = new GameObject("ExitToKaido");
        exitGo.transform.position = new Vector3(0.1f, -4.7f, 0f);
        var exitCol = exitGo.AddComponent<BoxCollider2D>();
        exitCol.isTrigger = true;
        exitCol.size = new Vector2(4.5f, 0.8f);
        var trigger = exitGo.AddComponent<SceneTransitionTrigger>();
        trigger.targetSceneName = "KaidoScene";

        EditorSceneManager.MarkSceneDirty(hutScene);
        EditorSceneManager.SaveScene(hutScene);

        // 4. Setup KaidoScene
        var kaidoScene = EditorSceneManager.OpenScene(kaidoPath, OpenSceneMode.Single);
        var kaidoRoots = kaidoScene.GetRootGameObjects();

        // Tint background for Kaido (lively road / dirt road brown-orange tint)
        var bg = kaidoRoots.FirstOrDefault(g => g.name == "MountainHutBackground" || g.name == "MountainPathBackground" || g.name == "KaidoBackground");
        if (bg != null)
        {
            bg.name = "KaidoBackground";
            var sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.7f, 0.55f, 0.35f, 1f); // Dirt road / highway brown tint
            }
        }

        var kaidoBottomWall = kaidoRoots.FirstOrDefault(g => g.name == "Obstacle_Wall_GardenBottom");
        if (kaidoBottomWall != null)
        {
            var col = kaidoBottomWall.GetComponent<BoxCollider2D>();
            if (col != null) col.isTrigger = true;
        }

        // Top exit back to Mountain Hut
        var hutExit = kaidoRoots.FirstOrDefault(g => g.name == "ExitToMountainHut");
        if (hutExit != null) Object.DestroyImmediate(hutExit);

        var hutExitGo = new GameObject("ExitToMountainHut");
        hutExitGo.transform.position = new Vector3(0.1f, 4.7f, 0f);
        var hutCol = hutExitGo.AddComponent<BoxCollider2D>();
        hutCol.isTrigger = true;
        hutCol.size = new Vector2(4.5f, 0.8f);
        var hutTrigger = hutExitGo.AddComponent<SceneTransitionTrigger>();
        hutTrigger.targetSceneName = "MountainHutScene";

        EditorSceneManager.MarkSceneDirty(kaidoScene);
        EditorSceneManager.SaveScene(kaidoScene);

        Debug.Log("KaidoSceneSetup: Kaido map setup completed successfully!");
    }
}
