using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShapeTest : EditorWindow
{
    // ── Arch leg dimensions (shared) ──────────────────────────────────────
    const float BOTTOM_W = 9f;
    const float TOP_W    = 4.5f;
    const float HEIGHT   = 3f;
    const float DEPTH    = 0.4f;
    const float LEG_T    = 0.35f;

    [MenuItem("Shape Test/Create Arch Leg")]
    public static void CreateArchLeg()
    {
        foreach (var n in new[]{"Test_ArchLeg","Test_4Legs","Test_Trapezoid","Test_4ArchLegs"})
        { var o = GameObject.Find(n); if (o != null) DestroyImmediate(o); }

        var root = new GameObject("Test_ArchLeg");
        float R = BOTTOM_W * 0.45f / 2f;
        root.transform.position = new Vector3(0, R + HEIGHT / 2f, 8f);

        BuildArchLeg(root, MakeWoodMat());
        Selection.activeGameObject = root;
        Debug.Log("Arch leg created!");
    }

    [MenuItem("Shape Test/Create 4 Arch Legs")]
    public static void Create4ArchLegs()
    {
        var old = GameObject.Find("Test_4ArchLegs");
        if (old != null) DestroyImmediate(old);

        float R    = BOTTOM_W * 0.45f / 2f;
        float posY = R + HEIGHT / 2f;
        float gap  = BOTTOM_W + 1f; // space them side by side

        for (int i = 0; i < 4; i++)
        {
            var root = new GameObject("ArchLeg_" + (i + 1));
            root.transform.position = new Vector3((i - 1.5f) * gap, posY, 0);
            BuildArchLeg(root, MakeWoodMat());

            // Add Rigidbody so you can move them in Play mode
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
            root.AddComponent<BoxCollider>().size = new Vector3(BOTTOM_W, HEIGHT, DEPTH * 2f);
        }

        Debug.Log("4 arch legs created — select any one in the Hierarchy to move it.");
    }

    // ── Builder ───────────────────────────────────────────────────────────

    static void BuildArchLeg(GameObject root, Material mat)
    {
        float R        = BOTTOM_W * 0.45f / 2f;
        float yBase    = -HEIGHT / 2f;
        float legAngle = Mathf.Atan2((BOTTOM_W - TOP_W) / 2f, HEIGHT) * Mathf.Rad2Deg;
        float legLen   = Mathf.Sqrt(HEIGHT * HEIGHT + Mathf.Pow((BOTTOM_W - TOP_W) / 2f, 2f));
        float midX     = (BOTTOM_W / 2f + TOP_W / 2f) / 2f;

        var legL = AddBox(root, "LegL", mat, new Vector3(LEG_T, legLen, DEPTH), new Vector3(-midX, 0, 0));
        legL.transform.localRotation = Quaternion.Euler(0, 0, -legAngle);

        var legR = AddBox(root, "LegR", mat, new Vector3(LEG_T, legLen, DEPTH), new Vector3(midX, 0, 0));
        legR.transform.localRotation = Quaternion.Euler(0, 0, legAngle);

        AddBox(root, "TopBar", mat, new Vector3(TOP_W, LEG_T, DEPTH), new Vector3(0, HEIGHT / 2f, 0));

        CreateSemicircleArc(root, "Arc", mat,
            centerX: 0f, centerY: yBase,
            radius: R, thickness: LEG_T, depth: DEPTH, segments: 32, curveUp: true);

        float connLen = BOTTOM_W / 2f - R + LEG_T;
        float connCx  = (R - LEG_T / 2f) + connLen / 2f;
        AddBox(root, "ConnL", mat, new Vector3(connLen, LEG_T, DEPTH), new Vector3(-connCx, yBase, 0));
        AddBox(root, "ConnR", mat, new Vector3(connLen, LEG_T, DEPTH), new Vector3( connCx, yBase, 0));

        BuildLattice(root, mat, R, yBase);
    }

    static void BuildLattice(GameObject root, Material mat, float arcR, float arcCy)
    {
        float barT    = 0.08f;
        float barD    = DEPTH * 0.6f;
        float cellSz  = 1.0f;

        int rows = Mathf.CeilToInt(HEIGHT / cellSz);

        for (int row = 0; row < rows; row++)
        {
            float yCen = -HEIGHT / 2f + (row + 0.5f) * cellSz;
            if (yCen >= HEIGHT / 2f) continue;

            // Interior width narrows from BOTTOM_W to TOP_W as we go up
            float t      = (yCen + HEIGHT / 2f) / HEIGHT;
            float innerW = Mathf.Lerp(BOTTOM_W, TOP_W, t) - LEG_T * 2.5f;
            int   cols   = Mathf.Max(1, Mathf.RoundToInt(innerW / cellSz));
            float cellW  = innerW / cols;

            for (int col = 0; col < cols; col++)
            {
                float xCen = -innerW / 2f + (col + 0.5f) * cellW;

                // Skip cells whose center falls inside the semicircle
                float dx   = xCen;
                float dy   = yCen - arcCy;
                if (Mathf.Sqrt(dx * dx + dy * dy) < arcR - barT) continue;

                float diagLen = Mathf.Sqrt(cellW * cellW + cellSz * cellSz);
                float angle   = Mathf.Atan2(cellSz, cellW) * Mathf.Rad2Deg;
                Vector3 pos   = new Vector3(xCen, yCen, 0);

                var bA = AddBox(root, "Lat" + row + "_" + col + "A", mat,
                    new Vector3(barT, diagLen, barD), pos);
                bA.transform.localRotation = Quaternion.Euler(0, 0, angle);

                var bB = AddBox(root, "Lat" + row + "_" + col + "B", mat,
                    new Vector3(barT, diagLen, barD), pos);
                bB.transform.localRotation = Quaternion.Euler(0, 0, -angle);
            }
        }
    }

    // ── Wood material with procedural grain texture ────────────────────────
    static Material MakeWoodMat()
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.mainTexture = GenerateWoodTexture();
        mat.SetFloat("_Smoothness", 0.05f);
        return mat;
    }

    static Texture2D GenerateWoodTexture(int w = 256, int h = 256)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        Color dark  = new Color(0.38f, 0.22f, 0.08f);
        Color mid   = new Color(0.58f, 0.38f, 0.16f);
        Color light = new Color(0.76f, 0.56f, 0.28f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Horizontal grain lines with wavy distortion
                float distort = Mathf.PerlinNoise(x * 0.015f, y * 0.005f) * 40f;
                float wave    = Mathf.Sin((y + distort) * 0.18f) * 0.5f + 0.5f;
                // Fine grain noise
                float fine    = Mathf.PerlinNoise(x * 0.05f, y * 1.2f) * 0.3f;
                float t = Mathf.Clamp01(wave * 0.7f + fine);
                Color c = t < 0.5f ? Color.Lerp(dark, mid, t * 2f) : Color.Lerp(mid, light, (t - 0.5f) * 2f);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    // ── Semicircle arc mesh ────────────────────────────────────────────────
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

        for (int i = 0; i <= segments; i++)
        {
            float alpha = Mathf.PI * i / segments;
            float nx = -Mathf.Cos(alpha);
            float ny = curveUp ? Mathf.Sin(alpha) : -Mathf.Sin(alpha);
            float px = centerX + radius * nx;
            float py = centerY + radius * ny;

            verts.Add(new Vector3(px + halfT * nx, py + halfT * ny,  halfD)); // outer-front
            verts.Add(new Vector3(px + halfT * nx, py + halfT * ny, -halfD)); // outer-back
            verts.Add(new Vector3(px - halfT * nx, py - halfT * ny,  halfD)); // inner-front
            verts.Add(new Vector3(px - halfT * nx, py - halfT * ny, -halfD)); // inner-back
        }

        for (int i = 0; i < segments; i++)
        {
            int b0 = i * 4;
            int b1 = (i + 1) * 4;
            AddQuad(tris, b0+0, b1+0, b0+1, b1+1); // outer
            AddQuad(tris, b1+2, b0+2, b1+3, b0+3); // inner
            AddQuad(tris, b0+2, b1+2, b0+0, b1+0); // front
            AddQuad(tris, b0+1, b1+1, b0+3, b1+3); // back
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

    [MenuItem("Shape Test/Create Magnet Test (2 pieces)")]
    public static void CreateMagnetTest()
    {
        // Wipe every test and puzzle object from the scene
        foreach (var n in new[]{"Test_Magnet","Test_ArchLeg","Test_4Legs","Test_Trapezoid",
                                 "Test_4ArchLegs","Test_Foundation","EiffelTower_Puzzle","ArchLeg_1",
                                 "ArchLeg_2","ArchLeg_3","ArchLeg_4"})
        { var o = GameObject.Find(n); if (o != null) DestroyImmediate(o); }

        var scene  = new GameObject("Test_Magnet");
        Material wood   = MakeWoodMat();
        Material green  = MakeFlatMat(new Color(0.1f, 0.85f, 0.2f));
        Material yellow = MakeFlatMat(new Color(1f, 0.80f, 0f));

        // Piece A — fixed at center
        var a = MakeTestSlab(scene, "PieceA", wood, green, yellow, new Vector3(0, 0.25f, 0), true);

        // Piece B — off to the side, free to drag
        MakeTestSlab(scene, "PieceB", wood, green, yellow, new Vector3(12f, 0.25f, 0), false);

        // Position camera wide and overhead to see both pieces clearly
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(6f, 18f, -22f);
            cam.transform.LookAt(new Vector3(6f, 0f, 0f));
            var orbit = cam.GetComponent<OrbitCamera>();
            if (orbit != null) { orbit.distance = 28f; orbit.target = scene.transform; }
        }

        Selection.activeGameObject = scene;
        Debug.Log("Magnet test ready — Press Play, drag PieceB toward PieceA and feel the pull!");
    }

    static GameObject MakeTestSlab(GameObject parent, string name, Material wood, Material green, Material yellow,
        Vector3 pos, bool fixed_)
    {
        float s = 8f; float thick = 0.5f; float jOff = 2f;

        var slab = new GameObject(name);
        slab.transform.SetParent(parent.transform);
        slab.transform.position = pos;

        var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(slab.transform);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale    = new Vector3(s, thick, s);
        body.GetComponent<Renderer>().sharedMaterial = wood;
        DestroyImmediate(body.GetComponent<Collider>());

        slab.AddComponent<BoxCollider>().size = new Vector3(s, thick, s);
        var rb = slab.AddComponent<Rigidbody>();
        rb.isKinematic = true; rb.useGravity = false;

        var pp = slab.AddComponent<PuzzlePiece>();
        pp.pieceID  = name; pp.category = PuzzlePiece.PieceCategory.Base;
        pp.isPlaced = fixed_;
        pp.isFixed  = fixed_;

        // Top sockets (green)
        AddJointMarker(slab, "socket_slab", "plug_slab", new Vector3(-jOff, thick/2f, -jOff), green);
        AddJointMarker(slab, "socket_slab", "plug_slab", new Vector3( jOff, thick/2f, -jOff), green);
        AddJointMarker(slab, "socket_slab", "plug_slab", new Vector3(-jOff, thick/2f,  jOff), green);
        AddJointMarker(slab, "socket_slab", "plug_slab", new Vector3( jOff, thick/2f,  jOff), green);

        // Bottom plugs (yellow) — only on the free piece
        if (!fixed_)
        {
            AddJointMarker(slab, "plug_slab", "socket_slab", new Vector3(-jOff, -thick/2f, -jOff), yellow);
            AddJointMarker(slab, "plug_slab", "socket_slab", new Vector3( jOff, -thick/2f, -jOff), yellow);
            AddJointMarker(slab, "plug_slab", "socket_slab", new Vector3(-jOff, -thick/2f,  jOff), yellow);
            AddJointMarker(slab, "plug_slab", "socket_slab", new Vector3( jOff, -thick/2f,  jOff), yellow);
        }

        return slab;
    }

    [MenuItem("Shape Test/Create Foundation Slabs")]
    public static void CreateFoundationSlabs()
    {
        var old = GameObject.Find("Test_Foundation");
        if (old != null) DestroyImmediate(old);

        var scene = new GameObject("Test_Foundation");

        Material wood   = MakeWoodMat();
        Material green  = MakeFlatMat(new Color(0.1f, 0.85f, 0.2f));
        Material yellow = MakeFlatMat(new Color(1f,   0.80f, 0f));

        // Sizes: biggest at bottom (index 0), smallest at top (index 3)
        // Slab 4 (top) is just bigger than arch leg bottomW=9
        float[] sizes = { 18f, 15f, 12f, 10f };
        float thick   = 0.5f;
        float jOff    = 2.5f; // joint offset from center — same on all slabs so they align

        // Slab 1 fixed at center; others scattered for kid to pick up and stack
        Vector3[] startPos = {
            new Vector3(  0f, thick / 2f,   0f),   // slab 1 — fixed base at center
            new Vector3(-14f, thick / 2f,   0f),   // slab 2 — left
            new Vector3( 14f, thick / 2f,   0f),   // slab 3 — right
            new Vector3(  0f, thick / 2f, -14f),   // slab 4 — front
        };

        for (int i = 0; i < 4; i++)
        {
            float s    = sizes[i];
            bool fixed_ = (i == 0); // only the biggest bottom slab is fixed

            var slab = new GameObject("Slab_" + (i + 1));
            slab.transform.SetParent(scene.transform);
            slab.transform.position = startPos[i];

            // Body
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(slab.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale    = new Vector3(s, thick, s);
            body.GetComponent<Renderer>().sharedMaterial = wood;
            DestroyImmediate(body.GetComponent<Collider>());

            // Collider + physics on slab root
            slab.AddComponent<BoxCollider>().size = new Vector3(s, thick, s);
            var rb = slab.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;

            // PuzzlePiece
            var pp = slab.AddComponent<PuzzlePiece>();
            pp.pieceID  = "slab_" + (i + 1);
            pp.category = PuzzlePiece.PieceCategory.Base;
            pp.isPlaced = fixed_;

            // Bottom plugs (slide into the slab below) — not needed on very bottom slab
            if (i > 0)
            {
                AddJointMarker(slab, "plug_slab",   "socket_slab", new Vector3(-jOff, -thick/2f, -jOff), yellow);
                AddJointMarker(slab, "plug_slab",   "socket_slab", new Vector3( jOff, -thick/2f, -jOff), yellow);
                AddJointMarker(slab, "plug_slab",   "socket_slab", new Vector3(-jOff, -thick/2f,  jOff), yellow);
                AddJointMarker(slab, "plug_slab",   "socket_slab", new Vector3( jOff, -thick/2f,  jOff), yellow);
            }

            // Top sockets — receive the slab above (or arch legs on the top slab)
            string topSocket = (i == 3) ? "socket_arch" : "socket_slab";
            string topCompat = (i == 3) ? "plug_arch"   : "plug_slab";
            AddJointMarker(slab, topSocket, topCompat, new Vector3(-jOff, thick/2f, -jOff), green);
            AddJointMarker(slab, topSocket, topCompat, new Vector3( jOff, thick/2f, -jOff), green);
            AddJointMarker(slab, topSocket, topCompat, new Vector3(-jOff, thick/2f,  jOff), green);
            AddJointMarker(slab, topSocket, topCompat, new Vector3( jOff, thick/2f,  jOff), green);

        }

        Selection.activeGameObject = scene;
        Debug.Log("Foundation slabs ready — enter Play mode and stack them biggest to smallest!");
    }

    static void AddJointMarker(GameObject parent, string jType, string compatType, Vector3 localPos, Material mat)
    {
        var jGo = new GameObject("Joint_" + jType);
        jGo.transform.SetParent(parent.transform);
        jGo.transform.localPosition = localPos;

        var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        vis.name = "JointVisual";
        vis.transform.SetParent(jGo.transform);
        vis.transform.localPosition = Vector3.zero;
        vis.transform.localScale    = Vector3.one * 0.5f;
        vis.GetComponent<Renderer>().sharedMaterial = mat;
        DestroyImmediate(vis.GetComponent<Collider>());

        var jp = jGo.AddComponent<JointPoint>();
        jp.jointType     = jType;
        jp.compatibleType = compatType;
        jp.snapRadius    = 1.5f;
    }

    static Material MakeFlatMat(Color c)
    {
        var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.color = c;
        m.SetFloat("_Smoothness", 0.3f);
        return m;
    }

    [MenuItem("Shape Test/Create Lower Connector")]
    public static void CreateLowerConnectors()
    {
        var old = GameObject.Find("Test_LowerConnectors");
        if (old != null) DestroyImmediate(old);

        var root = new GameObject("Test_LowerConnectors");
        root.transform.position = new Vector3(0, 1f, 0f);
        BuildLowerConnector(root, MakeFlatMat(new Color(0.72f, 0.48f, 0.22f)));

        Selection.activeGameObject = root;
        Debug.Log("Lower connector created!");
    }

    // Single compound: 6 open-front boxes in a row, sharing dividing walls
    static void BuildLowerConnector(GameObject root, Material mat)
    {
        int   count = 6;
        float boxW  = 0.8f;   // width of each cell
        float boxH  = 1.0f;   // height (smaller)
        float boxD  = 0.75f;  // depth front-to-back
        float t     = 0.1f;   // wall thickness (bigger)
        float totalW = count * boxW;
        float startX = -totalW / 2f;

        for (int i = 0; i < count; i++)
        {
            float cx = startX + i * boxW + boxW / 2f;
            Vector3 c = new Vector3(cx, boxH / 2f, 0f);

            // Base
            AddBox(root, "Base" + i, mat, new Vector3(boxW, t, boxD),
                new Vector3(cx, t / 2f, 0f));
            // Top wall (added — closes the top)
            AddBox(root, "Top" + i, mat, new Vector3(boxW, t, boxD),
                new Vector3(cx, boxH - t / 2f, 0f));
            // Back wall
            AddBox(root, "Back" + i, mat, new Vector3(boxW, boxH, t),
                new Vector3(cx, boxH / 2f, boxD / 2f - t / 2f));
            // Left divider (skip on first cell — outer left wall added separately)
            if (i == 0)
                AddBox(root, "WallL" + i, mat, new Vector3(t, boxH, boxD),
                    new Vector3(startX + t / 2f, boxH / 2f, 0f));
            // Right divider / outer right wall
            AddBox(root, "WallR" + i, mat, new Vector3(t, boxH, boxD),
                new Vector3(startX + (i + 1) * boxW - t / 2f, boxH / 2f, 0f));
        }
    }

    [MenuItem("Shape Test/Create Solid Trapezoid")]
    public static void CreateTrapezoid()
    {
        var old = GameObject.Find("Test_Trapezoid");
        if (old != null) DestroyImmediate(old);

        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.55f, 0.35f, 0.18f);

        float legAngle = Mathf.Atan2((BOTTOM_W - TOP_W) / 2f, HEIGHT) * Mathf.Rad2Deg;
        float legLen   = Mathf.Sqrt(HEIGHT * HEIGHT + Mathf.Pow((BOTTOM_W - TOP_W) / 2f, 2f));
        float midX     = (BOTTOM_W / 2f + TOP_W / 2f) / 2f;

        var root = new GameObject("Test_Trapezoid");
        root.transform.position = new Vector3(0, 1.5f, 8f);

        var legL = AddBox(root, "LegL", mat, new Vector3(LEG_T, legLen, DEPTH), new Vector3(-midX, 0, 0));
        legL.transform.localRotation = Quaternion.Euler(0, 0, -legAngle);

        var legR = AddBox(root, "LegR", mat, new Vector3(LEG_T, legLen, DEPTH), new Vector3(midX, 0, 0));
        legR.transform.localRotation = Quaternion.Euler(0, 0, legAngle);

        AddBox(root, "BottomBar", mat, new Vector3(BOTTOM_W, LEG_T, DEPTH), new Vector3(0, -HEIGHT / 2f, 0));
        AddBox(root, "TopBar",    mat, new Vector3(TOP_W,    LEG_T, DEPTH), new Vector3(0,  HEIGHT / 2f, 0));

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
