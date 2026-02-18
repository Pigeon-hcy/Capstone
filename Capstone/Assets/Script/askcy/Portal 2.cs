using UnityEngine;
using MoreMountains.Feedbacks;

public class Portal : MonoBehaviour
{

    public Transform targetPortal;
    public Portal otherPortal;

    public float protalCoolDown = 3;
    private float protalCoolDownTimer = 0;
    private bool isCoolingDown = false;

    public bool coverPlayerAngle = false;

    public GameObject particleHolder;
    public float travelTime = 0.1f;
    public bool isTraveling = false;
    public MMF_Player EndTravelEffect;

    private float travelProgress = 0;

    

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
                isTraveling = true;

            }
            else
            {
                float exitAngleDegrees = targetPortal.transform.eulerAngles.z;
                Vector2 direction = (Quaternion.Euler(0f, 0f, exitAngleDegrees) * Vector2.up).normalized;
                playerRb.linearVelocity = direction * playerSpeed;
                other.transform.position = targetPosition;
                isCoolingDown = true;
                otherPortal.isCoolingDown = true;
                isTraveling = true;
            }

        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(particleHolder != null)
        {
            particleHolder.GetComponent<ParticleSystem>().Stop();
        }
        
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

        if(particleHolder != null)
        {
            if(isTraveling)
            {
                
                particleHolder.GetComponent<ParticleSystem>().Play();
                travelProgress += Time.deltaTime / travelTime;
                particleHolder.transform.position = Vector3.Lerp(this.transform.position, targetPortal.position, travelProgress);
                if(travelProgress >= 1)
                {
                    isTraveling = false;
                    travelProgress = 0;
                }
            }
            else
            {
                particleHolder.GetComponent<ParticleSystem>().Stop();
            }
        }
    }
}
