using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public PuzzlePiece[] allPieces;
    public TextMeshProUGUI progressText;
    public GameObject completionPanel;

    private int placedCount = 0;

    void Update()
    {
        int count = 0;
        foreach (var p in allPieces)
            if (p.isPlaced) count++;

        if (count != placedCount)
        {
            placedCount = count;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (progressText != null)
            progressText.text = $"Pieces: {placedCount} / {allPieces.Length}";

        if (placedCount == allPieces.Length)
            OnPuzzleComplete();
    }

    void OnPuzzleComplete()
    {
        if (completionPanel != null)
            completionPanel.SetActive(true);
        Debug.Log("Puzzle Complete!");
    }
}
