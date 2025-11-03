using UnityEngine;
using MoreMountains.Feedbacks;

public class Lifetime : MonoBehaviour
{
    public float lifetime = 10f;
    public MMF_Player OnSpawnEffect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (OnSpawnEffect != null)
        {
            OnSpawnEffect.PlayFeedbacks();
        }
        if (lifetime > 0)
        {
            Invoke("DestroyObject", lifetime);
        }
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
