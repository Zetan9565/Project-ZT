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
        startPosition = cameraFollowing.CameraTransform.position;
        targetPosition = new Vector3(position.x, position.y, cameraFollowing.CameraTransform.position.z);
        isMoving = true;
        if (cameraFollowing.enabled) cameraFollowing.enabled = false;
    }
    private void Move()
    {
        if (isMoving)
        {
            cameraMovingTime += Time.deltaTime * 5;
            if (targetPosition != cameraFollowing.CameraTransform.position)
                cameraFollowing.CameraTransform.position = Vector3.Lerp(startPosition, targetPosition, cameraMovingTime);
            else
            {
                isMoving = false;
                cameraMovingTime = 0;
            }
        }
    }

    public void Stop()
    {
        if (!cameraFollowing.enabled) cameraFollowing.enabled = true;
    }
}
