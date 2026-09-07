using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class TestCharacter2DBuilder
{
    const string PartsDir = "Assets/Sprites2D/parts";

    public static void OpenScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TestCharacter2DScene.unity");
    }

    public static void Build()
    {
        // Ensure sprite import settings for every part.
        foreach (var name in PartNames)
        {
            string path = $"{PartsDir}/{name}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            var settings = importer.GetDefaultPlatformTextureSettings();
            settings.maxTextureSize = 1024;
            // 圧縮フォーマットだとアルファの境界にブロックノイズが出て、回転時に
            // 白い破片のようなゴーストが見えることがあるため非圧縮にする。
            settings.format = TextureImporterFormat.RGBA32;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.overridden = true;
            importer.SetPlatformTextureSettings(settings);
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
        AssetDatabase.Refresh();

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject root = new GameObject("TestCharacter2D");

        GameObject hip = NewChild(root.transform, "Hip", Vector2.zero);

        // Torso (directly under Hip)
        GameObject torso = AddSprite(hip.transform, "Torso", "torso", new Vector2(0f, 1.0f), 5);

        // Head
        GameObject neckJoint = NewChild(hip.transform, "NeckJoint", new Vector2(0f, 2.0f));
        AddSprite(neckJoint.transform, "Head", "head", new Vector2(0f, 1.25f), 12);

        // Left arm (shoulder -> elbow -> wrist, each a separate anatomical part)
        // 描画順は全パーツで重複しないように1つずつ増やす(同じsortingOrderの重なりが
        // 境界のちらつき/ノイズの原因になっていたため)。
        GameObject leftShoulder = NewChild(hip.transform, "LeftShoulderJoint", new Vector2(-0.8f, 1.7f));
        AddSprite(leftShoulder.transform, "LeftUpperArm", "left_upper_arm", new Vector2(-0.3f, -0.55f), 6);
        GameObject leftElbow = NewChild(leftShoulder.transform, "LeftElbowJoint", new Vector2(-0.6f, -1.05f));
        AddSprite(leftElbow.transform, "LeftForearm", "left_forearm", new Vector2(0.075f, -0.4f), 7);
        GameObject leftWrist = NewChild(leftElbow.transform, "LeftWristJoint", new Vector2(-0.05f, -0.73f));
        AddSprite(leftWrist.transform, "LeftHand", "left_hand", new Vector2(-0.05f, -0.575f), 8);

        // Right arm
        GameObject rightShoulder = NewChild(hip.transform, "RightShoulderJoint", new Vector2(0.8f, 1.7f));
        AddSprite(rightShoulder.transform, "RightUpperArm", "right_upper_arm", new Vector2(0.4f, -0.55f), 9);
        GameObject rightElbow = NewChild(rightShoulder.transform, "RightElbowJoint", new Vector2(0.6f, -1.05f));
        AddSprite(rightElbow.transform, "RightForearm", "right_forearm", new Vector2(-0.1f, -0.45f), 10);
        GameObject rightWrist = NewChild(rightElbow.transform, "RightWristJoint", new Vector2(-0.58f, -0.85f));
        AddSprite(rightWrist.transform, "RightHand", "right_hand", new Vector2(0.805f, -0.475f), 11);

        // Left leg (hip -> knee -> ankle)
        GameObject leftHipJoint = NewChild(hip.transform, "LeftHipJoint", new Vector2(-0.35f, 0.1f));
        AddSprite(leftHipJoint.transform, "LeftThigh", "left_thigh", new Vector2(0.075f, -0.675f), 0);
        GameObject leftKnee = NewChild(leftHipJoint.transform, "LeftKneeJoint", new Vector2(0f, -1.5f));
        AddSprite(leftKnee.transform, "LeftShin", "left_shin", new Vector2(0.075f, -0.375f), 1);
        GameObject leftAnkle = NewChild(leftKnee.transform, "LeftAnkleJoint", new Vector2(0f, -0.8f));
        AddSprite(leftAnkle.transform, "LeftFoot", "left_foot", new Vector2(0.025f, -0.375f), 2);

        // Right leg
        GameObject rightHipJoint = NewChild(hip.transform, "RightHipJoint", new Vector2(0.35f, 0.1f));
        AddSprite(rightHipJoint.transform, "RightThigh", "right_thigh", new Vector2(0.4f, -0.675f), 0);
        GameObject rightKnee = NewChild(rightHipJoint.transform, "RightKneeJoint", new Vector2(0f, -1.5f));
        AddSprite(rightKnee.transform, "RightShin", "right_shin", new Vector2(0.4f, -0.375f), 3);
        GameObject rightAnkle = NewChild(rightKnee.transform, "RightAnkleJoint", new Vector2(0.4f, -0.8f));
        AddSprite(rightAnkle.transform, "RightFoot", "right_foot", new Vector2(0f, -0.375f), 4);

        // Ground plane for reference
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Quad);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, -1.5f, 0.5f);
        ground.transform.localScale = new Vector3(6, 3, 1);

        root.transform.position = new Vector3(0, 0, 0);

        // Camera framing
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0, 0f, -10);
            cam.transform.rotation = Quaternion.identity;
        }

        // Animation + AutoRun-free idle
        Animation anim = root.AddComponent<Animation>();
        AnimationClip clip = BuildBreathClip();
        clip.legacy = true;
        string clipPath = "Assets/Sprites2D/TestCharacter2D_Idle.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);
        anim.AddClip(clip, clip.name);
        anim.clip = clip;
        anim.playAutomatically = true;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/TestCharacter2DScene.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("2D bone-rig test character built.");
    }

    static readonly string[] PartNames = new[]
    {
        "head", "torso", "left_upper_arm", "left_forearm", "left_hand", "right_upper_arm", "right_forearm", "right_hand",
        "left_thigh", "left_shin", "left_foot", "right_thigh", "right_shin", "right_foot"
    };

    static GameObject NewChild(Transform parent, string name, Vector2 localPos)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        return go;
    }

    static GameObject AddSprite(Transform parent, string name, string spriteName, Vector2 localPos, int order)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        var sr = go.AddComponent<SpriteRenderer>();
        string path = $"{PartsDir}/{spriteName}.png";
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        sr.sortingOrder = order;
        return go;
    }

    static AnimationClip BuildBreathClip()
    {
        AnimationClip clip = new AnimationClip();
        clip.name = "Idle_Breath";
        clip.wrapMode = WrapMode.Loop;

        float period = 2.6f;
        float step = 0.05f;

        AnimationCurve torsoScaleX = new AnimationCurve();
        AnimationCurve torsoScaleY = new AnimationCurve();
        AnimationCurve neckRot = new AnimationCurve();
        AnimationCurve leftShoulderRot = new AnimationCurve();
        AnimationCurve rightShoulderRot = new AnimationCurve();
        AnimationCurve leftElbowRot = new AnimationCurve();
        AnimationCurve rightElbowRot = new AnimationCurve();
        AnimationCurve leftHipRot = new AnimationCurve();
        AnimationCurve rightHipRot = new AnimationCurve();
        AnimationCurve leftKneeRot = new AnimationCurve();
        AnimationCurve rightKneeRot = new AnimationCurve();

        // 戦闘待機の構えポーズ基本角度(度)。呼吸の揺れはこの角度を基準に上乗せする。
        const float shoulderBase = 15f;   // 腕をやや前方・内側に
        const float elbowBase = 40f;      // 肘を軽く曲げる(交差しない程度)
        const float hipBase = 10f;        // 腰を少し開いて安定した構え足に
        const float kneeBase = -16f;      // 膝を軽く曲げて重心を落とす

        for (float t = 0; t <= period + 0.0001f; t += step)
        {
            float phase = (t / period) * Mathf.PI * 2f;
            float breath = Mathf.Sin(phase);

            torsoScaleX.AddKey(t, 1f + breath * 0.015f);
            torsoScaleY.AddKey(t, 1f + breath * 0.02f);
            neckRot.AddKey(t, breath * 2f);
            leftShoulderRot.AddKey(t, shoulderBase + breath * 4f);
            rightShoulderRot.AddKey(t, -shoulderBase - breath * 4f);
            leftElbowRot.AddKey(t, elbowBase + breath * 5f);
            rightElbowRot.AddKey(t, -elbowBase - breath * 5f);
            leftHipRot.AddKey(t, hipBase + breath * 1.5f);
            rightHipRot.AddKey(t, -hipBase - breath * 1.5f);
            leftKneeRot.AddKey(t, kneeBase - breath * 2f);
            rightKneeRot.AddKey(t, -kneeBase + breath * 2f);
        }

        SetLinear(torsoScaleX); SetLinear(torsoScaleY); SetLinear(neckRot);
        SetLinear(leftShoulderRot); SetLinear(rightShoulderRot);
        SetLinear(leftElbowRot); SetLinear(rightElbowRot);
        SetLinear(leftHipRot); SetLinear(rightHipRot);
        SetLinear(leftKneeRot); SetLinear(rightKneeRot);

        clip.SetCurve("Hip/Torso", typeof(Transform), "localScale.x", torsoScaleX);
        clip.SetCurve("Hip/Torso", typeof(Transform), "localScale.y", torsoScaleY);
        clip.SetCurve("Hip/NeckJoint", typeof(Transform), "localEulerAngles.z", neckRot);
        clip.SetCurve("Hip/LeftShoulderJoint", typeof(Transform), "localEulerAngles.z", leftShoulderRot);
        clip.SetCurve("Hip/RightShoulderJoint", typeof(Transform), "localEulerAngles.z", rightShoulderRot);
        clip.SetCurve("Hip/LeftShoulderJoint/LeftElbowJoint", typeof(Transform), "localEulerAngles.z", leftElbowRot);
        clip.SetCurve("Hip/RightShoulderJoint/RightElbowJoint", typeof(Transform), "localEulerAngles.z", rightElbowRot);
        clip.SetCurve("Hip/LeftHipJoint", typeof(Transform), "localEulerAngles.z", leftHipRot);
        clip.SetCurve("Hip/RightHipJoint", typeof(Transform), "localEulerAngles.z", rightHipRot);
        clip.SetCurve("Hip/LeftHipJoint/LeftKneeJoint", typeof(Transform), "localEulerAngles.z", leftKneeRot);
        clip.SetCurve("Hip/RightHipJoint/RightKneeJoint", typeof(Transform), "localEulerAngles.z", rightKneeRot);

        return clip;
    }

    static void SetLinear(AnimationCurve curve)
    {
        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
    }

    public static void CaptureFilmstrip()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TestCharacter2DScene.unity");
        GameObject root = GameObject.Find("TestCharacter2D");
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Sprites2D/TestCharacter2D_Idle.anim");
        Camera cam = Camera.main;
        if (root == null || clip == null || cam == null)
        {
            Debug.LogError("Missing objects for filmstrip capture.");
            return;
        }

        int w = 350, h = 900;
        float[] times = new float[] { 0f, 0.65f, 1.3f, 1.95f };
        string outDir = Path.GetDirectoryName(Application.dataPath);

        for (int i = 0; i < times.Length; i++)
        {
            clip.SampleAnimation(root, times[i]);

            RenderTexture rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            Texture2D shot = new Texture2D(w, h, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = rt;
            shot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            shot.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] bytes = shot.EncodeToPNG();
            string path = Path.Combine(outDir, "Char2D_Frame_" + i + ".png");
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(shot);
        }
        Debug.Log("Filmstrip frames captured.");
    }

    public static void ExportDetailFrames()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TestCharacter2DScene.unity");
        GameObject root = GameObject.Find("TestCharacter2D");
        GameObject ground = GameObject.Find("Ground");
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Sprites2D/TestCharacter2D_Idle.anim");
        Camera cam = Camera.main;
        if (root == null || clip == null || cam == null)
        {
            Debug.LogError("Missing objects for frame export.");
            return;
        }
        if (ground != null) ground.SetActive(false);

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.orthographic = true;
        cam.orthographicSize = 4.6f;
        cam.transform.position = new Vector3(0f, 0.5f, -10f);

        int w = 800, h = 600;
        int frameCount = 12;
        float clipPeriod = 2.6f; // BuildBreathClipで定義した実際のクリップ周期(変更しないこと)
        int cyclesPerLoop = 2;   // 12フレームの中に呼吸を何サイクル収めるか(体感速度の倍率)

        string outDir = "C:/Users/norio/Documents/mostone/assets/characters";
        Directory.CreateDirectory(outDir);

        for (int i = 0; i < frameCount; i++)
        {
            float t = (cyclesPerLoop * clipPeriod / frameCount) * i;
            clip.SampleAnimation(root, t);

            RenderTexture rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            Texture2D shot = new Texture2D(w, h, TextureFormat.RGBA32, false);
            cam.Render();
            RenderTexture.active = rt;
            shot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            shot.Apply();
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);

            byte[] bytes = shot.EncodeToPNG();
            string path = Path.Combine(outDir, "rin_detail_frame_" + i.ToString("00") + ".png");
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(shot);
        }

        if (ground != null) ground.SetActive(true);
        Debug.Log("Exported " + frameCount + " detail frames to " + outDir);
    }

    public static void DiagnoseBackground()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TestCharacter2DScene.unity");
        GameObject root = GameObject.Find("TestCharacter2D");
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Sprites2D/TestCharacter2D_Idle.anim");
        Camera cam = Camera.main;

        Debug.Log("Root found: " + (root != null) + ", Clip found: " + (clip != null) + ", Cam found: " + (cam != null));
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            Debug.Log("Child: " + t.name + " active=" + t.gameObject.activeSelf + " activeInHierarchy=" + t.gameObject.activeInHierarchy);
        }
        GameObject ground = GameObject.Find("Ground");
        Debug.Log("Ground found: " + (ground != null) + (ground != null ? (" active=" + ground.activeSelf) : ""));

        clip.SampleAnimation(root, 0.65f);

        Transform leftElbowT = root.transform.Find("Hip/LeftShoulderJoint/LeftElbowJoint");
        Transform leftKneeT = root.transform.Find("Hip/LeftHipJoint/LeftKneeJoint");
        Debug.Log("LeftElbowJoint found=" + (leftElbowT != null) + " localEulerAngles.z=" + (leftElbowT != null ? leftElbowT.localEulerAngles.z.ToString() : "N/A"));
        Debug.Log("LeftKneeJoint found=" + (leftKneeT != null) + " localEulerAngles.z=" + (leftKneeT != null ? leftKneeT.localEulerAngles.z.ToString() : "N/A"));
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(1f, 0f, 1f, 1f); // magenta, fully opaque, to visually reveal any white opaque rects
        cam.orthographic = true;
        cam.orthographicSize = 3.8f;
        cam.transform.position = new Vector3(0f, 0.5f, -10f);
        if (ground != null) ground.SetActive(false);

        int w = 800, h = 600;
        RenderTexture rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
        Texture2D shot = new Texture2D(w, h, TextureFormat.RGBA32, false);
        cam.Render();
        RenderTexture.active = rt;
        shot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        shot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = shot.EncodeToPNG();
        string outPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "DiagnoseBackground.png");
        File.WriteAllBytes(outPath, bytes);
        Debug.Log("Diagnostic capture saved to " + outPath);
    }

    public static void CaptureShot()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/TestCharacter2DScene.unity");
        GameObject root = GameObject.Find("TestCharacter2D");
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Sprites2D/TestCharacter2D_Idle.anim");
        Camera cam = Camera.main;
        if (root == null || clip == null || cam == null)
        {
            Debug.LogError("Missing objects for capture.");
            return;
        }
        clip.SampleAnimation(root, 0.65f);

        int w = 700, h = 900;
        RenderTexture rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        Texture2D shot = new Texture2D(w, h, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        shot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        shot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = shot.EncodeToPNG();
        string outPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Character2DCapture.png");
        File.WriteAllBytes(outPath, bytes);
        Debug.Log("Captured 2D character shot to " + outPath);
    }
}
