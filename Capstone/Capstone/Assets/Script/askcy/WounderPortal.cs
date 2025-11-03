using UnityEngine;

public class WounderPortal : MonoBehaviour
{
    public float targetZ;
    public bool reCover = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, targetZ);
            
            SpriteRenderer spriteRenderer = other.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (!reCover)
                {
                    spriteRenderer.color = Color.black;
                }
                else
                {
                    spriteRenderer.color = Color.white;
                }
            }
        
        }
    }

}
