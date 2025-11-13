using UnityEngine;
using UnityEngine.UI;

public class PageFlow : MonoBehaviour
{
    public Button nextButton;      
    public GameObject currentPage;
    public GameObject nextPage;     

    void Awake()
    {
        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(GoNext);
        }
    }

    public void GoNext()
    {
        if (!nextPage) return;
        nextPage.SetActive(true);
        gameObject.SetActive(false);
        currentPage.SetActive(false);
    }

    public void GoPrev()
    {
        if (!currentPage) return;
        currentPage.SetActive(true);
        gameObject.SetActive(false);
    }
}