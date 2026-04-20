using UnityEngine;
using UnityEditor;

public class ShapeTest : EditorWindow
{
    [MenuItem("Shape Test/Create Solid Trapezoid")]
    public static void CreateTrapezoid()
    {
        var old = GameObject.Find("Test_Trapezoid");
        if (old != null) DestroyImmediate(old);

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.55f, 0.35f, 0.18f);

        float bottomW = 9f;
        float topW    = 4.5f;
        float height  = 3f;
        float depth   = 0.4f;
        float legT    = 0.35f;
        float barT    = 0.1f;  // lattice bar thickness

        float legAngle = Mathf.Atan2((bottomW - topW) / 2f, height) * Mathf.Rad2Deg;
        float legLen   = Mathf.Sqrt(height * height + Mathf.Pow((bottomW - topW) / 2f, 2f));
        float midX     = (bottomW / 2f + topW / 2f) / 2f;

        var root = new GameObject("Test_Trapezoid");
        root.transform.position = new Vector3(0, 1.5f, 8f);

        // ── Legs ──────────────────────────────────────────────────────
        var legL = AddBox(root, "LegL", mat, new Vector3(legT, legLen, depth), new Vector3(-midX, 0, 0));
        legL.transform.localRotation = Quaternion.Euler(0, 0, -legAngle);

        var legR = AddBox(root, "LegR", mat, new Vector3(legT, legLen, depth), new Vector3(midX, 0, 0));
        legR.transform.localRotation = Quaternion.Euler(0, 0, legAngle);

        // ── Lattice fill (X pattern with horizontal dividers) ─────────
        int rows = 3;
        float rowH = height / rows;

        for (int row = 0; row < rows; row++)
        {
            float yBottom = -height / 2f + row * rowH;
            float yTop    = yBottom + rowH;
            float yCen    = (yBottom + yTop) / 2f;

            // Width at bottom and top of this row (lerp along trapezoid)
            float wBottom = Mathf.Lerp(bottomW, topW, (yBottom + height / 2f) / height) - legT;
            float wTop    = Mathf.Lerp(bottomW, topW, (yTop    + height / 2f) / height) - legT;
            float wCen    = (wBottom + wTop) / 2f;

            // Horizontal divider between rows
            if (row > 0)
                AddBox(root, "HDiv" + row, mat, new Vector3(wBottom + legT, barT, depth), new Vector3(0, yBottom, 0));

            // Number of columns based on row width
            int cols = Mathf.Max(1, Mathf.RoundToInt(wCen / rowH));
            float colW = wCen / cols;

            for (int col = 0; col < cols; col++)
            {
                float xCen = -wCen / 2f + colW * (col + 0.5f);

                // Cell diagonal length
                float diagLen   = Mathf.Sqrt(colW * colW + rowH * rowH);
                float diagAngle = Mathf.Atan2(colW, rowH) * Mathf.Rad2Deg;

                // X brace: two diagonals crossing
                var d1 = AddBox(root, $"X{row}_{col}A", mat,
                    new Vector3(barT, diagLen, depth * 0.6f),
                    new Vector3(xCen, yCen, 0));
                d1.transform.localRotation = Quaternion.Euler(0, 0, -diagAngle);

                var d2 = AddBox(root, $"X{row}_{col}B", mat,
                    new Vector3(barT, diagLen, depth * 0.6f),
                    new Vector3(xCen, yCen, 0));
                d2.transform.localRotation = Quaternion.Euler(0, 0, diagAngle);
            }
        }

        // ── Bars (drawn last to cover leg/lattice ends) ────────────────
        AddBox(root, "BottomBar", mat, new Vector3(bottomW, legT, depth), new Vector3(0, -height / 2f, 0));
        AddBox(root, "TopBar",    mat, new Vector3(topW,    legT, depth), new Vector3(0,  height / 2f, 0));

        Selection.activeGameObject = root;
        Debug.Log("Trapezoid with lattice pattern created!");
    }

    static GameObject AddBox(GameObject parent, string name, Material mat, Vector3 size, Vector3 localPos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
}
