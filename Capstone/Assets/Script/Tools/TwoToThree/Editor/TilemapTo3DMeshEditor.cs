using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Editor window: Tilemap → 3D Mesh Extruder
/// Open via  Tools > Tilemap To 3D Mesh
/// </summary>
public class TilemapTo3DMeshEditor : EditorWindow
{
    // ── Serialised fields (persisted across domain reloads) ───────────────
    private Tilemap _tilemap;
    private float   _depth          = 1f;
    private string  _prefabSavePath = "Assets/GeneratedMeshes";
    private string  _meshName       = "ExtrudedTilemapMesh";

    // ── UI state ──────────────────────────────────────────────────────────
    private bool    _showPreview;
    private Mesh    _previewMesh;
    private string  _statusMsg      = "";
    private bool    _statusIsError;

    private GUIStyle _headerStyle;
    private GUIStyle _statusStyle;

    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Tilemap To 3D Mesh")]
    public static void ShowWindow()
        => GetWindow<TilemapTo3DMeshEditor>("Tilemap → 3D Mesh");

    // ─────────────────────────────────────────────────────────────────────
    private void OnEnable()  => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyPreviewMesh();
    }

    // ─────────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        InitStyles();

        // ── Header ──────────────────────────────────────────────────────
        GUILayout.Space(8);
        GUILayout.Label("Tilemap → 3D Mesh Extruder", _headerStyle);
        DrawSeparator();

        // ── Source ──────────────────────────────────────────────────────
        GUILayout.Label("Source", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _tilemap = (Tilemap)EditorGUILayout.ObjectField(
            "Tilemap", _tilemap, typeof(Tilemap), true);
        if (EditorGUI.EndChangeCheck()) InvalidatePreview();

        GUILayout.Space(4);

        // ── Settings ────────────────────────────────────────────────────
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        _depth = EditorGUILayout.FloatField("Extrusion Depth (Z)", _depth);
        if (_depth <= 0f) _depth = 0.01f;
        if (EditorGUI.EndChangeCheck()) InvalidatePreview();

        _meshName       = EditorGUILayout.TextField("Asset Name",    _meshName);
        _prefabSavePath = EditorGUILayout.TextField("Save Directory", _prefabSavePath);

        GUILayout.Space(4);

        // ── Preview toggle ───────────────────────────────────────────────
        EditorGUI.BeginChangeCheck();
        _showPreview = EditorGUILayout.Toggle("Preview in Scene", _showPreview);
        if (EditorGUI.EndChangeCheck())
        {
            if (_showPreview) RefreshPreview();
            else              DestroyPreviewMesh();
            SceneView.RepaintAll();
        }

        DrawSeparator();

        // ── Generate button ──────────────────────────────────────────────
        GUI.enabled = _tilemap != null;
        if (GUILayout.Button("Generate & Save Prefab", GUILayout.Height(36)))
            GenerateAndSave();
        GUI.enabled = true;

        // ── Status ───────────────────────────────────────────────────────
        if (!string.IsNullOrEmpty(_statusMsg))
        {
            GUILayout.Space(6);
            EditorGUILayout.HelpBox(_statusMsg,
                _statusIsError ? MessageType.Error : MessageType.Info);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Generate + save
    // ─────────────────────────────────────────────────────────────────────
    private void GenerateAndSave()
    {
        _statusMsg     = "";
        _statusIsError = false;

        if (_tilemap == null)
        {
            SetStatus("Please assign a Tilemap.", true);
            return;
        }

        Mesh mesh = TilemapMeshBuilder.Build(_tilemap, _depth);
        if (mesh == null)
        {
            SetStatus("Tilemap appears to be empty — no tiles found.", true);
            return;
        }

        // ── Make sure directory exists ───────────────────────────────────
        string dir = _prefabSavePath.TrimEnd('/');
        if (!AssetDatabase.IsValidFolder(dir))
        {
            CreateFolderRecursive(dir);
        }

        // ── Save Mesh asset ──────────────────────────────────────────────
        string meshPath = $"{dir}/{_meshName}.mesh";
        AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(meshPath));

        // ── Build Prefab hierarchy ───────────────────────────────────────
        var root = new GameObject(_meshName);

        var mf = root.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = root.AddComponent<MeshRenderer>();
        mr.sharedMaterial = GetDefaultMaterial();

        // ── Save Prefab ──────────────────────────────────────────────────
        string prefabPath = $"{dir}/{_meshName}.prefab";
        prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

        bool success;
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath, out success);
        DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (success)
        {
            SetStatus($"Saved!\nMesh   → {meshPath}\nPrefab → {prefabPath}", false);
            // Ping in Project window
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        }
        else
        {
            SetStatus("Prefab save failed — check the console for details.", true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Scene preview (drawn via Handles)
    // ─────────────────────────────────────────────────────────────────────
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!_showPreview || _previewMesh == null) return;

        var wireMat = GetWireframeMaterial();
        if (wireMat == null) return;

        wireMat.SetPass(0);
        Graphics.DrawMeshNow(_previewMesh, Vector3.zero, Quaternion.identity);
    }

    private static Material _wireMat;
    private static Material GetWireframeMaterial()
    {
        if (_wireMat != null) return _wireMat;
        var shader = Shader.Find("Hidden/Internal-Colored");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) return null;
        _wireMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        _wireMat.SetColor("_Color", new Color(0.2f, 0.8f, 1f, 0.6f));
        _wireMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _wireMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _wireMat.SetInt("_Cull",     (int)UnityEngine.Rendering.CullMode.Off);
        _wireMat.SetInt("_ZWrite",   0);
        return _wireMat;
    }

    private void RefreshPreview()
    {
        DestroyPreviewMesh();
        if (_tilemap == null) return;
        _previewMesh = TilemapMeshBuilder.Build(_tilemap, _depth);
    }

    private void InvalidatePreview()
    {
        if (_showPreview) RefreshPreview();
    }

    private void DestroyPreviewMesh()
    {
        if (_previewMesh != null)
        {
            DestroyImmediate(_previewMesh);
            _previewMesh = null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────
    private static Material GetDefaultMaterial()
    {
        // Use URP Lit if available, otherwise the built-in default
        var mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
        return mat;
    }

    private static void CreateFolderRecursive(string path)
    {
        string[] parts = path.Split('/');
        string   cur   = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
    }

    private void SetStatus(string msg, bool isError)
    {
        _statusMsg     = msg;
        _statusIsError = isError;
        Repaint();
    }

    private void InitStyles()
    {
        if (_headerStyle != null) return;
        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 14,
            alignment = TextAnchor.MiddleCenter
        };
    }

    private static void DrawSeparator()
    {
        GUILayout.Space(4);
        var rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        GUILayout.Space(4);
    }
}
