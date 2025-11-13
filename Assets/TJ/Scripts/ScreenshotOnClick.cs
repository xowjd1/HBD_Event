using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;

public class ScreenshotOnEnable : MonoBehaviour
{
    public string subFolder = "ScreenShot";
    public string filePrefix = "screenshot_";
    
    public float delaySeconds = 0.05f;
    public int supersize = 1;

    bool _capturedThisEnable = false;

    void OnEnable()
    {
        if (_capturedThisEnable) return;
        _capturedThisEnable = true;
        StartCoroutine(CaptureRoutine());
    }

    void OnDisable()
    {
        _capturedThisEnable = false;
    }

    IEnumerator CaptureRoutine()
    {
        if (delaySeconds > 0f)
            yield return new WaitForSecondsRealtime(delaySeconds);
        
        yield return new WaitForEndOfFrame();
        
        string buildRoot = Directory.GetParent(Application.dataPath)?.FullName
                           ?? Application.persistentDataPath;
        string dir = Path.Combine(buildRoot, subFolder);
        Directory.CreateDirectory(dir);

        string nick  = GetNicknameForFile(); 
        string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string file  = $"{filePrefix}{nick}_{stamp}.png";
        string path  = Path.Combine(dir, file);

        ScreenCapture.CaptureScreenshot(path, Mathf.Max(1, supersize));
        Debug.Log($"[Screenshot] Saved: {path}");
    }

    string GetNicknameForFile()
    {
        string raw = "user";
        try
        {
            LocalJsonDB.EnsureSession();
            var r = LocalJsonDB.Current;
            if (r != null && !string.IsNullOrWhiteSpace(r.nickname))
                raw = r.nickname.Trim();
        }
        catch { /* 세션 없으면 기본값 사용 */ }

        string safe = Regex.Replace(raw, @"[\\/:*?""<>|]+", "_");
        safe = Regex.Replace(safe, @"\s+", "_").Trim('_');

        if (string.IsNullOrEmpty(safe)) safe = "user";
        if (safe.Length > 40) safe = safe.Substring(0, 40);

        return safe;
    }
}
