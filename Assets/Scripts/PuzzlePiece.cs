using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzlePiece : MonoBehaviour
{
    public enum PieceCategory { Base, Lower, Middle, Top }

    public string pieceID;
    public PieceCategory category;
    public JointPoint[] jointPoints;
    public bool isPlaced = false;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private float dragDepth;
    private Camera mainCam;
    private GhostGuide guide;

    void Start()
    {
        mainCam = Camera.main;
        guide   = FindAnyObjectByType<GhostGuide>();
    }

    void OnMouseDown()
    {
        if (isPlaced) return;
        isDragging  = true;
        dragDepth   = mainCam.WorldToScreenPoint(transform.position).z;
        dragOffset  = transform.position - ScreenToWorld(Mouse.current.position.ReadValue());
        guide?.SetHeldPiece(this);
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        transform.position = ScreenToWorld(Mouse.current.position.ReadValue()) + dragOffset;
        CheckSnap(); // snap as soon as close enough while dragging
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;
        guide?.ClearHeldPiece();
        CheckSnap();
    }

    void CheckSnap()
    {
        foreach (var jp in jointPoints)
        {
            JointPoint nearest = jp.FindNearestCompatible();
            if (nearest != null)
            {
                SnapToJoint(jp, nearest);
                return;
            }
        }
    }

    void SnapToJoint(JointPoint myJoint, JointPoint targetJoint)
    {
        Vector3 offset = myJoint.transform.position - transform.position;
        transform.position = targetJoint.transform.position - offset;
        isPlaced = true;
        myJoint.Connect(targetJoint);
        HideJointVisual(myJoint);
        HideJointVisual(targetJoint);
    }

    void HideJointVisual(JointPoint jp)
    {
        var v = jp.transform.Find("JointVisual");
        if (v != null) v.gameObject.SetActive(false);
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        return mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, dragDepth));
    }
}
