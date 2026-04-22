using UnityEngine;

public class JointPoint : MonoBehaviour
{
    public string jointType;
    public string compatibleType;
    public float snapRadius = 1.5f;
    public bool isConnected = false;

    public Color OriginalColor { get; private set; }
    private Renderer visRend;

    void Start()
    {
        var vis = transform.Find("JointVisual");
        if (vis != null)
        {
            visRend = vis.GetComponent<Renderer>();
            if (visRend != null) OriginalColor = visRend.material.color;
        }
    }

    // Called by PuzzlePiece with its own magnet radius
    public JointPoint FindNearest(float radius)
    {
        JointPoint[] all = FindObjectsByType<JointPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        JointPoint best = null;
        float bestDist  = radius;

        foreach (var other in all)
        {
            if (other == this) continue;
            if (other.isConnected || isConnected) continue;
            if (other.jointType != compatibleType) continue;
            if (other.GetComponentInParent<PuzzlePiece>() == GetComponentInParent<PuzzlePiece>()) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < bestDist) { bestDist = dist; best = other; }
        }
        return best;
    }

    // Legacy — kept so existing code doesn't break
    public JointPoint FindNearestCompatible() => FindNearest(snapRadius);

    public JointPoint connectedTo; // tracks the paired joint so both can be reset on detach

    public void Connect(JointPoint other)
    {
        isConnected       = true;
        connectedTo       = other;
        other.isConnected = true;
        other.connectedTo = this;
    }

    public void Disconnect()
    {
        if (connectedTo != null)
        {
            connectedTo.isConnected = false;
            connectedTo.connectedTo = null;
            RestoreVisual(connectedTo);
        }
        isConnected = false;
        connectedTo = null;
        RestoreVisual(this);
    }

    static void RestoreVisual(JointPoint jp)
    {
        var vis = jp.transform.Find("JointVisual");
        if (vis == null) return;
        vis.gameObject.SetActive(true);
        var rend = vis.GetComponent<Renderer>();
        if (rend != null) rend.material.color = jp.OriginalColor;
        var mark = jp.transform.Find("ConnectedMark");
        if (mark != null) Object.Destroy(mark.gameObject);
    }

    public void SetHighlight(bool on)
    {
        if (visRend == null) return;
        visRend.material.color = on ? Color.cyan : OriginalColor;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isConnected ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, snapRadius);
    }
}
