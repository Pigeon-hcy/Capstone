using UnityEngine;

public class Portal : MonoBehaviour
{

    public Transform targetPortal;
    public Portal otherPortal;

    public float protalCoolDown = 3;
    private float protalCoolDownTimer = 0;
    private bool isCoolingDown = false;

    public bool coverPlayerAngle = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && !isCoolingDown)
        {
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            float playerSpeed = playerRb.linearVelocity.magnitude;
            Vector3 targetPosition = targetPortal.position;

            if (!coverPlayerAngle)
            {
                other.transform.position = targetPosition;
                isCoolingDown = true;
                otherPortal.isCoolingDown = true;
            }
            else
            {
                float exitAngleDegrees = targetPortal.transform.eulerAngles.z;
                Vector2 direction = (Quaternion.Euler(0f, 0f, exitAngleDegrees) * Vector2.up).normalized;
                playerRb.linearVelocity = direction * playerSpeed;
                other.transform.position = targetPosition;
                isCoolingDown = true;
                otherPortal.isCoolingDown = true;
            }

        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isCoolingDown)
        {
            protalCoolDownTimer += Time.deltaTime;
            if (protalCoolDownTimer >= protalCoolDown)
            {
                isCoolingDown = false;
                protalCoolDownTimer = 0;
            }
        }
    }
}
