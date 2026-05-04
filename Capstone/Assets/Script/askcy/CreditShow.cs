using UnityEngine;
using MoreMountains.Feedbacks;

public class CreditShow : MonoBehaviour
{
    public MMF_Player creditShow;

    public string NameDisplay;
    public string titleDisplay;

    [SerializeField]
    Color triggerGizmoColor = new Color(0.2f, 0.85f, 1f, 0.9f);

    void OnDrawGizmos()
    {
        Collider2D[] cols = GetComponents<Collider2D>();
        if (cols == null || cols.Length == 0)
        {
            return;
        }

        Color prev = Gizmos.color;
        Gizmos.color = triggerGizmoColor;

        foreach (Collider2D col in cols)
        {
            if (col == null || !col.enabled || !col.isTrigger)
            {
                continue;
            }

            switch (col)
            {
                case BoxCollider2D box:
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawWireCube((Vector3)box.offset, box.size);
                    break;
                case CircleCollider2D circle:
                    Vector3 worldCenter = transform.TransformPoint(circle.offset);
                    float r = circle.radius * Mathf.Max(
                        Mathf.Abs(transform.lossyScale.x),
                        Mathf.Abs(transform.lossyScale.y));
                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.DrawWireSphere(worldCenter, r);
                    break;
                default:
                    Gizmos.matrix = Matrix4x4.identity;
                    Bounds b = col.bounds;
                    Gizmos.DrawWireCube(b.center, b.size);
                    break;
            }
        }

        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = prev;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || creditShow == null || creditShow.FeedbacksList == null)
        {
            return;
        }

        int revealIndex = 0;
        foreach (MMF_Feedback fb in creditShow.FeedbacksList)
        {
            if (fb is not MMF_TMPTextReveal reveal || !fb.Active)
            {
                continue;
            }

            if (revealIndex == 0)
            {
                reveal.ReplaceText = true;
                reveal.NewText = NameDisplay ?? string.Empty;
            }
            else if (revealIndex == 1)
            {
                reveal.ReplaceText = true;
                reveal.NewText = titleDisplay ?? string.Empty;
            }
            else
            {
                break;
            }

            revealIndex++;
        }

        creditShow.PlayFeedbacks();
    }
}
