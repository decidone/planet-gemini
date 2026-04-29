using UnityEngine;
using UnityEngine.UI;

public class PingMarker : MonoBehaviour
{
    public Image iconImage;
    public Button closeButton;
    public int pingId;

    System.Action<int> onDismiss;

    public void Init(int id, Sprite icon, System.Action<int> dismissCallback)
    {
        pingId = id;
        onDismiss = dismissCallback;

        if (icon != null && iconImage != null)
            iconImage.sprite = icon;

        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                SoundManager.instance.PlayUISFX("PingRemove");
                onDismiss?.Invoke(pingId);
                onDismiss = null;
            });
    }
}
