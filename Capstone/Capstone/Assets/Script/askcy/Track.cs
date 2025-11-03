using UnityEngine;

public class Track : MonoBehaviour
{
    LogicalRail rail;

    void Start()
    {
        rail = GetComponentInChildren<LogicalRail>();
    }

    public Vector3 GetTrackPosition()
    {
        return transform.position;
    }

    public Vector3 GetTrackDirection()
    {
        return transform.right;
    }

    public TrackDirComputeTool GetDirTool()
    {
        return new TrackDirComputeTool(rail, transform.rotation.eulerAngles.z);

    }
}
