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

        // 1. Copy MountainHutScene to KaidoScene if it doesn't exist
        if (!System.IO.File.Exists(kaidoPath))
        {
            System.IO.File.Copy("Assets/Scenes/MountainHutScene.unity", kaidoPath, true);
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
        var bottomWall = roots.FirstOrDefault(g => g != null && g.name == "Obstacle_Wall_GardenBottom");
        if (bottomWall != null)
        {
            var col = bottomWall.GetComponent<BoxCollider2D>();
            if (col != null) col.isTrigger = true;
        }

        var exit = roots.FirstOrDefault(g => g != null && (g.name == "ExitToMountainPath1" || g.name == "ExitToKaido"));
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

        // 4. Setup KaidoScene (Outdoor highway / 街道)
        var kaidoScene = EditorSceneManager.OpenScene(kaidoPath, OpenSceneMode.Single);

        // Tint background for Kaido (outdoor dirt road brown-orange tint)
        var bg = kaidoScene.GetRootGameObjects().FirstOrDefault(g => g != null && (g.name == "MountainHutBackground" || g.name == "MountainPathBackground" || g.name == "KaidoBackground"));
        if (bg != null)
        {
            bg.name = "KaidoBackground";
            var sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.75f, 0.6f, 0.4f, 1f); // Outdoor dirt road tint
            }
        }

        // Remove indoor furniture from KaidoScene so it's a distinct outdoor road map
        string[] indoorObjectsToDestroy = new string[] {
            "Obstacle_Chair", "Obstacle_Fireplace", "Obstacle_Nightstand", "Obstacle_Bed", "CampfireSmall"
        };
        foreach (var objName in indoorObjectsToDestroy)
        {
            var obj = kaidoScene.GetRootGameObjects().FirstOrDefault(g => g != null && g.name == objName);
            if (obj != null) Object.DestroyImmediate(obj);
        }

        // Top exit back to Mountain Hut
        var hutExit = kaidoScene.GetRootGameObjects().FirstOrDefault(g => g != null && g.name == "ExitToMountainHut");
        if (hutExit != null) Object.DestroyImmediate(hutExit);

        var hutExitGo = new GameObject("ExitToMountainHut");
        hutExitGo.transform.position = new Vector3(0.1f, 4.7f, 0f);
        var hutCol = hutExitGo.AddComponent<BoxCollider2D>();
        hutCol.isTrigger = true;
        hutCol.size = new Vector2(4.5f, 0.8f);
        var hutTrigger = hutExitGo.AddComponent<SceneTransitionTrigger>();
        hutTrigger.targetSceneName = "MountainHutScene";

        // Add sign / InteractPrompt for Kaido
        var sign = kaidoScene.GetRootGameObjects().FirstOrDefault(g => g != null && g.name == "Sign_Kaido");
        if (sign != null) Object.DestroyImmediate(sign);

        var signGo = new GameObject("Sign_Kaido");
        signGo.transform.position = new Vector3(0f, 0f, 0f);
        var signPrompt = signGo.AddComponent<InteractPrompt>();
        var player = kaidoScene.GetRootGameObjects().FirstOrDefault(g => g != null && g.GetComponent<TopDownWalker>() != null);
        if (player != null)
        {
            signPrompt.player = player.transform;
        }
        signPrompt.detectionCenter = signGo.transform;
        signPrompt.radius = 1.2f;
        signPrompt.speechMessage = "街道：遠くまで続く広々とした街道だ！";

        EditorSceneManager.MarkSceneDirty(kaidoScene);
        EditorSceneManager.SaveScene(kaidoScene);

        Debug.Log("KaidoSceneSetup: Kaido map setup completed successfully!");
    }
}
