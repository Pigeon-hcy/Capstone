using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;

    public Vector3 startPoint;
    public bool isActive = false;

    void Start()
    {
        startPoint = transform.position;
    }

    void Update()
    {
        if (isActive)
        {
            transform.Translate(Vector2.up * speed * Time.deltaTime);
        }
    }

    public void launch()
    {
        transform.position = startPoint;
        isActive = true;
    }

    public void reset()
    {
        isActive = false;
        transform.position = startPoint;
    }


}
