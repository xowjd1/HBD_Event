using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DoljabiPicker : MonoBehaviour
{
    public string saveKey = "doljabi_pick";
    public Button[] optionButtons;
    public string[] optionLabels;
    public GameObject[] targetPages;
    GameObject currentPageRoot;
    public bool goImmediately = true;
    public float goDelay = 0.08f;

    void Awake()
    {
        if (!currentPageRoot) currentPageRoot = gameObject;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i;
            var btn = optionButtons[i];
            if (!btn) continue;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnPick(idx));
        }
    }

    public void OnPick(int index)
    {
        string label =
            (optionLabels != null && index < optionLabels.Length && !string.IsNullOrEmpty(optionLabels[index]))
            ? optionLabels[index]
            : (optionButtons != null && index < optionButtons.Length && optionButtons[index]
                ? optionButtons[index].name
                : $"Option{index}");
        
        LocalJsonDB.SetFreeText(saveKey, label);

        if (goImmediately || goDelay <= 0f) ActivateOnly(index);
        else StartCoroutine(GoAfterDelay(index));
    }

    IEnumerator GoAfterDelay(int index)
    {
        yield return new WaitForSecondsRealtime(goDelay);
        ActivateOnly(index);
    }

    void ActivateOnly(int index)
    {
        if (targetPages != null)
        {
            for (int i = 0; i < targetPages.Length; i++)
                if (targetPages[i]) targetPages[i].SetActive(i == index);
        }

        if (currentPageRoot) currentPageRoot.SetActive(false);
    }
}
