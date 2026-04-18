using UnityEngine;
using UnityEditor;

public class EiffelTowerGenerator : EditorWindow
{
    [MenuItem("Eiffel Puzzle/Generate Tower Pieces")]
    public static void Generate()
    {
        ClearExisting();

        Material ironMat   = CreateMaterial("IronGrey",  new Color(0.35f, 0.38f, 0.45f));
        Material baseMat   = CreateMaterial("BaseStone", new Color(0.55f, 0.50f, 0.40f));
        Material greenMat  = CreateMaterial("Socket",    new Color(0.1f,  0.85f, 0.2f));
        Material yellowMat = CreateMaterial("Plug",      new Color(1f,    0.80f, 0f));

        GameObject root = new GameObject("EiffelTower_Puzzle");

        // ── BASE (fixed at origin) ─────────────────────────────────────────
        // Scale: (5, 0.4, 5)  →  local y=0.5 = top surface
        var baseObj = MakePiece("Base_Foundation", root, PrimitiveType.Cube, new Vector3(5f, 0.4f, 5f), baseMat);
        baseObj.transform.position = new Vector3(0, -0.2f, 0); // sit on ground
        var basePiece = baseObj.GetComponent<PuzzlePiece>();
        basePiece.isPlaced = true;
        basePiece.pieceID  = "base";
        basePiece.category = PuzzlePiece.PieceCategory.Base;
        baseObj.GetComponent<Rigidbody>().isKinematic = true;

        // Green sockets on BASE top surface — normalized local coords, y=0.5 = top face
        // x/z corners at ±0.3 in local space → world ±1.5
        AddJoint(baseObj, "base_legFL", "leg_bottom", new Vector3(-0.3f,  0.5f, -0.3f), greenMat,  snapR: 0.8f);
        AddJoint(baseObj, "base_legFR", "leg_bottom", new Vector3( 0.3f,  0.5f, -0.3f), greenMat,  snapR: 0.8f);
        AddJoint(baseObj, "base_legBL", "leg_bottom", new Vector3(-0.3f,  0.5f,  0.3f), greenMat,  snapR: 0.8f);
        AddJoint(baseObj, "base_legBR", "leg_bottom", new Vector3( 0.3f,  0.5f,  0.3f), greenMat,  snapR: 0.8f);

        // ── LEGS (scale 0.6, 2.2, 0.6) ──────────────────────────────────────
        // local y=-0.5 = bottom face, y=+0.5 = top face
        string[]  legNames  = { "Leg_FrontLeft", "Leg_FrontRight", "Leg_BackLeft", "Leg_BackRight" };
        string[]  legSuffix = { "FL", "FR", "BL", "BR" };
        Vector3[] scatter   = {
            new Vector3(-8f, 1.1f, -4f), new Vector3( 8f, 1.1f, -4f),
            new Vector3(-8f, 1.1f,  4f), new Vector3( 8f, 1.1f,  4f)
        };

        for (int i = 0; i < 4; i++)
        {
            var leg = MakePiece(legNames[i], root, PrimitiveType.Cube, new Vector3(0.6f, 2.2f, 0.6f), ironMat);
            leg.transform.position = scatter[i];
            var lp = leg.GetComponent<PuzzlePiece>();
            lp.pieceID  = "leg_" + i;
            lp.category = PuzzlePiece.PieceCategory.Lower;

            AddJoint(leg, "leg_bottom", "base_leg" + legSuffix[i], new Vector3(0, -0.5f, 0), yellowMat, snapR: 0.8f);
            AddJoint(leg, "leg_top",    "platform_bottom",          new Vector3(0,  0.5f, 0), greenMat,  snapR: 0.8f);
        }

        // ── PLATFORM 1 (scale 3, 0.35, 3) ─────────────────────────────────
        var plat1 = MakePiece("Platform_Level1", root, PrimitiveType.Cube, new Vector3(3f, 0.35f, 3f), ironMat);
        plat1.transform.position = new Vector3(-10f, 0.175f, 0f);
        var p1 = plat1.GetComponent<PuzzlePiece>();
        p1.pieceID = "platform1"; p1.category = PuzzlePiece.PieceCategory.Lower;

        AddJoint(plat1, "platform_bottom_FL", "leg_top", new Vector3(-0.35f, -0.5f, -0.35f), yellowMat, snapR: 0.8f);
        AddJoint(plat1, "platform_bottom_FR", "leg_top", new Vector3( 0.35f, -0.5f, -0.35f), yellowMat, snapR: 0.8f);
        AddJoint(plat1, "platform_bottom_BL", "leg_top", new Vector3(-0.35f, -0.5f,  0.35f), yellowMat, snapR: 0.8f);
        AddJoint(plat1, "platform_bottom_BR", "leg_top", new Vector3( 0.35f, -0.5f,  0.35f), yellowMat, snapR: 0.8f);
        AddJoint(plat1, "platform_top", "mid_bottom",    new Vector3( 0f,    0.5f,  0f),     greenMat,  snapR: 0.8f);

        // ── MID SECTION (scale 1.1, 2.5, 1.1) ────────────────────────────
        var mid = MakePiece("Mid_Section", root, PrimitiveType.Cube, new Vector3(1.1f, 2.5f, 1.1f), ironMat);
        mid.transform.position = new Vector3(10f, 1.25f, 0f);
        var mp = mid.GetComponent<PuzzlePiece>();
        mp.pieceID = "mid"; mp.category = PuzzlePiece.PieceCategory.Middle;

        AddJoint(mid, "mid_bottom", "platform_top",     new Vector3(0, -0.5f, 0), yellowMat, snapR: 0.8f);
        AddJoint(mid, "mid_top",    "platform2_bottom", new Vector3(0,  0.5f, 0), greenMat,  snapR: 0.8f);

        // ── PLATFORM 2 (scale 1.8, 0.25, 1.8) ───────────────────────────
        var plat2 = MakePiece("Platform_Level2", root, PrimitiveType.Cube, new Vector3(1.8f, 0.25f, 1.8f), ironMat);
        plat2.transform.position = new Vector3(-10f, 4f, 0f);
        var p2 = plat2.GetComponent<PuzzlePiece>();
        p2.pieceID = "platform2"; p2.category = PuzzlePiece.PieceCategory.Middle;

        AddJoint(plat2, "platform2_bottom", "mid_top",      new Vector3(0, -0.5f, 0), yellowMat, snapR: 0.8f);
        AddJoint(plat2, "platform2_top",    "spire_bottom", new Vector3(0,  0.5f, 0), greenMat,  snapR: 0.8f);

        // ── SPIRE (Cylinder scale 0.3, 1.8, 0.3 — Unity cylinder local height = 2) ──
        var spire = MakePiece("Spire_Top", root, PrimitiveType.Cylinder, new Vector3(0.3f, 1.8f, 0.3f), ironMat);
        spire.transform.position = new Vector3(0f, 8f, 0f);
        var sp = spire.GetComponent<PuzzlePiece>();
        sp.pieceID = "spire"; sp.category = PuzzlePiece.PieceCategory.Top;

        // Cylinder local surface is at y=±1 (not ±0.5 like cube)
        AddJoint(spire, "spire_bottom", "platform2_top", new Vector3(0, -1f, 0), yellowMat, snapR: 0.8f);

        // ── Puzzle Manager ────────────────────────────────────────────────
        var mgrGo = new GameObject("PuzzleManager");
        var pm = mgrGo.AddComponent<PuzzleManager>();
        pm.allPieces = root.GetComponentsInChildren<PuzzlePiece>();

        Selection.activeGameObject = root;
        Debug.Log("Generated! Green = socket. Yellow = plug. Drag yellow onto green to connect.");
    }

    static GameObject MakePiece(string name, GameObject parent, PrimitiveType shape, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;

        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity  = false;
        rb.isKinematic = true;

        go.AddComponent<PuzzlePiece>();
        return go;
    }

    // localPos uses NORMALIZED local space: y=+0.5 is top surface, y=-0.5 is bottom surface
    // The visual cap is sized ~40% of the piece width for clear visibility
    static void AddJoint(GameObject piece, string jointType, string compatibleType,
        Vector3 localPos, Material mat, float snapR)
    {
        var jObj = new GameObject("Joint_" + jointType);
        jObj.transform.SetParent(piece.transform);
        jObj.transform.localPosition = localPos;
        jObj.transform.localRotation = Quaternion.identity;
        jObj.transform.localScale    = Vector3.one;

        var jp = jObj.AddComponent<JointPoint>();
        jp.jointType      = jointType;
        jp.compatibleType = compatibleType;
        jp.snapRadius     = snapR;

        // Flat disc cap — sized as fraction of piece face, always 0.08 world units tall
        Vector3 ps = piece.transform.localScale;
        float capW = 0.4f; // 40% of normalized width → stays proportional
        float capH = 0.08f / Mathf.Abs(ps.y); // fixed world height 0.08

        var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "JointVisual";
        cap.transform.SetParent(jObj.transform);
        cap.transform.localPosition = Vector3.zero;
        cap.transform.localScale    = new Vector3(capW, capH, capW);
        cap.GetComponent<Renderer>().sharedMaterial = mat;
        DestroyImmediate(cap.GetComponent<Collider>());

        piece.GetComponent<PuzzlePiece>().jointPoints =
            piece.GetComponentsInChildren<JointPoint>();
    }

    static void ClearExisting()
    {
        var old = GameObject.Find("EiffelTower_Puzzle");
        if (old != null) DestroyImmediate(old);
        var mgr = GameObject.Find("PuzzleManager");
        if (mgr != null) DestroyImmediate(mgr);
    }

    static Material CreateMaterial(string name, Color color)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        mat.name  = name;
        return mat;
    }
}
