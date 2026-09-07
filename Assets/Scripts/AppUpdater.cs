using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AppUpdater : MonoBehaviour
{
    const string GitHubOwner = "kingsworksub-jpg";
    const string GitHubRepo = "peisuke-adventure";
    const string ApkAssetName = "PeisukeAdventure.apk";

    public Text statusText;

    string downloadedApkPath;
    bool isBusy;

    [Serializable]
    class GitHubAsset
    {
        public string name;
        public string browser_download_url;
    }

    [Serializable]
    class GitHubRelease
    {
        public string tag_name;
        public GitHubAsset[] assets;
    }

    public void OnUpdateButtonPressed()
    {
        if (isBusy) return;

        if (!string.IsNullOrEmpty(downloadedApkPath) && File.Exists(downloadedApkPath))
        {
            InstallApk(downloadedApkPath);
            return;
        }

        StartCoroutine(CheckAndDownload());
    }

    IEnumerator CheckAndDownload()
    {
        isBusy = true;
        SetStatus("update check...");

        string apiUrl = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
        using (var req = UnityWebRequest.Get(apiUrl))
        {
            req.SetRequestHeader("Accept", "application/vnd.github+json");
            req.SetRequestHeader("User-Agent", "PeisukeAdventure-Updater");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                SetStatus("Update check failed: " + req.error);
                isBusy = false;
                yield break;
            }

            GitHubRelease release;
            try
            {
                release = JsonUtility.FromJson<GitHubRelease>(req.downloadHandler.text);
            }
            catch (Exception e)
            {
                SetStatus("Update info parse error: " + e.Message);
                isBusy = false;
                yield break;
            }

            int latestVersion = ParseVersion(release.tag_name);
            int currentVersion = ParseVersion(Application.version);

            if (latestVersion <= currentVersion)
            {
                SetStatus($"Latest ({Application.version})");
                isBusy = false;
                yield break;
            }

            string apkUrl = null;
            foreach (var asset in release.assets)
            {
                if (asset.name == ApkAssetName)
                {
                    apkUrl = asset.browser_download_url;
                    break;
                }
            }

            if (string.IsNullOrEmpty(apkUrl))
            {
                SetStatus("Update APK not found in release.");
                isBusy = false;
                yield break;
            }

            yield return DownloadApk(apkUrl);
        }

        isBusy = false;
    }

    IEnumerator DownloadApk(string url)
    {
        string dir = Path.Combine(Application.temporaryCachePath, "updates");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, ApkAssetName);

        using (var req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerFile(path);
            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                SetStatus($"downloading... {(int)(req.downloadProgress * 100)}%");
                yield return null;
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                SetStatus("Download failed: " + req.error);
                yield break;
            }
        }

        downloadedApkPath = path;
        SetStatus("Update ready! Tap again to install");
    }

    void InstallApk(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var javaFile = new AndroidJavaObject("java.io.File", path))
            using (var fileProviderClass = new AndroidJavaClass("androidx.core.content.FileProvider"))
            {
                string applicationId = Application.identifier;
                AndroidJavaObject contentUri = fileProviderClass.CallStatic<AndroidJavaObject>(
                    "getUriForFile", activity, applicationId + ".fileprovider", javaFile);

                using (var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW"))
                {
                    intent.Call<AndroidJavaObject>("setDataAndType", contentUri, "application/vnd.android.package-archive");
                    intent.Call<AndroidJavaObject>("addFlags", 1); // FLAG_GRANT_READ_URI_PERMISSION
                    intent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK
                    activity.Call("startActivity", intent);
                }
            }
        }
        catch (Exception e)
        {
            SetStatus("Install failed: " + e.Message);
        }
#else
        SetStatus("Install only supported on Android device.");
#endif
    }

    static int ParseVersion(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        string digits = "";
        foreach (char c in s)
            if (char.IsDigit(c)) digits += c;
        return digits.Length > 0 ? int.Parse(digits) : 0;
    }

    void SetStatus(string msg)
    {
        Debug.Log("[AppUpdater] " + msg);
        if (statusText != null) statusText.text = msg;
    }
}
