using UnityEngine;
using UnityEditor;

public class EiffelTowerGenerator : EditorWindow
{
    [MenuItem("Eiffel Puzzle/Generate Tower Pieces")]
    public static void Generate()
    {
        ClearExisting();

        // Materials
        Material woodMat   = CreateMaterial("Wood",    new Color(0.55f, 0.35f, 0.18f));
        Material darkWood  = CreateMaterial("DarkWood",new Color(0.40f, 0.25f, 0.12f));
        Material greenMat  = CreateMaterial("Socket",  new Color(0.1f,  0.85f, 0.2f));
        Material yellowMat = CreateMaterial("Plug",    new Color(1f,    0.80f, 0f));

        GameObject root = new GameObject("EiffelTower_Puzzle");

        // ── 1. BASE PLATFORM (fixed) ───────────────────────────────────
        var baseObj = new GameObject("Base_Foundation");
        baseObj.transform.SetParent(root.transform);
        baseObj.transform.position = new Vector3(0, 0, 0);
        BuildBase(baseObj, woodMat);
        var basePiece = baseObj.AddComponent<PuzzlePiece>();
        basePiece.isPlaced = true;
        basePiece.pieceID  = "base";
        basePiece.category = PuzzlePiece.PieceCategory.Base;
        var baseRb = baseObj.AddComponent<Rigidbody>();
        baseRb.isKinematic = true;
        baseRb.useGravity  = false;
        baseObj.AddComponent<BoxCollider>().size = new Vector3(6f, 0.3f, 6f);

        // Green sockets at 4 corners for arch legs
        AddJoint(baseObj, basePiece, "socket_arch", "plug_arch", new Vector3(-1.5f, 0.15f, -1.5f), greenMat);
        AddJoint(baseObj, basePiece, "socket_arch", "plug_arch", new Vector3( 1.5f, 0.15f, -1.5f), greenMat);
        AddJoint(baseObj, basePiece, "socket_arch", "plug_arch", new Vector3(-1.5f, 0.15f,  1.5f), greenMat);
        AddJoint(baseObj, basePiece, "socket_arch", "plug_arch", new Vector3( 1.5f, 0.15f,  1.5f), greenMat);

        // ── 2. FOUR ARCH LEGS ─────────────────────────────────────────
        string[]  archNames  = { "Arch_FrontLeft", "Arch_FrontRight", "Arch_BackLeft", "Arch_BackRight" };
        string[]  archSuffix = { "FL", "FR", "BL", "BR" };
        Vector3[] archScatter = {
            new Vector3(-9f, 1.5f, -3f), new Vector3( 9f, 1.5f, -3f),
            new Vector3(-9f, 1.5f,  3f), new Vector3( 9f, 1.5f,  3f)
        };

        for (int i = 0; i < 4; i++)
        {
            var archObj = new GameObject(archNames[i]);
            archObj.transform.SetParent(root.transform);
            archObj.transform.position = archScatter[i];
            BuildArch(archObj, woodMat);

            var ap = archObj.AddComponent<PuzzlePiece>();
            ap.pieceID  = "arch_" + i;
            ap.category = PuzzlePiece.PieceCategory.Lower;
            var rb = archObj.AddComponent<Rigidbody>();
            rb.isKinematic = true; rb.useGravity = false;
            archObj.AddComponent<BoxCollider>().size = new Vector3(2.4f, 3f, 0.4f);

            AddJoint(archObj, ap, "plug_arch",    "socket_arch",   new Vector3(0, -1.5f, 0), yellowMat);
            AddJoint(archObj, ap, "socket_plat1", "plug_plat1",   new Vector3(0,  1.5f, 0), greenMat);
        }

        // ── 3. PLATFORM 1 (wide band) ─────────────────────────────────
        var plat1Obj = new GameObject("Platform_Level1");
        plat1Obj.transform.SetParent(root.transform);
        plat1Obj.transform.position = new Vector3(-11f, 0f, 0f);
        BuildPlatform(plat1Obj, woodMat, darkWood, new Vector3(4f, 0.5f, 4f));
        var p1 = plat1Obj.AddComponent<PuzzlePiece>();
        p1.pieceID = "platform1"; p1.category = PuzzlePiece.PieceCategory.Lower;
        var p1rb = plat1Obj.AddComponent<Rigidbody>();
        p1rb.isKinematic = true; p1rb.useGravity = false;
        plat1Obj.AddComponent<BoxCollider>().size = new Vector3(4f, 0.5f, 4f);

        AddJoint(plat1Obj, p1, "plug_plat1", "socket_plat1", new Vector3(-1.5f, -0.25f, -1.5f), yellowMat);
        AddJoint(plat1Obj, p1, "plug_plat1", "socket_plat1", new Vector3( 1.5f, -0.25f, -1.5f), yellowMat);
        AddJoint(plat1Obj, p1, "plug_plat1", "socket_plat1", new Vector3(-1.5f, -0.25f,  1.5f), yellowMat);
        AddJoint(plat1Obj, p1, "plug_plat1", "socket_plat1", new Vector3( 1.5f, -0.25f,  1.5f), yellowMat);
        AddJoint(plat1Obj, p1, "socket_lower", "plug_lower",  new Vector3(0,     0.25f,  0),    greenMat);

        // ── 4. LOWER TOWER SECTION ────────────────────────────────────
        var lowerObj = new GameObject("Tower_Lower");
        lowerObj.transform.SetParent(root.transform);
        lowerObj.transform.position = new Vector3(11f, 2f, 0f);
        BuildTapered(lowerObj, woodMat, darkWood, bottomSize: 2.8f, topSize: 1.6f, height: 4f);
        var lt = lowerObj.AddComponent<PuzzlePiece>();
        lt.pieceID = "lowerTower"; lt.category = PuzzlePiece.PieceCategory.Middle;
        var ltrb = lowerObj.AddComponent<Rigidbody>();
        ltrb.isKinematic = true; ltrb.useGravity = false;
        lowerObj.AddComponent<BoxCollider>().size = new Vector3(2.8f, 4f, 2.8f);

        AddJoint(lowerObj, lt, "plug_lower",   "socket_lower",  new Vector3(0, -2f, 0), yellowMat);
        AddJoint(lowerObj, lt, "socket_plat2", "plug_plat2",    new Vector3(0,  2f, 0), greenMat);

        // ── 5. PLATFORM 2 ─────────────────────────────────────────────
        var plat2Obj = new GameObject("Platform_Level2");
        plat2Obj.transform.SetParent(root.transform);
        plat2Obj.transform.position = new Vector3(-11f, 4f, 0f);
        BuildPlatform(plat2Obj, woodMat, darkWood, new Vector3(2.4f, 0.4f, 2.4f));
        var p2 = plat2Obj.AddComponent<PuzzlePiece>();
        p2.pieceID = "platform2"; p2.category = PuzzlePiece.PieceCategory.Middle;
        var p2rb = plat2Obj.AddComponent<Rigidbody>();
        p2rb.isKinematic = true; p2rb.useGravity = false;
        plat2Obj.AddComponent<BoxCollider>().size = new Vector3(2.4f, 0.4f, 2.4f);

        AddJoint(plat2Obj, p2, "plug_plat2",   "socket_plat2",  new Vector3(0, -0.2f, 0), yellowMat);
        AddJoint(plat2Obj, p2, "socket_upper", "plug_upper",    new Vector3(0,  0.2f, 0), greenMat);

        // ── 6. UPPER TOWER SECTION ────────────────────────────────────
        var upperObj = new GameObject("Tower_Upper");
        upperObj.transform.SetParent(root.transform);
        upperObj.transform.position = new Vector3(11f, 6f, 0f);
        BuildTapered(upperObj, woodMat, darkWood, bottomSize: 1.4f, topSize: 0.5f, height: 4f);
        var ut = upperObj.AddComponent<PuzzlePiece>();
        ut.pieceID = "upperTower"; ut.category = PuzzlePiece.PieceCategory.Middle;
        var utrb = upperObj.AddComponent<Rigidbody>();
        utrb.isKinematic = true; utrb.useGravity = false;
        upperObj.AddComponent<BoxCollider>().size = new Vector3(1.4f, 4f, 1.4f);

        AddJoint(upperObj, ut, "plug_upper",   "socket_upper",  new Vector3(0, -2f, 0), yellowMat);
        AddJoint(upperObj, ut, "socket_plat3", "plug_plat3",   new Vector3(0,  2f, 0), greenMat);

        // ── 7. PLATFORM 3 (small top band) ────────────────────────────
        var plat3Obj = new GameObject("Platform_Level3");
        plat3Obj.transform.SetParent(root.transform);
        plat3Obj.transform.position = new Vector3(-11f, 8f, 0f);
        BuildPlatform(plat3Obj, woodMat, darkWood, new Vector3(1f, 0.3f, 1f));
        var p3 = plat3Obj.AddComponent<PuzzlePiece>();
        p3.pieceID = "platform3"; p3.category = PuzzlePiece.PieceCategory.Top;
        var p3rb = plat3Obj.AddComponent<Rigidbody>();
        p3rb.isKinematic = true; p3rb.useGravity = false;
        plat3Obj.AddComponent<BoxCollider>().size = new Vector3(1f, 0.3f, 1f);

        AddJoint(plat3Obj, p3, "plug_plat3",   "socket_plat3",  new Vector3(0, -0.15f, 0), yellowMat);
        AddJoint(plat3Obj, p3, "socket_spire", "plug_spire",   new Vector3(0,  0.15f, 0), greenMat);

        // ── 8. SPIRE ──────────────────────────────────────────────────
        var spireObj = new GameObject("Spire");
        spireObj.transform.SetParent(root.transform);
        spireObj.transform.position = new Vector3(0f, 10f, 0f);
        BuildSpire(spireObj, woodMat);
        var sp = spireObj.AddComponent<PuzzlePiece>();
        sp.pieceID = "spire"; sp.category = PuzzlePiece.PieceCategory.Top;
        var sprb = spireObj.AddComponent<Rigidbody>();
        sprb.isKinematic = true; sprb.useGravity = false;
        spireObj.AddComponent<BoxCollider>().size = new Vector3(0.6f, 3.5f, 0.6f);

        AddJoint(spireObj, sp, "plug_spire", "socket_spire", new Vector3(0, -1.75f, 0), yellowMat);

        // ── Puzzle Manager ────────────────────────────────────────────
        var mgrGo = new GameObject("PuzzleManager");
        var pm = mgrGo.AddComponent<PuzzleManager>();
        pm.allPieces = root.GetComponentsInChildren<PuzzlePiece>();

        Selection.activeGameObject = root;
        Debug.Log("Eiffel Tower generated! 9 pieces total. Green=socket, Yellow=plug.");
    }

    // ── Shape Builders ────────────────────────────────────────────────

    // Flat base platform with slight decorative edge
    static void BuildBase(GameObject parent, Material mat)
    {
        // Main flat slab
        AddBox(parent, "Slab", mat, new Vector3(6f, 0.2f, 6f), Vector3.zero);
        // Raised border edges
        AddBox(parent, "EdgeF", mat, new Vector3(6f, 0.15f, 0.2f), new Vector3(0, 0.175f, -2.9f));
        AddBox(parent, "EdgeB", mat, new Vector3(6f, 0.15f, 0.2f), new Vector3(0, 0.175f,  2.9f));
        AddBox(parent, "EdgeL", mat, new Vector3(0.2f, 0.15f, 6f), new Vector3(-2.9f, 0.175f, 0));
        AddBox(parent, "EdgeR", mat, new Vector3(0.2f, 0.15f, 6f), new Vector3( 2.9f, 0.175f, 0));
    }

    // Trapezoid arch: bottom=3m wide, top=1.5m wide, height=3m, equal angled legs
    static void BuildArch(GameObject parent, Material mat)
    {
        float bottomW  = 3f;    // bottom width
        float topW     = 1.5f;  // top width
        float height   = 3f;    // total height
        float thickness = 0.35f; // leg thickness

        // The legs lean inward — calculate angle from geometry
        // horizontal offset per leg = (bottomW - topW) / 2 = 0.75
        // angle = atan(0.75 / height) in degrees
        float legAngle = Mathf.Atan2((bottomW - topW) / 2f, height) * Mathf.Rad2Deg;
        // leg length (hypotenuse)
        float legLen = Mathf.Sqrt(height * height + Mathf.Pow((bottomW - topW) / 2f, 2f));

        // Left leg — bottom starts at -bottomW/2, top ends at -topW/2
        float leftMidX = -(bottomW / 2f + topW / 2f) / 2f; // midpoint x = -1.125
        var left = AddBox(parent, "LegL", mat,
            new Vector3(thickness, legLen, thickness),
            new Vector3(leftMidX, 0, 0));
        left.transform.localRotation = Quaternion.Euler(0, 0, legAngle);

        // Right leg — mirrored
        float rightMidX = (bottomW / 2f + topW / 2f) / 2f; // midpoint x = +1.125
        var right = AddBox(parent, "LegR", mat,
            new Vector3(thickness, legLen, thickness),
            new Vector3(rightMidX, 0, 0));
        right.transform.localRotation = Quaternion.Euler(0, 0, -legAngle);

        // Bottom bar (wide foot)
        AddBox(parent, "BottomBar", mat,
            new Vector3(bottomW, thickness, thickness),
            new Vector3(0, -height / 2f, 0));

        // Top bar (narrow top)
        AddBox(parent, "TopBar", mat,
            new Vector3(topW, thickness, thickness),
            new Vector3(0, height / 2f, 0));

        // Cross brace in the middle (X pattern)
        float braceW = (bottomW + topW) / 2f * 0.85f;
        var braceL = AddBox(parent, "BraceL", mat, new Vector3(thickness * 0.6f, braceW, thickness * 0.5f), new Vector3(-0.2f, 0, 0));
        braceL.transform.localRotation = Quaternion.Euler(0, 0, 50f);
        var braceR = AddBox(parent, "BraceR", mat, new Vector3(thickness * 0.6f, braceW, thickness * 0.5f), new Vector3( 0.2f, 0, 0));
        braceR.transform.localRotation = Quaternion.Euler(0, 0, -50f);
    }

    // Flat platform with decorative railing edge boxes
    static void BuildPlatform(GameObject parent, Material mat, Material dark, Vector3 size)
    {
        // Main deck
        AddBox(parent, "Deck", mat, size, Vector3.zero);
        float hw = size.x / 2f;
        float hd = size.z / 2f;
        float ry = size.y / 2f + 0.1f;
        float rt = 0.12f;
        // Railing edges
        AddBox(parent, "RailF", dark, new Vector3(size.x + rt*2, 0.2f, rt), new Vector3(0,  ry, -hd));
        AddBox(parent, "RailB", dark, new Vector3(size.x + rt*2, 0.2f, rt), new Vector3(0,  ry,  hd));
        AddBox(parent, "RailL", dark, new Vector3(rt, 0.2f, size.z),        new Vector3(-hw, ry, 0));
        AddBox(parent, "RailR", dark, new Vector3(rt, 0.2f, size.z),        new Vector3( hw, ry, 0));
    }

    // Tapered tower section using 3 stacked boxes decreasing in width
    static void BuildTapered(GameObject parent, Material mat, Material dark,
        float bottomSize, float topSize, float height)
    {
        float step = height / 3f;
        for (int i = 0; i < 3; i++)
        {
            float t    = i / 2f;
            float size = Mathf.Lerp(bottomSize, topSize, t);
            float yPos = -height / 2f + step * i + step / 2f;
            AddBox(parent, "Section" + i, mat, new Vector3(size, step * 0.9f, size), new Vector3(0, yPos, 0));

            // Cross braces on each section face
            if (size > 0.6f)
            {
                float bs = size * 0.85f;
                var b1 = AddBox(parent, "Brace" + i + "A", dark, new Vector3(bs, 0.08f, 0.08f), new Vector3(0, yPos + step * 0.2f, size / 2f));
                b1.transform.localRotation = Quaternion.Euler(35f, 0, 0);
                var b2 = AddBox(parent, "Brace" + i + "B", dark, new Vector3(bs, 0.08f, 0.08f), new Vector3(0, yPos - step * 0.2f, size / 2f));
                b2.transform.localRotation = Quaternion.Euler(-35f, 0, 0);
            }
        }
    }

    // Spire: wide base cylinder + narrow middle cylinder + thin antenna
    static void BuildSpire(GameObject parent, Material mat)
    {
        // Base knob
        AddCylinder(parent, "SpireBase", mat, new Vector3(0.5f, 0.4f, 0.5f), new Vector3(0, -1.5f, 0));
        // Main spire body
        AddCylinder(parent, "SpireBody", mat, new Vector3(0.25f, 1.2f, 0.25f), new Vector3(0, 0, 0));
        // Thin antenna
        AddCylinder(parent, "Antenna", mat, new Vector3(0.06f, 0.6f, 0.06f), new Vector3(0, 1.7f, 0));
    }

    // ── Primitive Helpers ─────────────────────────────────────────────

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

    static GameObject AddCylinder(GameObject parent, string name, Material mat, Vector3 size, Vector3 localPos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale    = size;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    static void AddJoint(GameObject piece, PuzzlePiece pp, string jointType, string compatibleType,
        Vector3 worldOffset, Material mat)
    {
        var jObj = new GameObject("Joint_" + jointType);
        jObj.transform.SetParent(piece.transform);
        jObj.transform.localPosition = worldOffset;
        jObj.transform.localRotation = Quaternion.identity;
        jObj.transform.localScale    = Vector3.one;

        var jp = jObj.AddComponent<JointPoint>();
        jp.jointType      = jointType;
        jp.compatibleType = compatibleType;
        jp.snapRadius     = 1.5f;

        // Flat disc visual — fixed world size
        var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cap.name = "JointVisual";
        cap.transform.SetParent(jObj.transform);
        cap.transform.localPosition = Vector3.zero;
        cap.transform.localScale    = new Vector3(0.35f, 0.06f, 0.35f);
        cap.GetComponent<Renderer>().sharedMaterial = mat;
        DestroyImmediate(cap.GetComponent<Collider>());

        pp.jointPoints = piece.GetComponentsInChildren<JointPoint>();
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
