using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IdentityPageController : MonoBehaviour
{
    public Button btnMom;
    public Button btnDad;
    public Button btnWorker;
    public Button btnYouth;
    public Button btnStudent;
    public GameObject[] buttonHighlights;
    public TMP_InputField customInput;
    public GameObject customHighlight;
    public GameObject afterPickRoot;
    CanvasGroup _afterPickCG;

    [TextArea] public string sentenceFormat =
        "아, {0}은 {1}{2} 되었군요? 과거에 내가 생각한 것과 같나요?";
    public TMP_Text sentenceText;
    public Slider   percentSlider;
    public TMP_Text percentLabel;
    public Button     nextButton;
    public GameObject nextPage;
    public GameObject thisPage;

    string  _selected;
    Button[] _fixedButtons;

    void Awake()
    {
        if (!thisPage) thisPage = gameObject;
        LocalJsonDB.EnsureSession();

        _fixedButtons = new[] { btnMom, btnDad, btnWorker, btnYouth, btnStudent };
        
        if (afterPickRoot)
        {
            _afterPickCG = afterPickRoot.GetComponent<CanvasGroup>();
            if (_afterPickCG == null) _afterPickCG = afterPickRoot.AddComponent<CanvasGroup>();
            ShowAfterPick(false, instant:true);
        }

        ForceAllHighlightsOff();

        Wire(btnMom,    () => SelectFixed("엄마",    0));
        Wire(btnDad,    () => SelectFixed("아빠",    1));
        Wire(btnWorker, () => SelectFixed("직장인",  2));
        Wire(btnYouth,  () => SelectFixed("청년",    3));
        Wire(btnStudent,() => SelectFixed("학생",    4));

        if (customInput)
        {
            customInput.onValueChanged.RemoveAllListeners();
            customInput.onValueChanged.AddListener(_ =>
            {
                string t = customInput.text.Trim();
                if (t.Length > 0)
                {
                    _selected = t;
                    ShowOnlyCustomHighlight(true);
                }
                else
                {
                    if (IsCustomSelected()) _selected = null;
                    ShowOnlyCustomHighlight(false);
                }
                UpdateSentence();
                RefreshUIState();
            });
        }

        if (percentSlider)
        {
            percentSlider.minValue = 0;
            percentSlider.maxValue = 100;
            percentSlider.onValueChanged.RemoveAllListeners();
            percentSlider.onValueChanged.AddListener(v =>
            {
                if (percentLabel) percentLabel.SetText($"{Mathf.RoundToInt(v)}%");
            });
        }

        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnSaveAndNext);
        }

        RestoreFromDB();
        RefreshUIState();
    }

    void OnEnable()
    {
        RestoreFromDB();
        RefreshUIState();
    }

    void ForceAllHighlightsOff()
    {
        if (buttonHighlights != null)
            foreach (var go in buttonHighlights) if (go) go.SetActive(false);
        if (customHighlight) customHighlight.SetActive(false);
    }

    void ShowOnlyButtonHighlight(int index)
    {
        if (buttonHighlights != null)
            for (int i = 0; i < buttonHighlights.Length; i++)
                if (buttonHighlights[i]) buttonHighlights[i].SetActive(i == index);
        if (customHighlight) customHighlight.SetActive(false);
    }

    void ShowOnlyCustomHighlight(bool on)
    {
        if (buttonHighlights != null)
            foreach (var go in buttonHighlights) if (go) go.SetActive(false);
        if (customHighlight) customHighlight.SetActive(on);
    }

    bool IsCustomSelected()
    {
        return customInput && customInput.text.Trim().Length > 0
               && customHighlight && customHighlight.activeSelf;
    }

    void Wire(Button b, System.Action act)
    {
        if (!b) return;
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(() => act?.Invoke());
    }

    void SelectFixed(string label, int fixedIndex)
    {
        _selected = label;
        ShowOnlyButtonHighlight(fixedIndex);
        if (customInput) customInput.DeactivateInputField();
        UpdateSentence();
        RefreshUIState();
    }

    void UpdateSentence()
    {
        if (!sentenceText) return;
        var rec  = LocalJsonDB.Current;
        string nick = (rec != null && !string.IsNullOrWhiteSpace(rec.nickname)) ? rec.nickname : "닉네임";
        string pick = string.IsNullOrWhiteSpace(_selected) ? "…" : _selected;
        string particle = ChooseSubjectParticle(pick);
        sentenceText.SetText(string.Format(sentenceFormat, nick, pick, particle));
    }

    string ChooseSubjectParticle(string word)
    {
        if (string.IsNullOrEmpty(word)) return "가";
        char last = word[word.Length - 1];
        if (last < 0xAC00 || last > 0xD7A3) return "가";
        int code = last - 0xAC00;
        int jong = code % 28;
        return (jong == 0) ? "가" : "이";
    }

    bool IsValid() => !string.IsNullOrWhiteSpace(_selected);

    void RefreshUIState()
    {
        bool ok = IsValid();
        ShowAfterPick(ok);
        if (nextButton) nextButton.interactable = ok;
    }

    void ShowAfterPick(bool show, bool instant = false)
    {
        if (!afterPickRoot) return;

        if (_afterPickCG == null)
        {
            afterPickRoot.SetActive(show);
            return;
        }

        if (instant)
        {
            afterPickRoot.SetActive(true);
            _afterPickCG.alpha = show ? 1f : 0f;
            _afterPickCG.blocksRaycasts = show;
            _afterPickCG.interactable = show;
            if (!show) afterPickRoot.SetActive(false);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FadeAfterPick(show ? 1f : 0f, show));
        }
    }

    System.Collections.IEnumerator FadeAfterPick(float target, bool finalActive)
    {
        afterPickRoot.SetActive(true);
        _afterPickCG.blocksRaycasts = finalActive;
        _afterPickCG.interactable   = finalActive;

        float start = _afterPickCG.alpha;
        float t = 0f, dur = 0.18f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            _afterPickCG.alpha = Mathf.Lerp(start, target, t / dur);
            yield return null;
        }
        _afterPickCG.alpha = target;

        if (!finalActive) afterPickRoot.SetActive(false);
    }

    void OnSaveAndNext()
    {
        if (!IsValid()) { RefreshUIState(); return; }
        int pct = percentSlider ? Mathf.RoundToInt(percentSlider.value) : 0;

        LocalJsonDB.UpdateCurrent(r =>
        {
            r.identityLabel   = _selected;
            r.identityPercent = Mathf.Clamp(pct, 0, 100);
        });

        if (nextPage) { nextPage.SetActive(true); thisPage.SetActive(false); }
    }

    void RestoreFromDB()
    {
        var rec = LocalJsonDB.Current;

        int pct = (rec != null) ? Mathf.Clamp(rec.identityPercent, 0, 100) : 80;
        if (percentSlider) percentSlider.value = pct;
        if (percentLabel)  percentLabel.SetText($"{pct}%");

        string label = (rec != null) ? rec.identityLabel : null;

        if (string.IsNullOrEmpty(label))
        {
            _selected = null;
            ForceAllHighlightsOff();
        }
        else
        {
            _selected = label;
            int idx = LabelToFixedIndex(label);
            if (idx >= 0)
            {
                ShowOnlyButtonHighlight(idx);
                if (customInput) customInput.text = "";
            }
            else
            {
                if (customInput) customInput.text = label;
                ShowOnlyCustomHighlight(true);
            }
        }

        UpdateSentence();
    }

    int LabelToFixedIndex(string label)
    {
        if      (label == "엄마")   return 0;
        else if (label == "아빠")   return 1;
        else if (label == "직장인") return 2;
        else if (label == "청년")   return 3;
        else if (label == "학생")   return 4;
        return -1;
    }
}
