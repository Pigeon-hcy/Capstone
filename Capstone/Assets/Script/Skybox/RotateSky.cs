using UnityEngine;

public class RotateSky : MonoBehaviour
{
    public GameObject player;
    public float speed;
    private float currentRot;

   
    void Update()
    {
        //Grab a reference for the player's Rigisbody linearVelocityX
        float playerSpeed = player.GetComponent<Rigidbody2D>().linearVelocityX;
        currentRot += Time.deltaTime * playerSpeed * speed;

        RenderSettings.skybox.SetFloat("_Rotation", currentRot);
        
    }
}
