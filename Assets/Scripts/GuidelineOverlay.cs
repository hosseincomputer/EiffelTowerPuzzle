using UnityEngine;

public class GuidelineOverlay : MonoBehaviour
{
    [Header("Ghost previews of where each piece goes")]
    public GameObject[] ghostPrefabs;       // transparent versions of each piece
    public Transform[] targetPositions;     // where each piece should end up
    public float fadeAlpha = 0.25f;

    private GameObject[] ghosts;

    void Start()
    {
        ghosts = new GameObject[ghostPrefabs.Length];
        for (int i = 0; i < ghostPrefabs.Length; i++)
        {
            ghosts[i] = Instantiate(ghostPrefabs[i], targetPositions[i].position, targetPositions[i].rotation);
            SetAlpha(ghosts[i], fadeAlpha);
        }
    }

    public void HideGhost(int index)
    {
        if (index < ghosts.Length && ghosts[index] != null)
            ghosts[index].SetActive(false);
    }

    void SetAlpha(GameObject go, float alpha)
    {
        foreach (var r in go.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in r.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
                mat.SetFloat("_Surface", 1); // URP transparent surface
            }
        }
    }
}
