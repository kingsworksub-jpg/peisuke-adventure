using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class RunningCharacterBuilder
{
    public static void OpenRunScene()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/RunningCharacterScene.unity");
    }

    [MenuItem("Tools/Create Running Character")]
    public static void Build()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5, 1, 5);

        // Root
        GameObject root = new GameObject("RunningCharacter");
        root.transform.position = new Vector3(0, 0, 0);

        // Torso
        GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        torso.name = "Torso";
        torso.transform.SetParent(root.transform, false);
        torso.transform.localPosition = new Vector3(0, 1.2f, 0);
        torso.transform.localScale = new Vector3(0.44f, 0.3f, 0.44f);

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = new Vector3(0, 1.68f, 0);
        head.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);

        // Joints + limbs
        Transform leftHip = CreateJoint(root.transform, "LeftHipJoint", new Vector3(0.15f, 0.9f, 0));
        Transform rightHip = CreateJoint(root.transform, "RightHipJoint", new Vector3(-0.15f, 0.9f, 0));
        Transform leftShoulder = CreateJoint(root.transform, "LeftShoulderJoint", new Vector3(0.3f, 1.5f, 0));
        Transform rightShoulder = CreateJoint(root.transform, "RightShoulderJoint", new Vector3(-0.3f, 1.5f, 0));

        CreateLimb(leftHip, "LeftUpperLeg", 0.9f, 0.09f);
        CreateLimb(rightHip, "RightUpperLeg", 0.9f, 0.09f);
        CreateLimb(leftShoulder, "LeftArm", 0.6f, 0.07f);
        CreateLimb(rightShoulder, "RightArm", 0.6f, 0.07f);

        // Movement script
        var autoRun = root.AddComponent<AutoRun>();
        autoRun.speed = 3f;

        // Legacy animation component + clip
        Animation anim = root.AddComponent<Animation>();
        AnimationClip clip = BuildRunClip();
        clip.legacy = true;
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/RunAnimation.anim") != null)
        {
            AssetDatabase.DeleteAsset("Assets/RunAnimation.anim");
        }
        AssetDatabase.CreateAsset(clip, "Assets/RunAnimation.anim");
        anim.AddClip(clip, clip.name);
        anim.clip = clip;
        anim.playAutomatically = true;

        // Camera framing
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 2.2f, -5f);
            cam.transform.LookAt(new Vector3(0, 1f, 0));
        }

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/RunningCharacterScene.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Running character created.");
    }

    public static void BuildBattleIdle()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/RunningCharacterScene.unity");

        GameObject root = GameObject.Find("RunningCharacter");
        if (root == null)
        {
            Debug.LogError("RunningCharacter not found in scene. Run Build() first.");
            return;
        }

        AutoRun autoRun = root.GetComponent<AutoRun>();
        if (autoRun != null)
        {
            autoRun.enabled = false;
        }

        Animation anim = root.GetComponent<Animation>();
        if (anim == null)
        {
            anim = root.AddComponent<Animation>();
        }

        AnimationClip clip = BuildBattleIdleClip();
        clip.legacy = true;

        string assetPath = "Assets/BattleIdleAnimation.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }
        AssetDatabase.CreateAsset(clip, assetPath);

        anim.AddClip(clip, clip.name);
        anim.clip = clip;
        anim.playAutomatically = true;

        // Close-up camera framing on the upper body
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 1.35f, -2.6f);
            cam.transform.LookAt(new Vector3(0f, 1.45f, 0f));
            cam.fieldOfView = 40f;
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/RunningCharacterScene.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Battle idle animation created.");
    }

    static AnimationClip BuildBattleIdleClip()
    {
        AnimationClip clip = new AnimationClip();
        clip.name = "BattleIdle";
        clip.wrapMode = WrapMode.Loop;

        float period = 2.6f; // slow calm breathing cycle
        float sampleStep = 0.05f;

        // Base guard-stance pose offsets
        float shoulderBaseX = -75f;  // arms raised forward into a guard
        float hipBaseX = 8f;         // slight forward knee bend
        float hipBaseZ = 12f;        // legs spread for a ready stance

        // Breathing amplitudes
        float torsoScaleAmp = 0.012f;
        float torsoPosAmp = 0.015f;
        float headPosAmp = 0.012f;
        float shoulderAmp = 4f;
        float hipSwayAmp = 2f;
        float rootSwayAmp = 1.5f;

        AnimationCurve torsoScaleY = new AnimationCurve();
        AnimationCurve torsoPosY = new AnimationCurve();
        AnimationCurve headPosY = new AnimationCurve();
        AnimationCurve leftShoulderX = new AnimationCurve();
        AnimationCurve rightShoulderX = new AnimationCurve();
        AnimationCurve leftHipX = new AnimationCurve();
        AnimationCurve rightHipX = new AnimationCurve();
        AnimationCurve rootRotY = new AnimationCurve();

        for (float t = 0; t <= period + 0.0001f; t += sampleStep)
        {
            float phase = (t / period) * Mathf.PI * 2f;
            float breath = Mathf.Sin(phase); // -1..1

            torsoScaleY.AddKey(t, 0.3f + breath * torsoScaleAmp);
            torsoPosY.AddKey(t, 1.2f + breath * torsoPosAmp);
            headPosY.AddKey(t, 1.68f + breath * headPosAmp);

            leftShoulderX.AddKey(t, shoulderBaseX + breath * shoulderAmp);
            rightShoulderX.AddKey(t, shoulderBaseX + Mathf.Sin(phase + 0.4f) * shoulderAmp);

            leftHipX.AddKey(t, hipBaseX + breath * hipSwayAmp);
            rightHipX.AddKey(t, hipBaseX + breath * hipSwayAmp);

            rootRotY.AddKey(t, breath * rootSwayAmp);
        }

        SetLinearTangents(torsoScaleY);
        SetLinearTangents(torsoPosY);
        SetLinearTangents(headPosY);
        SetLinearTangents(leftShoulderX);
        SetLinearTangents(rightShoulderX);
        SetLinearTangents(leftHipX);
        SetLinearTangents(rightHipX);
        SetLinearTangents(rootRotY);

        clip.SetCurve("Torso", typeof(Transform), "localScale.y", torsoScaleY);
        clip.SetCurve("Torso", typeof(Transform), "localPosition.y", torsoPosY);
        clip.SetCurve("Head", typeof(Transform), "localPosition.y", headPosY);
        clip.SetCurve("LeftShoulderJoint", typeof(Transform), "localEulerAngles.x", leftShoulderX);
        clip.SetCurve("RightShoulderJoint", typeof(Transform), "localEulerAngles.x", rightShoulderX);
        clip.SetCurve("LeftHipJoint", typeof(Transform), "localEulerAngles.x", leftHipX);
        clip.SetCurve("RightHipJoint", typeof(Transform), "localEulerAngles.x", rightHipX);
        clip.SetCurve("LeftHipJoint", typeof(Transform), "localEulerAngles.z", ConstantCurve(period, sampleStep, hipBaseZ));
        clip.SetCurve("RightHipJoint", typeof(Transform), "localEulerAngles.z", ConstantCurve(period, sampleStep, -hipBaseZ));
        clip.SetCurve("LeftShoulderJoint", typeof(Transform), "localEulerAngles.z", ConstantCurve(period, sampleStep, -22f));
        clip.SetCurve("RightShoulderJoint", typeof(Transform), "localEulerAngles.z", ConstantCurve(period, sampleStep, 22f));
        clip.SetCurve("", typeof(Transform), "localEulerAngles.y", rootRotY);

        return clip;
    }

    static AnimationCurve ConstantCurve(float period, float sampleStep, float value)
    {
        AnimationCurve curve = new AnimationCurve();
        for (float t = 0; t <= period + 0.0001f; t += sampleStep)
        {
            curve.AddKey(t, value);
        }
        SetLinearTangents(curve);
        return curve;
    }

    public static void CaptureCameraShot()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/RunningCharacterScene.unity");
        GameObject root = GameObject.Find("RunningCharacter");
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/BattleIdleAnimation.anim");
        if (clip != null && root != null)
        {
            clip.SampleAnimation(root, 0.65f);
        }

        Camera cam = Camera.main;
        int w = 960, h = 540;
        RenderTexture rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(w, h, TextureFormat.RGB24, false);
        cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        screenShot.Apply();
        cam.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string outPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "CameraCapture.png");
        System.IO.File.WriteAllBytes(outPath, bytes);
        Debug.Log("Captured camera shot to " + outPath);
    }

    public static void CaptureFilmstrip()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/RunningCharacterScene.unity");
        GameObject root = GameObject.Find("RunningCharacter");
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/BattleIdleAnimation.anim");
        Camera cam = Camera.main;
        if (root == null || clip == null || cam == null)
        {
            Debug.LogError("Missing scene objects for filmstrip capture.");
            return;
        }

        int w = 480, h = 400;
        float[] times = new float[] { 0f, 0.65f, 1.3f, 1.95f };
        string outDir = System.IO.Path.GetDirectoryName(Application.dataPath);

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
            string path = System.IO.Path.Combine(outDir, "Frame_" + i + ".png");
            System.IO.File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(shot);
        }

        Debug.Log("Filmstrip frames captured.");
    }

    static Transform CreateJoint(Transform parent, string name, Vector3 localPos)
    {
        GameObject joint = new GameObject(name);
        joint.transform.SetParent(parent, false);
        joint.transform.localPosition = localPos;
        return joint.transform;
    }

    static void CreateLimb(Transform joint, string name, float length, float radius)
    {
        GameObject limb = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        limb.name = name;
        limb.transform.SetParent(joint, false);
        limb.transform.localPosition = new Vector3(0, -length / 2f, 0);
        limb.transform.localScale = new Vector3(radius * 2f, length / 2f, radius * 2f);
    }

    static AnimationClip BuildRunClip()
    {
        AnimationClip clip = new AnimationClip();
        clip.name = "Run";
        clip.wrapMode = WrapMode.Loop;

        float period = 0.5f; // seconds per full stride cycle
        float sampleStep = 0.025f;
        float legAmplitude = 30f;
        float armAmplitude = 25f;

        AnimationCurve leftHipCurve = new AnimationCurve();
        AnimationCurve rightHipCurve = new AnimationCurve();
        AnimationCurve leftShoulderCurve = new AnimationCurve();
        AnimationCurve rightShoulderCurve = new AnimationCurve();

        for (float t = 0; t <= period + 0.0001f; t += sampleStep)
        {
            float phase = (t / period) * Mathf.PI * 2f;
            float leftLeg = Mathf.Sin(phase) * legAmplitude;
            float rightLeg = -leftLeg;
            float leftArm = -leftLeg * (armAmplitude / legAmplitude);
            float rightArm = -leftArm;

            leftHipCurve.AddKey(t, leftLeg);
            rightHipCurve.AddKey(t, rightLeg);
            leftShoulderCurve.AddKey(t, leftArm);
            rightShoulderCurve.AddKey(t, rightArm);
        }

        SetLinearTangents(leftHipCurve);
        SetLinearTangents(rightHipCurve);
        SetLinearTangents(leftShoulderCurve);
        SetLinearTangents(rightShoulderCurve);

        clip.SetCurve("LeftHipJoint", typeof(Transform), "localEulerAngles.x", leftHipCurve);
        clip.SetCurve("RightHipJoint", typeof(Transform), "localEulerAngles.x", rightHipCurve);
        clip.SetCurve("LeftShoulderJoint", typeof(Transform), "localEulerAngles.x", leftShoulderCurve);
        clip.SetCurve("RightShoulderJoint", typeof(Transform), "localEulerAngles.x", rightShoulderCurve);

        return clip;
    }

    static void SetLinearTangents(AnimationCurve curve)
    {
        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
    }
}
