using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class ClickAnywhereNext : MonoBehaviour
{
    public GameObject nextPage;
    public GameObject thisPage;
    public float clickCooldown = 0.15f;
    public List<TMP_InputField> blockTMPInputs;
    public bool requireTextComplete = false;
    public List<StaggeredTextShow> gates = new();

    float _readyTime;
    bool  _armedByEvent = false;
    bool  _consumedOnce = false;

    void Awake()
    {
        if (!thisPage) thisPage = gameObject;
        _readyTime = Time.unscaledTime + 0.1f;
    }

    void OnEnable()
    {
        _consumedOnce = false;
        _armedByEvent = false;
        _readyTime = Time.unscaledTime + 0.1f;
    }

    void Update()
    {
        if (_consumedOnce) return;
        if (Time.unscaledTime < _readyTime) return;
        
        if (requireTextComplete && !IsAllGatesCompleted() && !_armedByEvent) return;

        bool down =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) ||
            (Pen.current != null && Pen.current.tip.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (!down) return;

        if (IsPointerOverUI()) return;

        if (IsAnyInputFocused()) return;

        GoNext();
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        return false;
    }

    bool IsAnyInputFocused()
    {
        if (blockTMPInputs != null)
            foreach (var f in blockTMPInputs)
                if (f && f.isFocused) return true;
        return false;
    }

    bool IsAllGatesCompleted()
    {
        if (gates == null || gates.Count == 0) return true;
        return gates.All(g => g != null && g.Completed);
    }
    
    public void GoNext()
    {
        if (!nextPage) return;
        nextPage.SetActive(true);
        thisPage.SetActive(false);
        _readyTime = Time.unscaledTime + clickCooldown;
        _consumedOnce = true;
    }
}
