using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowing2D : MonoBehaviour
{
    public new Camera camera;

    [HideInInspector]
    public Transform CameraTransform { get; private set; }

    public Transform target;

    public Vector2 offset;

    public bool smooth = true;

#if UNITY_EDITOR
    [HideIf("smooth", false)]
#endif
    public float smoothness = 0.25f;

    public UpdateMode updateMode = UpdateMode.LateUpdate;

    private void Awake()
    {
        if (!camera) camera = GetComponent<Camera>();
        CameraTransform = camera.transform;
    }

    private void Update()
    {
        if (updateMode == UpdateMode.Update) Follow();
    }

    private void FixedUpdate()
    {
        if (updateMode == UpdateMode.FixedUpdate) Follow();
    }

    void LateUpdate()
    {
        if (updateMode == UpdateMode.LateUpdate) Follow();
    }

    public void SetTarget(Transform target, Vector2 offset)
    {
        this.target = target;
        this.offset = offset;
    }

    void Follow()
    {
        if (target && CameraTransform)
        {
            if (smooth) CameraTransform.position = Vector3.Lerp(CameraTransform.position, (Vector3)offset + new Vector3(target.position.x, target.position.y, CameraTransform.position.z), smoothness);
            else CameraTransform.position = (Vector3)offset + new Vector3(target.position.x, target.position.y, CameraTransform.position.z);
        }
    }
}
