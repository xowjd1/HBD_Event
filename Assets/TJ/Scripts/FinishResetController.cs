using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FinishResetController : MonoBehaviour
{
    public Button finishButton;
    public GameObject firstPage;
    public GameObject[] allPages;
    public bool clearPlayerPrefs = false;

    void Awake()
    {
        if (finishButton)
        {
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(Finish);
        }
    }

    public void Finish()
    {
        ResetAllInputs();
        LocalJsonDB.StartNewSession();

        if (clearPlayerPrefs) PlayerPrefs.DeleteAll();
        
        if (allPages != null)
            foreach (var p in allPages) if (p) p.SetActive(false);

        if (firstPage) firstPage.SetActive(true);

        BroadcastMessage("OnAppReset", SendMessageOptions.DontRequireReceiver);
    }

    public void ResetAllInputs()
    {
        var root = transform.root;
        
        foreach (var ifd in root.GetComponentsInChildren<TMP_InputField>(true))
        {
            ifd.text = "";
        }
        
        foreach (var s in root.GetComponentsInChildren<Slider>(true))
        {
            if (!s.interactable) continue;
            s.value = s.minValue;
        }
        
        foreach (var t in root.GetComponentsInChildren<Toggle>(true))
        {
            t.isOn = false;
        }

        foreach (var og in root.GetComponentsInChildren<OptionGroup>(true))
        {
            og.ClearSelection();
        }
    }
}
