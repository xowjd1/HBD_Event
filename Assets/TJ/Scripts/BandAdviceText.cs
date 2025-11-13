using UnityEngine;
using TMPro;

public class BandAdviceText : MonoBehaviour
{
    public TMP_Text target;

    [Header("밴드별 문단 (0=정상,1=경도,2=중등도,3=고위험)")]
    [TextArea(4,10)] public string descNormal =
        "스트레스 평가 결과\n건강하게 스트레스를 잘 관리하고 계십니다.\n현재의 상태를 잘 유지하시고\n스스로의 스트레스를 지금처럼 잘 조절하시기 바랍니다.";
    [TextArea(4,10)] public string descMild =
        "경도의 스트레스 상태입니다.\n스트레스의 영향을 조금 받고 있는 상태입니다.\n스트레스를 줄이고 긍정적인 경험을 하는 등\n스트레스 관리가 필요합니다.";
    [TextArea(4,10)] public string descModerate =
        "중등도의 스트레스 상태입니다.\n지속적인 스트레스는 우울, 불안 등 정신적인 어려움으로 이어질 수 있으므로,\n적극적으로 스트레스 관리를 하는 것이 필요합니다.";
    [TextArea(4,10)] public string descSevere =
        "심한 스트레스 상태입니다.\n적극적인 스트레스 관리가 필요하며\n혼자서 해결하기 어려울 수 있으므로\n전문가의 도움을 적극적으로 받으시기 바랍니다.";

    [Header("문항 IDs (체크리스트 questionId와 동일)")]
    public string[] questionIds = new string[]
    {
        "Q1_unexpected","Q1_sensitive","Q1_control",
        "Q2_irritability_coping","Q2_best_condition","Q2_overwhelmed",
    };

    [Header("밴드별로 켤 이미지 (0~3 순서로 놓기)")]
    public GameObject[] bandImages;

    void Awake()
    {
        if (!target) target = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        Render();
    }

    public void Render()
    {
        if (!target) return;

        var rec   = LocalJsonDB.Current;
        int total = SumScore(rec);
        int band  = GetBand(total);
        
        target.text = band switch
        {
            0 => descNormal,
            1 => descMild,
            2 => descModerate,
            _ => descSevere
        };

        ShowBandImage(band);
    }

    void ShowBandImage(int band)
    {
        if (bandImages == null || bandImages.Length == 0) return;

        for (int i = 0; i < bandImages.Length; i++)
        {
            if (!bandImages[i]) continue;
            bandImages[i].SetActive(i == band);
        }
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
        if (total <= 6)  return 0;
        if (total == 7)  return 1;
        if (total <= 18) return 2;
        return 3;
    }
}
