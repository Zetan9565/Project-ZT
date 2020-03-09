using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowing2D : SingletonMonoBehaviour<CameraFollowing2D>
{
    public new Camera camera;

    public Transform target;

    public Vector2 offset;

    public bool smooth = true;

#if UNITY_EDITOR
    [ConditionalHide("smooth", true)]
#endif
    public float smoothness = 0.25f;

    public UpdateMode updateMode = UpdateMode.LateUpdate;

    private void Awake()
    {
        if (camera) camera = GetComponent<Camera>();
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
        if (target && camera)
        {
            if (smooth) camera.transform.position = Vector3.Lerp(camera.transform.position, (Vector3)offset + new Vector3(target.position.x, target.position.y, camera.transform.position.z), smoothness);
            else camera.transform.position = (Vector3)offset + new Vector3(target.position.x, target.position.y, camera.transform.position.z);
        }
    }
}
