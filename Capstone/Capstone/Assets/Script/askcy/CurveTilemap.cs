using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class CurveTilemap : MonoBehaviour
{
    [Header("Tilemap 设置")]
    public Tilemap tilemap;
    
    [Header("碰撞体设置")]
    [Tooltip("生成的碰撞体父物体")]
    public Transform colliderParent;
    
    [Tooltip("是否在运行时自动生成")]
    public bool generateOnStart = true;
    
    [Tooltip("碰撞体层级")]
    public string colliderLayer = "Default";
    
    [Tooltip("碰撞体所使用的物理材质")]
    public PhysicsMaterial2D physicsMaterial;
    
    [Header("调试选项")]
    [Tooltip("是否在Scene视图中显示碰撞体轮廓")]
    public bool showGizmos = true;
    
    private List<GameObject> generatedColliders = new List<GameObject>();

    void Start()
    {
        if (generateOnStart)
        {
            GenerateColliders();
        }
    }

    /// <summary>
    /// 生成所有 tile 的碰撞体
    /// </summary>
    [ContextMenu("生成碰撞体")]
    public void GenerateColliders()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap 未设置！");
            return;
        }

        // 清理之前生成的碰撞体
        ClearColliders();

        // 确保有父物体
        if (colliderParent == null)
        {
            GameObject parentObj = new GameObject("TilemapColliders");
            parentObj.transform.SetParent(transform);
            colliderParent = parentObj.transform;
        }

        // 获取 tilemap 的边界
        BoundsInt bounds = tilemap.cellBounds;
        int tileCount = 0;

        // 遍历所有 tile
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            
            if (tile != null)
            {
                // 为该 tile 生成碰撞体
                if (CreateColliderForTile(pos, tile))
                {
                    tileCount++;
                }
            }
        }

        Debug.Log($"成功为 {tileCount} 个 Tile 生成了碰撞体");
    }

    /// <summary>
    /// 为单个 tile 创建碰撞体
    /// </summary>
    private bool CreateColliderForTile(Vector3Int tilePos, TileBase tile)
    {
        // 获取 tile 的 sprite
        Sprite sprite = tilemap.GetSprite(tilePos);
        
        if (sprite == null)
        {
            return false;
        }

        // 获取 sprite 的物理形状
        List<Vector2[]> physicsShapes = new List<Vector2[]>();
        int shapeCount = sprite.GetPhysicsShapeCount();
        
        if (shapeCount == 0)
        {
            // 如果没有定义物理形状，使用默认的矩形
            Debug.LogWarning($"Tile 在位置 {tilePos} 没有定义物理形状，将使用默认矩形");
            return CreateDefaultRectangleCollider(tilePos);
        }

        // 获取所有物理形状
        for (int i = 0; i < shapeCount; i++)
        {
            List<Vector2> shape = new List<Vector2>();
            sprite.GetPhysicsShape(i, shape);
            
            if (shape.Count >= 3) // 至少需要3个点才能形成多边形
            {
                physicsShapes.Add(shape.ToArray());
            }
        }

        if (physicsShapes.Count == 0)
        {
            return false;
        }

        // 创建碰撞体 GameObject
        GameObject colliderObj = new GameObject($"Tile_Collider_{tilePos.x}_{tilePos.y}");
        colliderObj.transform.SetParent(colliderParent);
        colliderObj.layer = LayerMask.NameToLayer(colliderLayer);
        
        // 获取 tile 在世界空间的位置
        Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);
        colliderObj.transform.position = worldPos;

        // 获取 tile 的变换信息（旋转、翻转等）
        Matrix4x4 tileTransform = tilemap.GetTransformMatrix(tilePos);
        
        // 为每个物理形状创建一个 PolygonCollider2D
        foreach (Vector2[] shape in physicsShapes)
        {
            PolygonCollider2D polyCollider = colliderObj.AddComponent<PolygonCollider2D>();
            
            // 转换坐标点
            Vector2[] transformedPoints = TransformShapePoints(shape, tileTransform, sprite);
            
            // 设置碰撞体的点
            polyCollider.points = transformedPoints;
            
            // 应用物理材质
            if (physicsMaterial != null)
            {
                polyCollider.sharedMaterial = physicsMaterial;
            }
        }

        generatedColliders.Add(colliderObj);
        return true;
    }

    /// <summary>
    /// 为没有物理形状的 tile 创建默认矩形碰撞体
    /// </summary>
    private bool CreateDefaultRectangleCollider(Vector3Int tilePos)
    {
        GameObject colliderObj = new GameObject($"Tile_Collider_{tilePos.x}_{tilePos.y}");
        colliderObj.transform.SetParent(colliderParent);
        colliderObj.layer = LayerMask.NameToLayer(colliderLayer);
        
        Vector3 worldPos = tilemap.GetCellCenterWorld(tilePos);
        colliderObj.transform.position = worldPos;

        BoxCollider2D boxCollider = colliderObj.AddComponent<BoxCollider2D>();
        boxCollider.size = tilemap.cellSize;
        
        if (physicsMaterial != null)
        {
            boxCollider.sharedMaterial = physicsMaterial;
        }

        generatedColliders.Add(colliderObj);
        return true;
    }

    /// <summary>
    /// 转换形状的点以匹配 tile 的变换（旋转、缩放、翻转等）
    /// </summary>
    private Vector2[] TransformShapePoints(Vector2[] originalPoints, Matrix4x4 tileTransform, Sprite sprite)
    {
        Vector2[] transformedPoints = new Vector2[originalPoints.Length];
        
        // 获取 sprite 的像素大小和单位转换
        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;
        Rect rect = sprite.rect;
        
        for (int i = 0; i < originalPoints.Length; i++)
        {
            // 原始点是相对于 sprite 中心的
            Vector2 point = originalPoints[i];
            
            // 应用 tile 的变换矩阵
            Vector3 transformedPoint = tileTransform.MultiplyPoint3x4(point);
            
            transformedPoints[i] = new Vector2(transformedPoint.x, transformedPoint.y);
        }
        
        return transformedPoints;
    }

    /// <summary>
    /// 清除所有生成的碰撞体
    /// </summary>
    [ContextMenu("清除碰撞体")]
    public void ClearColliders()
    {
        foreach (GameObject obj in generatedColliders)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }
        generatedColliders.Clear();
        
        Debug.Log("已清除所有生成的碰撞体");
    }

    /// <summary>
    /// 在Scene视图中绘制碰撞体轮廓
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos || generatedColliders.Count == 0)
            return;

        Gizmos.color = Color.green;
        
        foreach (GameObject obj in generatedColliders)
        {
            if (obj == null) continue;
            
            PolygonCollider2D[] colliders = obj.GetComponents<PolygonCollider2D>();
            foreach (PolygonCollider2D collider in colliders)
            {
                DrawPolygonColliderGizmo(collider);
            }
        }
    }

    /// <summary>
    /// 绘制单个多边形碰撞体的轮廓
    /// </summary>
    private void DrawPolygonColliderGizmo(PolygonCollider2D collider)
    {
        if (collider == null) return;
        
        Vector2[] points = collider.points;
        Vector3 position = collider.transform.position;
        
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 start = position + (Vector3)points[i];
            Vector3 end = position + (Vector3)points[(i + 1) % points.Length];
            Gizmos.DrawLine(start, end);
        }
    }
}
