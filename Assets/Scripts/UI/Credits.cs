using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] GameObject creditsObj;
    bool isUIOpened = false;

    public void ShowCredits()
    {
        if (isUIOpened) return;

        isUIOpened = true;
        creditsObj.SetActive(true);
    }

    public void HideCredits()
    {
        if (!isUIOpened) return;

        isUIOpened = false;
        creditsObj.SetActive(false);
    }
}
