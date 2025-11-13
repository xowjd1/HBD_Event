using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class StaggeredTextShow : MonoBehaviour
{
    public List<TMP_Text> texts = new();
    public float firstDelay = 0.2f;
    public float gapDelay   = 0.35f;
    public float fadeDuration = 0.3f;
    public bool useTypewriter = false;
    public float charsPerSecond = 40f;
    public List<Selectable> finalSelectables = new();
    public bool buttonsAppearTogether = true;
    public float finalButtonsDelay = 0.2f;
    public float buttonGap = 0.15f;
    public bool playOnEnable = true;
    public bool resetOnDisable = true;
    public UnityEvent onCompleted;
    public bool Completed { get; private set; }
    
    readonly List<CanvasGroup> _textCGs = new();
    readonly List<CanvasGroup> _btnCGs  = new();

    void Awake()
    {
        EnsureTextCGs();
        EnsureButtonCGs();
        HideImmediate();
    }

    void OnEnable()
    {
        if (playOnEnable) StartPlay();
    }

    void OnDisable()
    {
        if (resetOnDisable) HideImmediate();
        Completed = false;
    }

    public void StartPlay()
    {
        StopAllCoroutines();
        Completed = false;
        StartCoroutine(PlayRoutine());
    }

    void EnsureTextCGs()
    {
        _textCGs.Clear();
        foreach (var t in texts)
        {
            if (!t) { _textCGs.Add(null); continue; }
            var cg = t.GetComponent<CanvasGroup>();
            if (!cg) cg = t.gameObject.AddComponent<CanvasGroup>();
            _textCGs.Add(cg);
        }
    }

    void EnsureButtonCGs()
    {
        _btnCGs.Clear();
        foreach (var sel in finalSelectables)
        {
            if (!sel) { _btnCGs.Add(null); continue; }
            var cg = sel.GetComponent<CanvasGroup>();
            if (!cg) cg = sel.gameObject.AddComponent<CanvasGroup>();
            _btnCGs.Add(cg);
        }
    }

    void HideImmediate()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            var t = texts[i];
            if (!t) continue;

            if (useTypewriter) t.maxVisibleCharacters = 0;
            var cg = _textCGs[i];
            if (cg) cg.alpha = 0f;
        }

        for (int i = 0; i < finalSelectables.Count; i++)
        {
            var sel = finalSelectables[i];
            var cg  = _btnCGs[i];
            if (!sel || !cg) continue;

            sel.interactable = false;
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            sel.gameObject.SetActive(true);
        }
    }

    IEnumerator PlayRoutine()
    {
        yield return new WaitForSeconds(firstDelay);
        
        for (int i = 0; i < texts.Count; i++)
        {
            var t  = texts[i];
            var cg = _textCGs[i];

            if (t)
            {
                t.gameObject.SetActive(true);
                if (useTypewriter) yield return StartCoroutine(TypeIn(t, cg));
                else               yield return StartCoroutine(FadeIn(cg));
            }

            if (i < texts.Count - 1) yield return new WaitForSeconds(gapDelay);
        }
        
        if (finalSelectables.Count > 0)
        {
            yield return new WaitForSeconds(finalButtonsDelay);

            if (buttonsAppearTogether)
            {
                List<Coroutine> runs = new();
                for (int i = 0; i < finalSelectables.Count; i++)
                {
                    var cg = _btnCGs[i];
                    if (!cg) continue;
                    runs.Add(StartCoroutine(FadeIn(cg)));
                }
                foreach (var c in runs) yield return c;
                
                for (int i = 0; i < finalSelectables.Count; i++)
                {
                    var sel = finalSelectables[i];
                    var cg  = _btnCGs[i];
                    if (!sel || !cg) continue;
                    cg.blocksRaycasts = true;
                    sel.interactable = true;
                }
            }
            else
            {
                for (int i = 0; i < finalSelectables.Count; i++)
                {
                    var sel = finalSelectables[i];
                    var cg  = _btnCGs[i];
                    if (!sel || !cg) continue;

                    yield return StartCoroutine(FadeIn(cg));
                    cg.blocksRaycasts = true;
                    sel.interactable = true;

                    if (i < finalSelectables.Count - 1)
                        yield return new WaitForSeconds(buttonGap);
                }
            }
        }

        Completed = true;
        onCompleted?.Invoke();
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        if (!cg) yield break;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    IEnumerator TypeIn(TMP_Text tmp, CanvasGroup cg)
    {
        if (cg) cg.alpha = 1f;
        tmp.ForceMeshUpdate();
        int total = tmp.textInfo.characterCount;
        if (total <= 0) yield break;

        int shown = 0;
        float perChar = 1f / Mathf.Max(1f, charsPerSecond);
        while (shown < total)
        {
            shown++;
            tmp.maxVisibleCharacters = shown;
            yield return new WaitForSeconds(perChar);
        }
    }
}
