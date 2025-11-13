using UnityEngine;
using TMPro;
using System;
public class FinalSummaryPresenter : MonoBehaviour
{

    [SerializeField] private TMP_Text lineBirthNick;
    [SerializeField] TMP_Text lineMemo1;
    [SerializeField] TMP_Text lineMemo2;
    [SerializeField] TMP_Text lineBand;
    [SerializeField] GameObject[] bandIcons;
    [SerializeField] int[] bandToIconIndex = new int[] { 0, 1, 2, 3 };
    [SerializeField] bool showYear = false;

    void OnEnable()
    {
        if (!lineBirthNick) lineBirthNick = GetComponent<TMP_Text>();

        LocalJsonDB.EnsureSession();
        var r = LocalJsonDB.GetLastFilledRecord();

        if (r == null) { RenderEmpty(); return; }

        string nickname = string.IsNullOrWhiteSpace(r.nickname) ? "사용자" : r.nickname.Trim();
        string birthStr = FormatBirth(r.birthYYMMDD, showYear);

        string memo1 = LocalJsonDB.GetFreeTextFromRecord(r, "Question1");
        string memo2 = LocalJsonDB.GetFreeTextFromRecord(r, "Question2");
        if (string.IsNullOrWhiteSpace(memo1)) memo1 = "______________________";
        if (string.IsNullOrWhiteSpace(memo2)) memo2 = "__________";

        int total = 0;
        for (int i = 0; i < r.checklistValues.Count; i++)
            if (int.TryParse(r.checklistValues[i], out var v)) total += Mathf.Clamp(v, 0, 4);
        total = Mathf.Clamp(total, 0, 24);

        int band = GetBand(total);
        string bandKo = BandKorean(band);

        if (lineBirthNick) lineBirthNick.text = $"{birthStr}에 태어난 <{nickname}>님은";
        if (lineMemo1)     lineMemo1.text     = $"가장 기억에 남는 생일로 {memo1}";
        if (lineMemo2)     lineMemo2.text     = $"앞으로는 {memo2} 라고 했습니다";
        if (lineBand)      lineBand.text      = $"스트레스 결과는 < {bandKo} > 입니다";
        
        ApplyBandIcons(band);
    }

    void ApplyBandIcons(int band)
    {
        if (bandIcons == null || bandIcons.Length == 0) return;

        int iconIdx = 0;
        if (band >= 0 && band < bandToIconIndex.Length)
            iconIdx = Mathf.Clamp(bandToIconIndex[band], 0, bandIcons.Length - 1);

        for (int i = 0; i < bandIcons.Length; i++)
            if (bandIcons[i]) bandIcons[i].SetActive(i == iconIdx);
    }

    static int GetBand(int total)
    {
        if (total <= 6)  return 0;  // 정상
        if (total == 7)  return 1;  // 경도
        if (total <= 18) return 2;  // 중등도
        return 3;                   // 고위험
    }

    static string BandKorean(int band)
    {
        return band switch
        {
            0 => "정상",
            1 => "경도",
            2 => "중등도",
            _ => "고위험"
        };
    }
    
    static string FormatBirth(string ymd, bool acceptTwoDigitYear = true)
    {
        if (string.IsNullOrWhiteSpace(ymd)) return "";

        ymd = ymd.Trim();
        int year, month, day;

        if (ymd.Length == 8)
        {
            if (!int.TryParse(ymd.Substring(0, 4), out year))  return "";
            if (!int.TryParse(ymd.Substring(4, 2), out month)) return "";
            if (!int.TryParse(ymd.Substring(6, 2), out day))   return "";
        }
        else if (ymd.Length == 6 && acceptTwoDigitYear)
        {
            if (!int.TryParse(ymd.Substring(0, 2), out var yy)) return "";
            if (!int.TryParse(ymd.Substring(2, 2), out month))  return "";
            if (!int.TryParse(ymd.Substring(4, 2), out day))    return "";

            year = (yy <= 49) ? (2000 + yy) : (1900 + yy);
        }
        else
        {
            return "";
        }
        
        if (month < 1 || month > 12) return "";
        if (day   < 1 || day   > 31) return "";
        
        return $"{year}년 {month:00}월 {day:00}일";
    }
    
    void RenderEmpty()
    {
        if (lineBirthNick) lineBirthNick.text = "0월 0일에 태어난 <사용자>님은";
        if (lineMemo1)     lineMemo1.text     = "가장 기억에 남는 생일로 ______________________";
        if (lineMemo2)     lineMemo2.text     = "앞으로는 __________ 라고 했습니다";
        if (lineBand)      lineBand.text      = "스트레스 결과는 < 정상 > 입니다";
        ApplyBandIcons(0);
    }
}
