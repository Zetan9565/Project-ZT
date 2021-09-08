using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ColliderButton : MonoBehaviour, IPointerClickHandler
{
    public bool interactable;

    public Button.ButtonClickedEvent onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    private void OnValidate()
    {
        if (Camera.main)
        {
            if (Camera.main.GetComponent<Physics2DRaycaster>() == null && Camera.main.GetComponent<PhysicsRaycaster>() == null)
                Debug.LogWarning("相机没有可用的射线接收器");
        }
        else Debug.LogWarning("没有可用的相机");
        if (GetComponent<Collider>() == null && GetComponent<Collider2D>() == null)
            Debug.LogWarning("当前对象没有可用的碰撞器");
    }
}
