using UnityEngine;

public class VineRope : MonoBehaviour
{
    [Header("General Refernces:")]
    public VineGun grapplingGun;
    public LineRenderer m_lineRenderer;

    [Header("General Settings:")]
    [SerializeField] private int percision = 40;
    [Range(0, 20)] [SerializeField] private float straightenLineSpeed = 5;

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)] [SerializeField] private float StartWaveSize = 2;
    float waveSize = 0;

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [SerializeField] [Range(1, 50)] private float ropeProgressionSpeed = 1;

    [Header("Fail Animation:")]
    [SerializeField] private float failExtendSpeed = 25f;
    [SerializeField] private float failRetractSpeed = 35f;

    float moveTime = 0;

    [HideInInspector] public bool isGrappling = true;

    bool strightLine = true;
    private bool isFailing = false;
    private bool isFailRetracting = false;
    private float failCurrentDistance = 0f;
    private float failMaxDistance = 0f;
    private Vector2 failDirection = Vector2.right;

    private void OnEnable()
    {
        moveTime = 0;
        if (isFailing)
        {
            SetupFailLine();
            return;
        }

        m_lineRenderer.positionCount = percision;
        waveSize = StartWaveSize;
        strightLine = false;

        LinePointsToFirePoint();

        m_lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        m_lineRenderer.enabled = false;
        isGrappling = false;
        isFailing = false;
        isFailRetracting = false;
    }

    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < percision; i++)
        {
            m_lineRenderer.SetPosition(i, grapplingGun.transform.position);
        }
    }

    private void Update()
    {
        if (isFailing)
        {
            UpdateFailRope();
            return;
        }

        moveTime += Time.deltaTime;
        DrawRope();
    }

    void DrawRope()
    {
        if (!strightLine)
        {
            if (m_lineRenderer.GetPosition(percision - 1).x == grapplingGun.grapplePoint.x)
            {
                strightLine = true;
            }
            else
            {
                DrawRopeWaves();
            }
        }
        else
        {
            if (!isGrappling)
            {
                grapplingGun.Grapple();
                isGrappling = true;
            }
            if (waveSize > 0)
            {
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                waveSize = 0;

                if (m_lineRenderer.positionCount != 2) { m_lineRenderer.positionCount = 2; }

                DrawRopeNoWaves();
            }
        }
    }

    void DrawRopeWaves()
    {
        for (int i = 0; i < percision; i++)
        {
            float delta = (float)i / ((float)percision - 1f);
            Vector2 offset = Vector2.Perpendicular(grapplingGun.grappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            Vector2 targetPosition = Vector2.Lerp(grapplingGun.transform.position, grapplingGun.grapplePoint, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(grapplingGun.transform.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            m_lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves()
    {
        m_lineRenderer.SetPosition(0, grapplingGun.transform.position);
        m_lineRenderer.SetPosition(1, grapplingGun.grapplePoint);
    }

    public void PlayFailedShot(Vector2 direction, float maxDistance)
    {
        if (direction.sqrMagnitude <= 0.0001f || m_lineRenderer == null || grapplingGun == null)
        {
            return;
        }

        isFailing = true;
        isFailRetracting = false;
        failCurrentDistance = 0f;
        failMaxDistance = Mathf.Max(0f, maxDistance);
        failDirection = direction.normalized;

        SetupFailLine();
        enabled = true;
    }

    private void SetupFailLine()
    {
        if (m_lineRenderer == null || grapplingGun == null)
        {
            return;
        }

        m_lineRenderer.positionCount = percision;
        m_lineRenderer.enabled = true;
        DrawFailWaves();
    }

    private void UpdateFailRope()
    {
        if (failMaxDistance <= 0f)
        {
            EndFailRope();
            return;
        }

        float speed = isFailRetracting ? failRetractSpeed : failExtendSpeed;
        float delta = speed * Time.deltaTime;
        failCurrentDistance += isFailRetracting ? -delta : delta;

        if (!isFailRetracting && failCurrentDistance >= failMaxDistance)
        {
            failCurrentDistance = failMaxDistance;
            isFailRetracting = true;
        }
        else if (isFailRetracting && failCurrentDistance <= 0f)
        {
            failCurrentDistance = 0f;
            EndFailRope();
            return;
        }

        DrawFailWaves();
    }

    private void DrawFailWaves()
    {
        Vector2 start = grapplingGun.transform.position;
        Vector2 end = start + failDirection * failCurrentDistance;
        Vector2 travel = end - start;
        Vector2 perpendicular = Vector2.Perpendicular(failDirection).normalized;

        for (int i = 0; i < percision; i++)
        {
            float delta = (float)i / ((float)percision - 1f);
            Vector2 offset = perpendicular * ropeAnimationCurve.Evaluate(delta) * StartWaveSize;
            Vector2 targetPosition = start + travel * delta + offset;
            m_lineRenderer.SetPosition(i, targetPosition);
        }
    }

    private void EndFailRope()
    {
        isFailing = false;
        isFailRetracting = false;
        m_lineRenderer.enabled = false;
        enabled = false;
    }
}
