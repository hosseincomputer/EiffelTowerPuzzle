using UnityEngine;

public class PuzzleBase : MonoBehaviour
{
    public JointPoint[] baseJoints;

    void Start()
    {
        // Base joints are always available as starting anchor points
        foreach (var j in baseJoints)
            j.isConnected = false;
    }
}
