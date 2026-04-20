using UnityEngine;

// Shows static colored joint indicators — no pulsing, just fixed LEGO-style studs
public class GhostGuide : MonoBehaviour
{
    public enum GuideMode { AlwaysVisible, OnlyWhenHolding }
    public GuideMode mode = GuideMode.AlwaysVisible;

    private PuzzlePiece heldPiece;

    public void SetHeldPiece(PuzzlePiece piece) => heldPiece = piece;
    public void ClearHeldPiece() => heldPiece = null;
}
