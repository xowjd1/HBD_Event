using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public class LocalJsonDB
{
    [Serializable]
    class Database
    {
        public List<Record> records = new List<Record>();
    }
    [Serializable]
    public class Record
    {
        public string sessionId;
        public string createdAtIso;
        public string updatedAtIso;

        public string nickname;
        public string birthYYMMDD;
        public int    identityPercent;
        public string identityLabel;
        
        public List<string> checklistKeys   = new();
        public List<string> checklistValues = new();

        [NonSerialized] public Dictionary<string, string> freeTextAnswers;
        public List<string> freeTextKeys   = new();
        public List<string> freeTextValues = new();

        public void SyncDictFromLists()
        {
            if (freeTextAnswers == null) freeTextAnswers = new Dictionary<string, string>();
            else freeTextAnswers.Clear();

            for (int i = 0; i < freeTextKeys.Count; i++)
            {
                var k = freeTextKeys[i];
                var v = (i < freeTextValues.Count) ? freeTextValues[i] : "";
                if (!string.IsNullOrEmpty(k)) freeTextAnswers[k] = v ?? "";
            }
        }
        
        public void SyncListsFromDict()
        {
            freeTextKeys.Clear();
            freeTextValues.Clear();
            if (freeTextAnswers == null) return;

            foreach (var kv in freeTextAnswers)
            {
                freeTextKeys.Add(kv.Key);
                freeTextValues.Add(kv.Value ?? "");
            }
        }
    }
    
    static Database _db;
    static Record   _current;

    public static Record Current => _current;

    static readonly string FileName = "birthday_reboot_db.json";
    static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static void EnsureSession()
    {
        if (_db == null) Load();
        if (_current == null) StartNewSession();
    }

    public static void StartNewSession()
    {
        if (_db == null) _db = new Database();

        _current = new Record
        {
            sessionId    = Guid.NewGuid().ToString("N"),
            createdAtIso = DateTime.UtcNow.ToString("o"),
            updatedAtIso = DateTime.UtcNow.ToString("o"),
            nickname     = "",
            birthYYMMDD  = "",
            identityLabel= "",
            identityPercent = 0
        };
        _current.SyncDictFromLists();
        _db.records.Add(_current);
        Save();
    }

    public static void UpdateCurrent(Action<Record> updater)
    {
        EnsureSession();
        updater?.Invoke(_current);
        _current.updatedAtIso = DateTime.UtcNow.ToString("o");
        Save();
    }

    public static void Save()
    {
        if (_db == null) _db = new Database();
        foreach (var r in _db.records) r.SyncListsFromDict();

        var json = JsonUtility.ToJson(_db, prettyPrint: true);
        try
        {
            File.WriteAllText(FilePath, json, Encoding.UTF8);
#if UNITY_EDITOR
            Debug.Log($"[LocalJsonDB] Saved: {FilePath}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LocalJsonDB] Save error: {e.Message}");
        }
    }

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                _db = new Database();
                _current = null;
                return;
            }
            var json = File.ReadAllText(FilePath, Encoding.UTF8);
            _db = JsonUtility.FromJson<Database>(json) ?? new Database();

            foreach (var r in _db.records) r.SyncDictFromLists();
            _current = _db.records.Count > 0 ? _db.records[_db.records.Count - 1] : null;
#if UNITY_EDITOR
            Debug.Log($"[LocalJsonDB] Loaded: {FilePath}, records={_db.records.Count}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[LocalJsonDB] Load error: {e.Message}");
            _db = new Database();
            _current = null;
        }
    }

    public static void SetBasicInfo(string nickname, string birthYmd)
    {
        UpdateCurrent(r =>
        {
            r.nickname    = nickname ?? "";
            r.birthYYMMDD = birthYmd ?? "";
        });
    }

    public static void SetIdentityLabel(string label)
    {
        UpdateCurrent(r => r.identityLabel = label ?? "");
    }

    public static void SetIdentityPercent(int percent)
    {
        UpdateCurrent(r => r.identityPercent = Mathf.Clamp(percent, 0, 100));
    }

    public static void SetChecklist(string key, int value)
    {
        UpdateCurrent(r =>
        {
            value = Mathf.Clamp(value, 0, 4);
            int idx = r.checklistKeys.IndexOf(key);
            if (idx < 0)
            {
                r.checklistKeys.Add(key);
                r.checklistValues.Add(value.ToString());
            }
            else
            {
                while (r.checklistValues.Count <= idx) r.checklistValues.Add("0");
                r.checklistValues[idx] = value.ToString();
            }
        });
    }

    public static bool TryGetChecklistInt(string key, out int value)
    {
        EnsureSession();
        value = 0;
        var r = _current;
        if (r == null) return false;

        int idx = r.checklistKeys.IndexOf(key);
        if (idx < 0 || idx >= r.checklistValues.Count) return false;
        if (!int.TryParse(r.checklistValues[idx], out var v)) return false;

        value = Mathf.Clamp(v, 0, 4);
        return true;
    }

    public static void SetFreeText(string key, string text)
    {
        UpdateCurrent(r =>
        {
            if (r.freeTextAnswers == null) r.freeTextAnswers = new Dictionary<string, string>();
            r.freeTextAnswers[key] = text ?? "";
        });
    }

    public static bool TryGetFreeText(string key, out string text)
    {
        EnsureSession();
        text = "";
        var r = _current;
        if (r == null || r.freeTextAnswers == null) return false;
        return r.freeTextAnswers.TryGetValue(key, out text);
    }

    public static string GetFreeText(string key)
    {
        return TryGetFreeText(key, out var t) ? (t ?? "") : "";
    }

    public static Record GetLastFilledRecord()
    {
        EnsureSession();
        if (_db == null || _db.records == null || _db.records.Count == 0) return _current;

        for (int i = _db.records.Count - 1; i >= 0; i--)
        {
            var r = _db.records[i];
            bool hasSomething =
                (r != null) &&
                (!string.IsNullOrWhiteSpace(r.nickname) ||
                 !string.IsNullOrWhiteSpace(r.birthYYMMDD) ||
                 (r.checklistValues != null && r.checklistValues.Count > 0) ||
                 (r.freeTextKeys != null && r.freeTextKeys.Count > 0));
            if (hasSomething) return r;
        }
        return _current;
    }
    
    public static string GetFreeTextFromRecord(Record r, string key)
    {
        if (r == null || string.IsNullOrEmpty(key)) return "";
        if (r.freeTextAnswers != null && r.freeTextAnswers.TryGetValue(key, out var v))
            return v ?? "";

        for (int i = 0; i < r.freeTextKeys.Count; i++)
            if (r.freeTextKeys[i] == key)
                return (i < r.freeTextValues.Count) ? (r.freeTextValues[i] ?? "") : "";
        return "";
    }

    
    
}
