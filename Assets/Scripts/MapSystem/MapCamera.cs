using UnityEngine;

[RequireComponent(typeof(Camera)), DisallowMultipleComponent]
public class MapCamera : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [ReadOnly]
#endif
    private new Camera camera;

    public Camera Camera => camera;

    private void OnValidate()
    {
        camera = GetComponent<Camera>();
        camera.hideFlags = HideFlags.NotEditable;
        camera.orthographic = true;
    }
}
