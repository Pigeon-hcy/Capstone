using UnityEngine;

public class Fans : MonoBehaviour
{
    public Transform transform;
    public float XRotationSpeed = 0f;
    public float YRotationSpeed = 0f;
    public float ZRotationSpeed = 0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(XRotationSpeed, YRotationSpeed, ZRotationSpeed) * Time.deltaTime);
    }
}
