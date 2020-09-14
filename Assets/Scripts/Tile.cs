using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using TileRotation;

public class Tile : MonoBehaviour
{

    [SerializeField]
    private Texture2D[] textures;

    private bool isStraight = true;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;    
    private float cellWidth = 1;
    private float cellHeight = 1;
    private float z = 1;

    private BoxCollider collider;

    [SerializeField]
    private Vector2GameEvent onTapped;

    private Orientation currentOrienation;
    public Orientation Orientation {
        get {
            return currentOrienation;
        }
        set {
            currentOrienation = value;
            ReOrient();
        }
    }

    private bool isLocked = false;
    public bool IsLocked {
        get {
            return isLocked;
        }
    }

    public Vector3 Position {
        get {
            return transform.position;
        }
        set {
            transform.position = value;
        }
    }

    private Vector2Int index;
    public Vector2Int Index {
        get {
            return index;
        }
        set {
            index = value;
        }
    }

    public Material material;

    [HideInInspector]
    public AudioSource audioSource;

    private void Awake()
    {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        collider = gameObject.AddComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) {
            Debug.LogWarning("Audio source is not attached to tile prefab");
        }
    }

    public void Initialize(float cellSize, bool isStraight, Vector2Int index, float z, Orientation startingOrientation) {
        this.cellWidth = cellSize;
        this.cellHeight = cellSize;
        this.isStraight = isStraight;
        this.z = z;
        this.currentOrienation = startingOrientation;
        Index = index;
    }

    private void Start() {
        Render(transform.position);
        collider.size = new Vector3(cellWidth, cellHeight, 1);
    }

    public void Destroy() {
        Destroy(this.gameObject);
    }

    public void Render(Vector3 position)
    {
        meshRenderer.sharedMaterial = material;
        meshRenderer.material.SetTexture("_MainTex", isStraight ? textures[1] : textures[0]);
        meshFilter.mesh = new Mesh();

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        mesh.vertices = GetVertices(position);
        mesh.triangles = GetTriangles();
        Vector3[] normals = new Vector3[4]
        {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
        };
        mesh.uv = uv;
        ReOrient();
    }

    private void ReOrient() {
        this.transform.rotation = RotationHelper.GetRotation(currentOrienation);
    }

    private Vector3[] GetVertices(Vector3 position)
    {
        Vector3[] vertices = new Vector3[4]
        {
                transform.InverseTransformPoint(new Vector3(position.x - cellWidth/2, position.y - cellHeight/2, z)),
                transform.InverseTransformPoint(new Vector3(position.x + cellWidth/2, position.y - cellHeight/2, z)),
                transform.InverseTransformPoint(new Vector3(position.x - cellWidth/2, position.y + cellHeight/2, z)),
                transform.InverseTransformPoint(new Vector3(position.x + cellWidth/2, position.y + cellHeight/2, z))
        };
        return vertices;
    }

    private int[] GetTriangles()
    {
        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        return tris;
    }

    private void OnMouseDown() {
        // onTapped.Raise(new Vector2(Index.x, Index.y));
        Lock();
    }

    private void Lock() {
        isLocked = true;
        meshRenderer.material.SetColor("_MainColor", new Color(1,1,1,.5f));
    }
}
