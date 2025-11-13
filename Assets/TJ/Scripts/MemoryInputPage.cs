using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MemoryInputPage : MonoBehaviour
{
    public enum RestoreMode { Always, IfEmpty, Never }

    [SerializeField] string answerKey = "Question1";
    
    [SerializeField] TMP_InputField answerIF;
    [SerializeField] Button saveNextBtn;
    [SerializeField] GameObject nextPage;
    [SerializeField] GameObject thisPage;

    [SerializeField] int minChars = 1;

    [SerializeField] RestoreMode restore = RestoreMode.Always;

    [SerializeField] bool saveToFile = true;
    [SerializeField] bool restoreFromFileIfEmpty = false;
    [SerializeField] string folderName = "BirthdayReboot";

    void Awake()
    {
        if (!thisPage) thisPage = gameObject;
        LocalJsonDB.EnsureSession();

        if (saveNextBtn)
        {
            saveNextBtn.onClick.RemoveAllListeners();
            saveNextBtn.onClick.AddListener(OnSaveAndNext);
        }
        if (answerIF)
        {
            answerIF.onValueChanged.RemoveAllListeners();
            answerIF.onValueChanged.AddListener(_ => RefreshButton());
        }

        TryRestore();
        RefreshButton();
    }

    void OnEnable()
    {
        TryRestore();
        RefreshButton();
    }

    void TryRestore()
    {
        if (!answerIF || restore == RestoreMode.Never) return;

        string saved = LoadFromDBByKey(answerKey);
        if (string.IsNullOrEmpty(saved) && restoreFromFileIfEmpty)
            saved = LoadFromFile(answerKey);

        if (string.IsNullOrEmpty(saved)) return;

        if (restore == RestoreMode.Always ||
           (restore == RestoreMode.IfEmpty && string.IsNullOrEmpty(answerIF.text)))
        {
            answerIF.text = saved;
        }
    }

    bool IsValid()
    {
        if (!answerIF) return false;
        var s = answerIF.text;
        if (string.IsNullOrWhiteSpace(s)) return false;
        return s.Trim().Length >= minChars;
    }

    void RefreshButton()
    {
        if (saveNextBtn) saveNextBtn.interactable = IsValid();
    }

    void OnSaveAndNext()
    {
        if (!IsValid()) { RefreshButton(); return; }
        string text = answerIF.text.Trim();
        
        LocalJsonDB.SetFreeText(answerKey, text);
        
        if (saveToFile) SaveToFile(answerKey, text);

        if (nextPage)
        {
            nextPage.SetActive(true);
            thisPage.SetActive(false);
        }
    }

    static string LoadFromDBByKey(string key)
    {
        return LocalJsonDB.TryGetFreeText(key, out var v) ? v : "";
    }

    void SaveToFile(string key, string value)
    {
        try
        {
            string dir = Path.Combine(Application.persistentDataPath, folderName);
            Directory.CreateDirectory(dir);

            string sessionId = LocalJsonDB.Current != null && !string.IsNullOrEmpty(LocalJsonDB.Current.sessionId)
                ? LocalJsonDB.Current.sessionId
                : System.DateTime.Now.ToString("yyyyMMddHHmmss");

            string path = Path.Combine(dir, $"{sessionId}_{key}.txt");
            File.WriteAllText(path, value);
#if UNITY_EDITOR
            Debug.Log($"[MemoryInputPage] Saved to file: {path}");
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MemoryInputPage] SaveToFile error: {e.Message}");
        }
    }

    static string LoadFromFile(string key, string folderName = "BirthdayReboot")
    {
        try
        {
            string dir = Path.Combine(Application.persistentDataPath, folderName);
            if (!Directory.Exists(dir)) return "";

            // 세션 매칭 우선
            string sessionId = LocalJsonDB.Current != null ? LocalJsonDB.Current.sessionId : null;
            if (!string.IsNullOrEmpty(sessionId))
            {
                string p = Path.Combine(dir, $"{sessionId}_{key}.txt");
                if (File.Exists(p)) return File.ReadAllText(p);
            }

            var files = Directory.GetFiles(dir, $"*_{key}.txt");
            if (files.Length > 0)
            {
                System.Array.Sort(files);
                return File.ReadAllText(files[files.Length - 1]);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MemoryInputPage] LoadFromFile error: {e.Message}");
        }
        return "";
    }
}
