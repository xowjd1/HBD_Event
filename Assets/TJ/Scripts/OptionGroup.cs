using System;
using System.Collections.Generic;
using UnityEngine;

public class OptionGroup : MonoBehaviour
{
    public string questionId;
    public List<OptionButton> options = new();
    public event Action OnSelectionChanged;
    int _selectedIndex = -1;

    void Awake() {
        foreach (var o in options) if (o) o.group = this;
        UpdateVisual();
    }

    public void Select(OptionButton ob) {
        _selectedIndex = options.IndexOf(ob);
        UpdateVisual();
        OnSelectionChanged?.Invoke();
        SendMessageUpwards("RefreshNextInteractable", SendMessageOptions.DontRequireReceiver);
    }

    void UpdateVisual() {
        for (int i = 0; i < options.Count; i++)
            options[i]?.SetSelected(i == _selectedIndex);
    }

    public bool HasSelection() => _selectedIndex >= 0;
    public string GetValue() => HasSelection() ? options[_selectedIndex]?.value : "";

    public void RestoreFromDB() {
        var rec = LocalJsonDB.Current;
        if (rec == null) return;
        int idx = rec.checklistKeys.IndexOf(questionId);
        if (idx < 0) return;
        string saved = rec.checklistValues[idx];
        for (int i = 0; i < options.Count; i++) {
            if (options[i] && options[i].value == saved) { Select(options[i]); break; }
        }
    }
    public void ClearSelection()
    {
        _selectedIndex = -1;
        UpdateVisual();
        OnSelectionChanged?.Invoke();
    }

}