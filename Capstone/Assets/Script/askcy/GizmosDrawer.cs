using UnityEngine;

[ExecuteAlways]

public class GizmosDrawer : MonoBehaviour
{
    private CompositeCollider2D composite;

    private void OnDrawGizmos()
    {
        if (composite == null) composite = GetComponent<CompositeCollider2D>();
        if (composite == null) return;

        Gizmos.color = new Color(1, 0, 0, 1f); // 半透明红

        for (int i = 0; i < composite.pathCount; i++)
        {
            Vector2[] points = new Vector2[composite.GetPathPointCount(i)];
            composite.GetPath(i, points);
            for (int j = 0; j < points.Length; j++)
            {
                Vector2 a = points[j];
                Vector2 b = points[(j + 1) % points.Length];
                Gizmos.DrawLine(a + (Vector2)transform.position, b + (Vector2)transform.position);
            }
        }
    }
}
