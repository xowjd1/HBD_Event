using UnityEngine;
using TMPro;

public class ScoreSummaryText : MonoBehaviour
{
    public TMP_Text target;
    [TextArea] public string formatBand = "{0}님은 {1}입니다.";
    public string fallbackName = "사용자";
    public string[] bandNames = { "정상", "경도", "중등도", "고위험" };
    public string[] questionIds = new string[]
    {
        "Q1_unexpected","Q1_sensitive","Q1_control",
        "Q2_irritability_coping","Q2_best_condition","Q2_overwhelmed",
    };

    void Awake(){ if (!target) target = GetComponent<TMP_Text>(); }
    void OnEnable(){ Render(); }

    public void Render()
    {
        if (!target) return;

        var rec = LocalJsonDB.Current;
        string name = (rec != null && !string.IsNullOrWhiteSpace(rec.nickname)) ? rec.nickname : fallbackName;

        int total = SumScore(rec);
        int band  = GetBand(total);
        string bandText = (bandNames != null && band >= 0 && band < bandNames.Length) ? bandNames[band] : "정상";

        target.SetText(string.Format(formatBand, name, bandText));
    }

    int SumScore(LocalJsonDB.Record rec)
    {
        int sum = 0;
        if (rec == null || rec.checklistKeys == null) return 0;

        foreach (var id in questionIds)
        {
            if (string.IsNullOrEmpty(id)) continue;
            int idx = rec.checklistKeys.IndexOf(id);
            if (idx < 0) continue;
            if (int.TryParse(rec.checklistValues[idx], out int v))
                sum += Mathf.Clamp(v, 0, 4);
        }
        return Mathf.Clamp(sum, 0, 24);
    }

    int GetBand(int total)
    {
        if (total <= 6)  return 0; // 정상
        if (total == 7)  return 1; // 경도
        if (total <= 18) return 2; // 중등도
        return 3;                  // 고위험
    }
}