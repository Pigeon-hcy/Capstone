using UnityEngine;

public class CylinderRotator : MonoBehaviour
{
    public Rigidbody2D playerRb;       // The player's Rigidbody
    public float rotationSpeed = 5f;   // Rotation multiplier
    public GameObject cylinder;        // Cylinder to rotate

    void Update()
{
    if (cylinder != null && playerRb != null)
    {
        // Get the movement vector of the player
        Vector3 movement = playerRb.linearVelocity; // Rigidbody2D uses .velocity

        // Project the movement onto the cylinder's local forward direction (Z axis)
        float speedAlongRoll = Vector3.Dot(movement, cylinder.transform.forward);

        // Calculate rotation amount
        float rotationAmount = speedAlongRoll * rotationSpeed * Time.deltaTime;

        // Rotate the cylinder around its local Y axis (inverted)
        cylinder.transform.Rotate(Vector3.up, -rotationAmount, Space.Self);
    }
}

}
