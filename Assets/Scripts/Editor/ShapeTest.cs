using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShapeTest : EditorWindow
{
    [MenuItem("Shape Test/Create Arch Leg")]
    public static void CreateArchLeg()
    {
        var old = GameObject.Find("Test_ArchLeg");
        if (old != null) DestroyImmediate(old);

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.55f, 0.35f, 0.18f);

        float bottomW = 9f;
        float topW    = 4.5f;
        float height  = 3f;
        float depth   = 0.4f;
        float legT    = 0.35f;

        float diameter = bottomW * 0.45f;  // smaller radius
        float R        = diameter / 2f;
        float yBase    = -height / 2f;     // -1.5
        float arcCy    = yBase;            // arc diameter sits exactly at trapezoid bottom

        float legAngle = Mathf.Atan2((bottomW - topW) / 2f, height) * Mathf.Rad2Deg;
        float legLen   = Mathf.Sqrt(height * height + Mathf.Pow((bottomW - topW) / 2f, 2f));
        float midX     = (bottomW / 2f + topW / 2f) / 2f;

        var root = new GameObject("Test_ArchLeg");
        root.transform.position = new Vector3(0, R + height / 2f, 8f);

        // ── Side legs ─────────────────────────────────────────────────
        var legL = AddBox(root, "LegL", mat, new Vector3(legT, legLen, depth), new Vector3(-midX, 0, 0));
        legL.transform.localRotation = Quaternion.Euler(0, 0, -legAngle);

        var legR = AddBox(root, "LegR", mat, new Vector3(legT, legLen, depth), new Vector3(midX, 0, 0));
        legR.transform.localRotation = Quaternion.Euler(0, 0, legAngle);

        // ── Top bar ────────────────────────────────────────────────────
        AddBox(root, "TopBar", mat, new Vector3(topW, legT, depth), new Vector3(0, height / 2f, 0));

        // ── Smooth semicircle border curving UPWARD ────────────────────
        CreateSemicircleArc(root, "SemicircleArc", mat,
            centerX: 0f, centerY: arcCy,
            radius: R, thickness: legT, depth: depth, segments: 32, curveUp: true);

        // ── Connectors: horizontal bars from arc endpoints to leg bottoms ─
        // Arc endpoints sit at x=±R, y=yBase (diameter ends face left/right)
        // Leg bottoms at x=±bottomW/2, y=yBase — same height, so connectors are horizontal
        float connLen = bottomW / 2f - R;
        float connCx  = R + connLen / 2f;

        AddBox(root, "ConnL", mat, new Vector3(connLen, legT, depth), new Vector3(-connCx, yBase, 0));
        AddBox(root, "ConnR", mat, new Vector3(connLen, legT, depth), new Vector3( connCx, yBase, 0));

        Selection.activeGameObject = root;
        Debug.Log("Arch leg with smooth semicircle border created!");
    }

    // Generates a single smooth 3D semicircle arc mesh (border only, no fill)
    static void CreateSemicircleArc(GameObject parent, string name, Material mat,
        float centerX, float centerY, float radius, float thickness, float depth, int segments, bool curveUp = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale    = Vector3.one;

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        float halfT = thickness / 2f;
        float halfD = depth / 2f;

        var verts = new List<Vector3>();
        var tris  = new List<int>();

        // For each cross-section point along the arc: 4 vertices
        // Layout: [outer-front, outer-back, inner-front, inner-back]
        for (int i = 0; i <= segments; i++)
        {
            float alpha = Mathf.PI * i / segments; // 0=left end, PI=right end, sweeps DOWN

            // Outward normal from circle center
            float nx = -Mathf.Cos(alpha);
            float ny = curveUp ? Mathf.Sin(alpha) : -Mathf.Sin(alpha); // flip Y for upward curve

            // Point on arc centerline
            float px = centerX + radius * nx;
            float py = centerY + radius * ny;

            verts.Add(new Vector3(px + halfT * nx, py + halfT * ny,  halfD)); // outer-front
            verts.Add(new Vector3(px + halfT * nx, py + halfT * ny, -halfD)); // outer-back
            verts.Add(new Vector3(px - halfT * nx, py - halfT * ny,  halfD)); // inner-front
            verts.Add(new Vector3(px - halfT * nx, py - halfT * ny, -halfD)); // inner-back
        }

        // Connect adjacent cross-sections with quads (double-sided)
        for (int i = 0; i < segments; i++)
        {
            int b0 = i * 4;
            int b1 = (i + 1) * 4;

            // Outer face
            AddQuad(tris, b0+0, b1+0, b0+1, b1+1);
            // Inner face
            AddQuad(tris, b1+2, b0+2, b1+3, b0+3);
            // Front face (z+)
            AddQuad(tris, b0+2, b1+2, b0+0, b1+0);
            // Back face (z-)
            AddQuad(tris, b0+1, b1+1, b0+3, b1+3);
        }

        // End caps
        AddQuad(tris, 2, 0, 3, 1);
        int L = segments * 4;
        AddQuad(tris, L+0, L+2, L+1, L+3);

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mf.sharedMesh = mesh;
    }

    static void AddQuad(List<int> tris, int a, int b, int c, int d)
    {
        tris.Add(a); tris.Add(b); tris.Add(c);
        tris.Add(b); tris.Add(d); tris.Add(c);
    }

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

        float legAngle = Mathf.Atan2((bottomW - topW) / 2f, height) * Mathf.Rad2Deg;
        float legLen   = Mathf.Sqrt(height * height + Mathf.Pow((bottomW - topW) / 2f, 2f));
        float midX     = (bottomW / 2f + topW / 2f) / 2f;

        var root = new GameObject("Test_Trapezoid");
        root.transform.position = new Vector3(0, 1.5f, 8f);

        var legL = AddBox(root, "LegL", mat, new Vector3(legT, legLen, depth), new Vector3(-midX, 0, 0));
        legL.transform.localRotation = Quaternion.Euler(0, 0, -legAngle);

        var legR = AddBox(root, "LegR", mat, new Vector3(legT, legLen, depth), new Vector3(midX, 0, 0));
        legR.transform.localRotation = Quaternion.Euler(0, 0, legAngle);

        AddBox(root, "BottomBar", mat, new Vector3(bottomW, legT, depth), new Vector3(0, -height / 2f, 0));
        AddBox(root, "TopBar",    mat, new Vector3(topW,    legT, depth), new Vector3(0,  height / 2f, 0));

        Selection.activeGameObject = root;
    }

    static GameObject AddBox(GameObject parent, string name, Material mat, Vector3 size, Vector3 localPos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale    = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }
}
