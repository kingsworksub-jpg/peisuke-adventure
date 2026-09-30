using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

public class KaidoSingleSceneSetup
{
    [MenuItem("Tools/Setup Single Scene Kaido")]
    public static void Setup()
    {
        string hutPath = "Assets/Scenes/MountainHutScene.unity";
        var scene = EditorSceneManager.OpenScene(hutPath, OpenSceneMode.Single);
        var roots = scene.GetRootGameObjects();

        // 1. Ensure bottom wall of Hut is pass-through trigger
        var bottomWall = roots.FirstOrDefault(g => g != null && g.name == "Obstacle_Wall_GardenBottom");
        if (bottomWall != null)
        {
            var col = bottomWall.GetComponent<BoxCollider2D>();
            if (col != null) col.isTrigger = true;
        }

        // Remove old transition objects
        var oldTrans = roots.FirstOrDefault(g => g != null && (g.name == "AreaTransitionManager" || g.name == "KaidoAreaRoot" || g.name == "ExitToKaido" || g.name == "ExitToMountainHut"));
        while (oldTrans != null)
        {
            Object.DestroyImmediate(oldTrans);
            oldTrans = scene.GetRootGameObjects().FirstOrDefault(g => g != null && (g.name == "AreaTransitionManager" || g.name == "KaidoAreaRoot" || g.name == "ExitToKaido" || g.name == "ExitToMountainHut"));
        }

        // 2. Create Kaido Area Root at y = -13.5
        var kaidoRoot = new GameObject("KaidoAreaRoot");
        kaidoRoot.transform.position = new Vector3(0f, -13.5f, 0f);

        // Kaido background sprite (duplicate or clone MountainHutBackground style)
        var hutBg = roots.FirstOrDefault(g => g != null && g.name == "MountainHutBackground");
        GameObject kaidoBg = null;
        if (hutBg != null)
        {
            kaidoBg = Object.Instantiate(hutBg, kaidoRoot.transform);
            kaidoBg.name = "KaidoBackground";
            kaidoBg.transform.localPosition = Vector3.zero;
            var sr = kaidoBg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(0.75f, 0.6f, 0.4f, 1f); // Dirt road brown tint
            }
        }

        // Kaido sign / InteractPrompt
        var signGo = new GameObject("Sign_Kaido");
        signGo.transform.SetParent(kaidoRoot.transform, false);
        signGo.transform.localPosition = new Vector3(0f, 1f, 0f);
        var signPrompt = signGo.AddComponent<InteractPrompt>();
        var player = roots.FirstOrDefault(g => g != null && g.GetComponent<TopDownWalker>() != null);
        if (player != null)
        {
            signPrompt.player = player.transform;
        }
        signPrompt.detectionCenter = signGo.transform;
        signPrompt.radius = 1.2f;
        signPrompt.speechMessage = "街道：遠くまで続く広々とした街道だ！";

        // 3. Create Area Transition Manager
        var managerGo = new GameObject("AreaTransitionManager");
        var trans = managerGo.AddComponent<AreaTransition>();
        if (player != null) trans.player = player.transform;
        trans.mainCam = Camera.main;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("KaidoSingleSceneSetup: Single-scene Kaido area set up successfully at y = -13.5!");
    }
}
