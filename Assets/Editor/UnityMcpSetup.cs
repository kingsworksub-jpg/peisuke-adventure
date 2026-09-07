using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

/// <summary>
/// Unity公式MCPサーバー（com.unity.ai.assistantパッケージ）をバッチモードから
/// インストールするためのエディタスクリプト。Package Manager UIのクリック操作なしで
/// パッケージ追加できるようにする（このマシンではUnityへの合成マウス入力が効かないため）。
/// </summary>
public static class UnityMcpSetup
{
    private const string PackageId = "com.unity.ai.assistant@2.18.0-pre.2";
    private static AddRequest _request;

    public static void InstallAiAssistantPackage()
    {
        Debug.Log($"[UnityMcpSetup] Requesting install: {PackageId}");
        _request = Client.Add(PackageId);
        EditorApplication.update += Progress;
    }

    private static void Progress()
    {
        if (_request == null || !_request.IsCompleted) return;
        EditorApplication.update -= Progress;

        if (_request.Status == StatusCode.Success)
        {
            Debug.Log($"[UnityMcpSetup] SUCCESS: installed {_request.Result.packageId}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[UnityMcpSetup] FAILED: {_request.Error?.message}");
            EditorApplication.Exit(1);
        }
    }
}
