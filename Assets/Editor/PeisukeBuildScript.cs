using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PeisukeBuildScript
{
    const string ScenePath = "Assets/Scenes/MountainHutScene.unity";
    const string CampfireSpritePath = "Assets/CharacterRef/Generated/Campfire_Small.png";

    public static void SetupCampfireAndBuild()
    {
        AssetDatabase.ImportAsset(CampfireSpritePath, ImportAssetOptions.ForceUpdate);
        var importer = (TextureImporter)AssetImporter.GetAtPath(CampfireSpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(CampfireSpritePath);
        if (sprite == null)
        {
            Debug.LogError("PeisukeBuildScript: campfire sprite failed to load at " + CampfireSpritePath);
        }

        var roots = scene.GetRootGameObjects();
        var existing = roots.FirstOrDefault(g => g.name == "CampfireSmall");
        if (existing != null) Object.DestroyImmediate(existing);

        var go = new GameObject("CampfireSmall");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        const float targetWidth = 0.32f;
        const float baseWidth = 1024f / 100f;
        float scale = targetWidth / baseWidth;
        go.transform.localScale = new Vector3(scale, scale, 1f);
        go.transform.position = new Vector3(0f, -4.167f, 0f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("PeisukeBuildScript: CampfireSmall placed at " + go.transform.position + " scale=" + scale);

        BumpVersionAndBuild();
    }

    public static void BumpVersionAndBuild()
    {
        int newVersionCode = PlayerSettings.Android.bundleVersionCode + 1;
        PlayerSettings.Android.bundleVersionCode = newVersionCode;
        PlayerSettings.bundleVersion = newVersionCode.ToString();

        var buildOptions = new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/Android/PeisukeAdventure.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);
        Debug.Log("PeisukeBuildScript: Build result=" + report.summary.result + " totalErrors=" + report.summary.totalErrors + " version=" + newVersionCode);
    }
}
