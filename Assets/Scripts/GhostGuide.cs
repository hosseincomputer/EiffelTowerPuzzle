using UnityEngine;

// Pulses joint visual indicators to guide kids where to connect pieces
public class GhostGuide : MonoBehaviour
{
    public enum GuideMode { AlwaysVisible, OnlyWhenHolding, Hidden }
    public GuideMode mode = GuideMode.AlwaysVisible;

    [Range(0.05f, 0.4f)]
    public float minScale = 0.15f;
    [Range(0.2f, 0.6f)]
    public float maxScale = 0.35f;
    public float pulseSpeed = 2f;

    private JointPoint[] allJoints;
    private PuzzlePiece heldPiece;

    void Start()
    {
        allJoints = FindObjectsByType<JointPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    }

    void Update()
    {
        float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
        float scale = Mathf.Lerp(minScale, maxScale, pulse);

        foreach (var jp in allJoints)
        {
            if (jp == null) continue;

            Transform visual = jp.transform.Find("JointVisual");
            if (visual == null) continue;

            bool show = ShouldShow(jp);
            visual.gameObject.SetActive(show);

            if (show)
            {
                visual.localScale = Vector3.one * scale;
                SetJointColor(visual, jp);
            }
        }
    }

    bool ShouldShow(JointPoint jp)
    {
        if (jp.isConnected) return false;
        if (mode == GuideMode.Hidden) return false;
        if (mode == GuideMode.AlwaysVisible) return true;

        // OnlyWhenHolding: show only joints compatible with currently held piece
        if (heldPiece == null) return false;
        foreach (var myJoint in heldPiece.jointPoints)
        {
            if (myJoint.compatibleType == jp.jointType) return true;
        }
        return false;
    }

    void SetJointColor(Transform visual, JointPoint jp)
    {
        var rend = visual.GetComponent<Renderer>();
        if (rend == null) return;

        // Color code by joint type category
        Color c = jp.jointType.StartsWith("base") ? new Color(0.2f, 0.8f, 0.2f) :
                  jp.jointType.StartsWith("leg")   ? new Color(1f, 0.85f, 0f) :
                  jp.jointType.StartsWith("plat")  ? new Color(0.2f, 0.6f, 1f) :
                  jp.jointType.StartsWith("mid")   ? new Color(1f, 0.4f, 0.1f) :
                                                     new Color(0.8f, 0.2f, 0.8f);
        rend.material.color = c;
        rend.material.SetColor("_EmissionColor", c * 1.5f);
        rend.material.EnableKeyword("_EMISSION");
    }

    public void SetHeldPiece(PuzzlePiece piece)
    {
        heldPiece = piece;
    }

    public void ClearHeldPiece()
    {
        heldPiece = null;
    }
}
