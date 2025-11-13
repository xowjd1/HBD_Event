using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class CsvAtUserData : MonoBehaviour
{
    public string folderName = "UserData";
    public string fileName   = "results.csv";

    public string[] checklistIds = { "Q1","Q2","Q3","Q4","Q5","Q6" };

    public string memoKey1 = "memo_when_with_whom";
    public string memoKey2 = "wish_free_memo";

    public string doljabiKey = "doljabi_pick";
    
    public string[] baseHeader = {
        "타임스탬프","닉네임","생년월일","정체성라벨","정체성퍼센트",
        "총점","밴드","메모1","메모2","돌잡이선택"
    };
    
    public void ExportNow()
    {
        string buildRoot = Directory.GetParent(Application.dataPath)?.FullName
                           ?? Application.persistentDataPath;
        string dir  = Path.Combine(buildRoot, folderName);
        string path = Path.Combine(dir, fileName);
        Directory.CreateDirectory(dir);

        LocalJsonDB.EnsureSession();
        var rec = LocalJsonDB.Current;
        if (rec == null)
        {
            Debug.LogWarning("[CsvAtUserData] 세션 없음");
            return;
        }

        if (!File.Exists(path))
        {
            var head = string.Join(",", baseHeader.Select(Csv)) + "," +
                       string.Join(",", checklistIds.Select(Csv)) + "\n";
            File.WriteAllText(path, head, new UTF8Encoding(encoderShouldEmitUTF8Identifier:true));
        }

        int total = 0;
        var choiceVals = new string[checklistIds.Length];
        for (int i = 0; i < checklistIds.Length; i++)
        {
            int v = LocalJsonDB.TryGetChecklistInt(checklistIds[i], out var got) ? got : 0;
            total += v;
            choiceVals[i] = v.ToString();
        }
        total = Mathf.Clamp(total, 0, 24);
        string band = "";
        if (total <= 6)
            band = "정상";
        else if (total == 7)
            band = "경도";
        else if (total <= 18)
            band = "중등도";
        else
            band = "고위험";
        
        string memo1   = LocalJsonDB.GetFreeText(memoKey1);
        string memo2   = LocalJsonDB.GetFreeText(memoKey2);
        string doljabi = LocalJsonDB.GetFreeText(doljabiKey);
        
        var sb = new StringBuilder(256);
        sb.Append(Csv(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))).Append(",");
        sb.Append(Csv(rec.nickname)).Append(",");
        sb.Append(Csv(rec.birthYYMMDD)).Append(",");
        sb.Append(Csv(rec.identityLabel)).Append(",");
        sb.Append(Csv(rec.identityPercent.ToString())).Append(",");
        sb.Append(Csv(total.ToString())).Append(",");
        sb.Append(Csv(band)).Append(",");
        sb.Append(Csv(memo1)).Append(",");
        sb.Append(Csv(memo2)).Append(",");
        sb.Append(Csv(doljabi));

        foreach (var v in choiceVals) sb.Append(",").Append(Csv(v));
        sb.AppendLine();

        File.AppendAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier:true));
#if UNITY_EDITOR
        Debug.Log($"[CsvAtUserData] CSV 저장: {path}");
#endif
    }

    static string Csv(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        bool q = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
        s = s.Replace("\"", "\"\"");
        return q ? $"\"{s}\"" : s;
    }
}
