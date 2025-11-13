using UnityEngine;

public class ResultPanelActive : MonoBehaviour
{
    public GameObject resultPage;
    public void ResultOpen()
    {
        resultPage.SetActive(true);
    }

    public void ResultClose()
    {
        resultPage.SetActive(false);
    }
}
