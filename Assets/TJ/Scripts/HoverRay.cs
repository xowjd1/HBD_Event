using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HoverRay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Graphic targetGraphic;
    public CanvasGroup rays;
    
    public float fadeIn = 0.18f;
    public float fadeOut = 0.25f;
    public float maxAlpha = 0.45f;
    public float rotateSpeed = 18f;
    public Vector2 scaleRange = new(1.05f, 1.20f);
    public float pulseSpeed = 0.8f;

    public Color outlineColor = new(0.15f, 0.35f, 1f, 1f);
    public Vector2 maxDistance = new(6f, -6f);
    public float outlineIn  = 0.15f;
    public float outlineOut = 0.15f;

    RectTransform _raysRT;
    Coroutine _fadeCo, _outlineCo;
    bool _hover;
    Outline _outline;

    void Awake()
    {
        if (!targetGraphic) targetGraphic = GetComponent<Graphic>();
        
        _raysRT = rays ? rays.GetComponent<RectTransform>() : null;
        if (rays)
        {
            rays.blocksRaycasts = false;

            PlaceRaysBehindGraphic();

            rays.alpha = 0f;
            if (_raysRT) _raysRT.localScale = Vector3.one * scaleRange.x;
        }
        
        if (targetGraphic)
        {
            _outline = targetGraphic.GetComponent<Outline>();
            if (!_outline) _outline = targetGraphic.gameObject.AddComponent<Outline>();
            _outline.effectColor    = outlineColor;
            _outline.effectDistance = Vector2.zero;
            _outline.enabled        = false;
        }
    }

    void Update()
    {
        if (!_hover || !_raysRT || !rays) return;

        _raysRT.Rotate(0f, 0f, -rotateSpeed * Time.unscaledDeltaTime);

        float t = Time.unscaledTime * pulseSpeed;
        float s = Mathf.Lerp(scaleRange.x, scaleRange.y, 0.5f + 0.5f * Mathf.Sin(t));
        _raysRT.localScale = new Vector3(s, s, 1f);

        float a = Mathf.Lerp(maxAlpha * 0.7f, maxAlpha, 0.5f + 0.5f * Mathf.Sin(t));
        rays.alpha = Mathf.Min(a, maxAlpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hover = true;
        if (rays) StartFade(true);
        StartOutline(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hover = false;
        if (rays) StartFade(false);
        StartOutline(false);
    }

    void OnDisable()
    {
        _hover = false;
        if (_fadeCo    != null) StopCoroutine(_fadeCo);
        if (_outlineCo != null) StopCoroutine(_outlineCo);
        if (rays) rays.alpha = 0f;
        if (_outline) { _outline.enabled = false; _outline.effectDistance = Vector2.zero; }
    }

    void PlaceRaysBehindGraphic()
    {
        if (!_raysRT) return;
        _raysRT.SetAsFirstSibling();

        if (!targetGraphic)
        {
            targetGraphic = GetComponentInChildren<Graphic>(true);
        }
    }

    void StartFade(bool enter)
    {
        if (_fadeCo != null) StopCoroutine(_fadeCo);
        _fadeCo = StartCoroutine(FadeRoutine(enter));
    }
    IEnumerator FadeRoutine(bool enter)
    {
        float dur = enter ? fadeIn : fadeOut;
        float t = 0f;
        float a0 = rays.alpha;
        float a1 = enter ? maxAlpha : 0f;

        if (dur <= 0f) { rays.alpha = a1; yield break; }
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            rays.alpha = Mathf.Lerp(a0, a1, k);
            yield return null;
        }
        rays.alpha = a1;
    }

    void StartOutline(bool enter)
    {
        if (!_outline) return;
        if (_outlineCo != null) StopCoroutine(_outlineCo);
        _outlineCo = StartCoroutine(OutlineRoutine(enter));
    }
    IEnumerator OutlineRoutine(bool enter)
    {
        _outline.enabled = true;
        Vector2 d0 = _outline.effectDistance;
        Vector2 d1 = enter ? maxDistance : Vector2.zero;
        float dur = enter ? outlineIn : outlineOut;

        float t = 0f;
        if (dur <= 0f) { _outline.effectDistance = d1; if (d1 == Vector2.zero) _outline.enabled = false; yield break; }
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / dur);
            _outline.effectDistance = Vector2.Lerp(d0, d1, k);
            yield return null;
        }
        _outline.effectDistance = d1;
        if (d1 == Vector2.zero) _outline.enabled = false;
    }
}
