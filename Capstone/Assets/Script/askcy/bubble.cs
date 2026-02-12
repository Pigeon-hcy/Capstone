using UnityEngine;

public class bubble : MonoBehaviour
{
    public float speed = 10f;
    public Transform startPoint;
    public Transform endPoint;

    void FixedUpdate()
    {
        //move the bubble to the end point
        //if bubble is at the end point, reset the bubble to the start point
        transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.fixedDeltaTime);
        if (transform.position == endPoint.position)
        {
            transform.position = startPoint.position;
        }
    }
}
