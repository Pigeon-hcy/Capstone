using UnityEngine;

public class Z_CM_Lock : MonoBehaviour
{
    [Header("Z轴锁定（用于Cinemachine Brain输出相机）")]
    public bool lockZAxis = true;
    public float lockedZ = -10f;

    void LateUpdate()
    {
        if (!lockZAxis) return;
        Transform camTransform = transform;
        camTransform.position = new Vector3(camTransform.position.x, camTransform.position.y, lockedZ);
    }
}
