using UnityEngine;

public class JointPoint : MonoBehaviour
{
    public string jointType;        // e.g. "leg_top", "platform_bottom"
    public string compatibleType;   // what this can connect to
    public float snapRadius = 0.3f;
    public bool isConnected = false;

    public Quaternion localRotation => transform.localRotation;

    public JointPoint FindNearestCompatible()
    {
        JointPoint[] all = FindObjectsByType<JointPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        JointPoint best = null;
        float bestDist = snapRadius;

        foreach (var other in all)
        {
            if (other == this) continue;
            if (other.isConnected || isConnected) continue;
            if (other.jointType != compatibleType) continue;
            if (other.GetComponentInParent<PuzzlePiece>() == GetComponentInParent<PuzzlePiece>()) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = other;
            }
        }
        return best;
    }

    public void Connect(JointPoint other)
    {
        isConnected = true;
        other.isConnected = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isConnected ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapRadius);
    }
}
