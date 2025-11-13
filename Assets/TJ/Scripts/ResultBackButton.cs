using UnityEngine;
using UnityEngine.UI;

public class ResultBackButton : MonoBehaviour
{
    public Button button;
    public GameObject prevPage;
    public GameObject thisPage;

    void Awake()
    {
        if (!thisPage) thisPage = gameObject;
        if (!button) button = GetComponent<Button>();
        if (button)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (prevPage) prevPage.SetActive(true);
                thisPage.SetActive(false);
            });
        }
    }
}