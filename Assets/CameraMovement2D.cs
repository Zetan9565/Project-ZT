using UnityEngine;

public class CameraMovement2D : SingletonMonoBehaviour<CameraMovement2D>
{
    [SerializeField]
    private CameraFollowing2D cameraFollowing;

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving;
    private float cameraMovingTime;
    public void MoveTo(Vector2 position)
    {
        isActing = true;
        targetPosition = new Vector3(position.x, position.y, cameraFollowing.camera.transform.position.z);
        isMoving = true;
        if (cameraFollowing.enabled) cameraFollowing.enabled = false;
    }
    private void Move()
    {
        if (isMoving)
        {
            cameraMovingTime += Time.deltaTime * 5;
            if (targetPosition != cameraFollowing.camera.transform.position)
                cameraFollowing.camera.transform.position = Vector3.Lerp(startPosition, targetPosition, cameraMovingTime);
            else
            {
                isMoving = false;
                cameraMovingTime = 0;
            }
        }
    }

    private bool isActing;
    public void Stop()
    {
        isActing = false;
        if (!cameraFollowing.enabled) cameraFollowing.enabled = true;
    }
}
