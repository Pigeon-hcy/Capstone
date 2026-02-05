using UnityEngine;
public class VineGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public VineRope grappleRope;


    [Header("Shoot Angle:")]
    [SerializeField] private float shootAngleDegrees = 0f;
    private void Start()
    {
        grappleRope.enabled = false;
    }

    public void FireGrabbingHook(float angle)
    {
        shootAngleDegrees = angle;
        Vector2 direction = GetShootDirectionFromAngle();
        grappleRope.StartExtending(direction);
    }

    private Vector2 GetShootDirectionFromAngle()
    {
        return (Quaternion.Euler(0f, 0f, shootAngleDegrees) * Vector2.right).normalized;
    }
}
