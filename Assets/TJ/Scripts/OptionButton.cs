using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class OptionButton : MonoBehaviour
{
    public OptionGroup group;
    public string value;
    public Image bg;
    public TMP_Text label;
    public Color normalColor   = Color.white;
    public Color selectedColor = new Color(0.90f, 0.94f, 1f, 1f);
    public Color normalLabel   = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color selectedLabel = new Color(0.10f, 0.20f, 0.45f, 1f);
    Button _btn;

    void Awake() {
        if (!bg) bg = GetComponent<Image>();
        _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(() => group.Select(this));
        SetSelected(false);
    }

    public void SetSelected(bool on) {
        if (bg)    bg.color    = on ? selectedColor : normalColor;
        if (label) label.color = on ? selectedLabel : normalLabel;
    }
}