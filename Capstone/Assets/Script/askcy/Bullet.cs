using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float xSpeed = 10f;
    public float ySpeed = 0f;

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
            transform.Translate(Vector2.right * xSpeed * Time.deltaTime, Space.World);
            transform.Translate(Vector2.up * ySpeed * Time.deltaTime, Space.World);
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
