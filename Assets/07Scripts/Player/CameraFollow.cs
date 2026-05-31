using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform player; // Drag your player GameObject here
    [SerializeField] Vector3 offset; // Camera position relative to player
    [SerializeField] float smoothSpeed = 0.125f;

    void Start()
    {
        // If player not assigned, try to find it automatically
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Set initial offset based on current positions
        offset = transform.position - player.position;
    }

    void LateUpdate() // Use LateUpdate for camera movement (runs after Update)
    {
        if (player != null)
        {
            // Calculate target position
            Vector3 targetPosition = player.position + offset;

            // Smoothly move camera towards target position
            Vector3 smoothedPosition = Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed
            );

            // Apply the position (keep camera's own forward movement)
            transform.position = smoothedPosition;
        }
    }
}