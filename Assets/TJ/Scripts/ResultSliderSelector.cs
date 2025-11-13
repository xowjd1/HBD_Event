using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResultSliderSelector : MonoBehaviour
{
    public string[] questionIds = new string[]
    {
        "Q1_unexpected","Q1_sensitive","Q1_control",
        "Q2_irritability_coping","Q2_best_condition","Q2_overwhelmed",
    };
    public Slider[] sliders;           
    public GameObject slidersRoot;
    public TMP_Text scoreText;
    public TMP_Text bandText;
    public TMP_Text headlineText;
    public bool showNumericScore = false;
    public bool bandTextShowRanges = false;
    public string[] bandNames = { "정상군", "경도", "중등도", "고위험" };
    public bool animate = true;
    public float animDuration = 0.5f;
    public bool startFromZero = true;
    public bool useInspectorSliderValue = true;
    
    float[] _designTarget01;

    void Awake()
    {
        
        if (sliders != null && sliders.Length > 0)
        {
            _designTarget01 = new float[sliders.Length];
            for (int i = 0; i < sliders.Length; i++)
            {
                var s = sliders[i];
                if (!s)
                {
                    _designTarget01[i] = 0f;
                    continue;
                }
                float min = s.minValue;
                float max = (s.maxValue <= min) ? (min + 1f) : s.maxValue;
                _designTarget01[i] = Mathf.InverseLerp(min, max, s.value);
            }
        }
    }

    void OnEnable() => Render();

    public void Render()
    {
        int total = SumScore();
        int band  = GetBand(total);

        if (scoreText)
        {
            scoreText.gameObject.SetActive(showNumericScore);
            if (showNumericScore) scoreText.SetText($"총점: {total}");
        }

        if (bandText)
        {
            string name = SafeBandName(band);
            if (bandTextShowRanges)
            {
                bandText.SetText(band switch
                {
                    0 => $"{name} (0~6점)",
                    1 => $"{name} (7점)",
                    2 => $"{name} (8~18점)",
                    _ => $"{name} (19~24점)"
                });
            }
            else bandText.SetText(name);
        }

        if (headlineText)
        {
            string nick = "사용자";
            if (LocalJsonDB.Current != null && !string.IsNullOrWhiteSpace(LocalJsonDB.Current.nickname))
                nick = LocalJsonDB.Current.nickname;
            headlineText.SetText($"{nick}님의 상태는 <b>{SafeBandName(band)}</b>입니다.");
        }
        
        ShowOnly(band);

        if (sliders == null || band < 0 || band >= sliders.Length) return;
        var s = sliders[band];
        if (!s) return;
        
        float target01;
        if (useInspectorSliderValue && _designTarget01 != null && band < _designTarget01.Length)
        {
            target01 = _designTarget01[band];
        }
        else
        {
            target01 = band switch
            {
                0 => Mathf.InverseLerp(0f, 6f,  total),
                1 => 1f,
                2 => Mathf.InverseLerp(8f, 18f, total),
                _ => Mathf.InverseLerp(19f, 24f, total)
            };
        }

        float start01 = startFromZero ? 0f : s.normalizedValue;

        if (!animate || animDuration <= 0f)
        {
            s.normalizedValue = target01;
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(AnimateSlider(s, start01, target01, animDuration));
        }
    }

    int SumScore()
    {
        int sum = 0;
        var rec = LocalJsonDB.Current;
        if (rec == null) return 0;

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

    string SafeBandName(int band)
    {
        if (bandNames != null && band >= 0 && band < bandNames.Length && !string.IsNullOrEmpty(bandNames[band]))
            return bandNames[band];
        return band switch { 0 => "정상군", 1 => "경도", 2 => "중등도", _ => "고위험" };
    }

    void ShowOnly(int idx)
    {
        if (sliders == null) return;
        for (int i = 0; i < sliders.Length; i++)
            if (sliders[i]) sliders[i].gameObject.SetActive(i == idx);
        if (slidersRoot) slidersRoot.SetActive(true);
    }

    IEnumerator AnimateSlider(Slider s, float start, float target, float dur)
    {
        float t = 0f;
        s.normalizedValue = start;
        if (dur <= 0f) { s.normalizedValue = target; yield break; }

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            s.normalizedValue = Mathf.Lerp(start, target, k);
            yield return null;
        }
        s.normalizedValue = target;
    }
}
