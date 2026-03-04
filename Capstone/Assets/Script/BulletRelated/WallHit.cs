using UnityEngine;

public class WallHit : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Wall Hit");
        if ((groundLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
