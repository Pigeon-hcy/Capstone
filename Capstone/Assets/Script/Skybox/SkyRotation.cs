using UnityEngine;

public class SkyRotation : MonoBehaviour
{
    public Rigidbody2D targetRb;
    public float rotationSpeed;
    private float currentRotation;
    private Material skybox;
    void Start()
    {
        skybox = RenderSettings.skybox;

        if(targetRb != null)
        {
            targetRb = targetRb.GetComponent<Rigidbody2D>();
        }
    }
    void Update()
    {
        if(skybox != null && targetRb != null)
        {
            float playerSpeed = targetRb.linearVelocityX;
            float rotationForce = playerSpeed * rotationSpeed * Time.deltaTime;

            currentRotation += rotationForce;

            skybox.SetFloat("_Rotation", currentRotation);
        }
        
    }
}
