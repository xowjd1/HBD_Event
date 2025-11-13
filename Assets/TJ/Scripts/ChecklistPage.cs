using UnityEngine;
using UnityEngine.UI;

public class ChecklistPage : MonoBehaviour
{
    public OptionGroup[] groups;
    public Button nextButton;
    public GameObject nextPage;
    public GameObject thisPage;

    void Awake() {
        if (!thisPage) thisPage = gameObject;

        LocalJsonDB.EnsureSession();

        if (groups == null || groups.Length == 0)
            groups = GetComponentsInChildren<OptionGroup>(includeInactive: true);

        foreach (var g in groups) if (g != null) g.OnSelectionChanged += RefreshNextInteractable;

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNext);

        RefreshNextInteractable();
    }

    void OnEnable() {
        foreach (var g in groups) g?.RestoreFromDB();
        RefreshNextInteractable();
    }

    public void RefreshNextInteractable() {
        bool allSelected = true;
        foreach (var g in groups) {
            if (!g || !g.HasSelection()) { allSelected = false; break; }
        }
        nextButton.interactable = allSelected;
    }

    void OnNext() {
        if (!nextButton.interactable) { RefreshNextInteractable(); return; }

        LocalJsonDB.UpdateCurrent(r => {
            foreach (var g in groups) {
                if (g == null) continue;
                int idx = r.checklistKeys.IndexOf(g.questionId);
                if (idx >= 0) r.checklistValues[idx] = g.GetValue();
                else { r.checklistKeys.Add(g.questionId); r.checklistValues.Add(g.GetValue()); }
            }
        });

        if (nextPage) {
            nextPage.SetActive(true);
            thisPage.SetActive(false);
        }
    }
}