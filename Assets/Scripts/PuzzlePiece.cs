using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzlePiece : MonoBehaviour
{
    public enum PieceCategory { Base, Lower, Middle, Top }

    public string pieceID;
    public PieceCategory category;
    public JointPoint[] jointPoints;
    public bool isPlaced = false;
    public bool isFixed  = false; // true = permanently fixed, cannot be detached

    [Header("Magnet Settings")]
    public float magnetRadius = 1.5f;  // each cube pair must be within this distance
    public float snapRadius   = 0.5f;  // locks when best pair is this close

    private bool isDragging = false;
    private Vector3 dragOffset;
    private float dragDepth;
    private Camera mainCam;
    private GhostGuide guide;
    private Vector3 startPosition;

    // Current best snap target while dragging
    private JointPoint attractedMy;
    private JointPoint attractedTarget;

    void Start()
    {
        mainCam       = Camera.main;
        guide         = FindAnyObjectByType<GhostGuide>();
        startPosition = transform.position;
        if (jointPoints == null || jointPoints.Length == 0)
            jointPoints = GetComponentsInChildren<JointPoint>();
    }

    void OnMouseDown()
    {
        if (isPlaced || isFixed) return;
        isDragging = true;
        dragDepth  = mainCam.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - ScreenToWorld(Mouse.current.position.ReadValue());
        guide?.SetHeldPiece(this);
    }

    void Update()
    {
        if (isFixed) return;
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            foreach (var jp in jointPoints)
                if (jp.isConnected) jp.Disconnect();
            isPlaced           = false;
            isDragging         = false;
            transform.position = startPosition;
            guide?.ClearHeldPiece();
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        // Follow mouse
        Vector3 mousePos = ScreenToWorld(Mouse.current.position.ReadValue()) + dragOffset;
        transform.position = mousePos;

        // Find nearest compatible joint pair
        FindBestAttraction();

        if (attractedMy != null && attractedTarget != null)
        {
            Vector3 jOffset    = attractedMy.transform.position - transform.position;
            Vector3 snapPos    = attractedTarget.transform.position - jOffset;
            float   distToSnap = Vector3.Distance(transform.position, snapPos);

            if (distToSnap < snapRadius)
            {
                // Lock in — snap and connect
                DoSnap(attractedMy, attractedTarget);
            }
            else if (distToSnap < magnetRadius)
            {
                // Magnetic pull — smoothly slide toward snap position
                float pull = 1f - (distToSnap / magnetRadius);
                transform.position = Vector3.Lerp(mousePos, snapPos, pull * pull);

                // Highlight joints to show attraction
                attractedMy.SetHighlight(true);
                attractedTarget.SetHighlight(true);
            }
            else
            {
                ClearHighlights();
            }
        }
        else
        {
            ClearHighlights();
        }
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        ClearHighlights();
        guide?.ClearHeldPiece();

        // Final snap check on release
        FindBestAttraction();
        if (attractedMy != null && attractedTarget != null)
        {
            float dist = Vector3.Distance(attractedMy.transform.position,
                                          attractedTarget.transform.position);
            if (dist < magnetRadius)
                DoSnap(attractedMy, attractedTarget);
        }
    }

    void FindBestAttraction()
    {
        attractedMy     = null;
        attractedTarget = null;

        int   totalPairs = 0;
        int   closePairs = 0;
        float bestDist   = float.MaxValue;
        JointPoint bestMy = null, bestTarget = null;

        foreach (var jp in jointPoints)
        {
            JointPoint nearest = jp.FindNearest(999f);
            if (nearest == null) continue;   // this joint has no compatible partner — skip

            totalPairs++;

            // Distance between THIS cube and ITS matching cube
            float d = Vector3.Distance(jp.transform.position, nearest.transform.position);

            if (d < magnetRadius)
            {
                closePairs++;
                if (d < bestDist) { bestDist = d; bestMy = jp; bestTarget = nearest; }
            }
        }

        // ALL four pairs must be close at the same time — not just one or two
        if (totalPairs > 0 && closePairs == totalPairs)
        {
            attractedMy     = bestMy;
            attractedTarget = bestTarget;
        }
    }

    void DoSnap(JointPoint myJoint, JointPoint targetJoint)
    {
        Vector3 offset = myJoint.transform.position - transform.position;
        // Small gap so pieces look separate but clearly connected
        transform.position = targetJoint.transform.position - offset + Vector3.up * 0.08f;
        isPlaced   = true;
        isDragging = false;
        myJoint.Connect(targetJoint);
        // Show all connected joints on this piece with bright color
        foreach (var jp in jointPoints)
            if (jp.isConnected) ShowConnected(jp);
        ShowConnected(targetJoint);
        guide?.ClearHeldPiece();
    }

    void FindBestAttractionAndSnap()
    {
        FindBestAttraction();
        if (attractedMy != null)
            DoSnap(attractedMy, attractedTarget);
    }

    void ShowConnected(JointPoint jp)
    {
        var vis = jp.transform.Find("JointVisual");
        if (vis == null) return;

        // Bright lime green = connected and locked
        Color connectedColor = new Color(0.2f, 1f, 0.3f);
        var rend = vis.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = connectedColor;
            rend.material.SetFloat("_Smoothness", 0.9f);
        }

        // Scale up slightly so it's obvious
        vis.localScale = Vector3.one * 0.65f;
    }

    void Detach()
    {
        if (!isPlaced || isFixed) return;

        // Disconnect every joint — resets both this piece AND the partner joints on the other piece
        foreach (var jp in jointPoints)
            if (jp.isConnected) jp.Disconnect();

        isPlaced   = false;
        isDragging = true;
        dragDepth  = mainCam.WorldToScreenPoint(transform.position).z;
        dragOffset = transform.position - ScreenToWorld(Mouse.current.position.ReadValue());
        guide?.SetHeldPiece(this);
    }

    void ClearHighlights()
    {
        foreach (var jp in jointPoints)
            jp.SetHighlight(false);
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        return mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, dragDepth));
    }
}
