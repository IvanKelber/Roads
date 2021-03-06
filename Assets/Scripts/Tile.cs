﻿using System.Collections;
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
    private bool shouldLock = false;

    [SerializeField]
    private Color unlockedTint;


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

    private GameObject particles;

    [SerializeField]
    private GameObject particlesPrefab;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null) {
            Debug.LogWarning("Audio source is not attached to tile prefab");
        }
    }

    public void Initialize(float cellSize, bool isStraight, Vector2Int index, float z, Orientation startingOrientation, bool shouldLock) {
        this.cellWidth = cellSize;
        this.cellHeight = cellSize;
        this.isStraight = isStraight;
        this.z = z;
        this.currentOrienation = startingOrientation;
        this.shouldLock = shouldLock;
        Index = index;
    }

    private void Start() {
        Render(transform.position);
    }

    public void Destroy() {
        if(particles != null) {
            Destroy(this.particles);
        }
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
        if(shouldLock) {
            Lock();
        } else {
            Unlock();
        }
    }

    public void AddParticlesAsChild() {
        particles = Instantiate(particlesPrefab, transform.position, Quaternion.identity);
        particles.transform.parent = this.transform;
        ParticleSystem particleSys = particles.GetComponent<ParticleSystem>();
        if(particleSys == null) {
            Debug.LogError("Particle system can not be found.  Check prefab");
            return;
        }
        particles.transform.localScale *= cellWidth;
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

    public void Lock() {
        isLocked = true;
        this.meshRenderer.material.SetInt("_RenderOutline", 0);
        this.meshRenderer.material.SetColor("_Color", Color.white);
    }

    public void Unlock() {
        isLocked = false;
        // Debug.Log("Unlocking tile: " + Index);
        this.meshRenderer.material.SetInt("_RenderOutline", 1);
        this.meshRenderer.material.SetColor("_Color", unlockedTint);
    }

    public HashSet<TileEntry> GetEntries() {
        HashSet<TileEntry> entries = new HashSet<TileEntry>();
        if(isStraight) {
            if(currentOrienation == Orientation.Turn0 || currentOrienation == Orientation.Turn180) {
                entries.Add(TileEntry.Top);
                entries.Add(TileEntry.Bottom);
            } else {
                entries.Add(TileEntry.Left);
                entries.Add(TileEntry.Right);    
            }
        } else {
            switch(currentOrienation) {
                case Orientation.Turn0:
                    entries.Add(TileEntry.Top);
                    entries.Add(TileEntry.Left);
                    break;
                case Orientation.Turn90:
                    entries.Add(TileEntry.Left);
                    entries.Add(TileEntry.Bottom);
                    break;
                case Orientation.Turn180:
                    entries.Add(TileEntry.Bottom);
                    entries.Add(TileEntry.Right);
                    break;
                case Orientation.Turn270:
                    entries.Add(TileEntry.Right);
                    entries.Add(TileEntry.Top);
                    break;
            }
        }
        return entries;
    }

    public enum TileEntry {
        Left,
        Right,
        Top,
        Bottom
    }

}
