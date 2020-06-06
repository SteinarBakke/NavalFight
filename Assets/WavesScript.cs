using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavesScript : MonoBehaviour
{

    public int Dimensions = 10;
    public float UVScale = 2f;
    public Octave[] Octaves;

    protected MeshFilter Meshfilter;
    protected Mesh Mesh;
    // Start is called before the first frame update
    void Start()
    {

        //Mesh Setup
        Mesh = new Mesh();
        Mesh.name = gameObject.name;

        Mesh.vertices = GenerateVerts();
        Mesh.triangles = GenerateTries();
        Mesh.uv = GenerateUVs();
        Mesh.RecalculateBounds();
        Mesh.RecalculateNormals();

        Meshfilter = gameObject.AddComponent<MeshFilter>();
        Meshfilter.mesh = Mesh;
    }

    // THIS IS CRUCIAL TO GET HEIGHT AT ANY GIVEN POINTS
    public float GetHeight(Vector3 position)
    {
        //scale factor and position in local space
        //if ure always using scale1 this is not needed
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        //get edge points
        var p1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var p2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var p3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var p4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        //clamp if the position is outside the plane
        p1.x = Mathf.Clamp(p1.x, 0, Dimensions);
        p1.z = Mathf.Clamp(p1.z, 0, Dimensions);
        p2.x = Mathf.Clamp(p2.x, 0, Dimensions);
        p2.z = Mathf.Clamp(p2.z, 0, Dimensions);
        p3.x = Mathf.Clamp(p3.x, 0, Dimensions);
        p3.z = Mathf.Clamp(p3.z, 0, Dimensions);
        p4.x = Mathf.Clamp(p4.x, 0, Dimensions);
        p4.z = Mathf.Clamp(p4.z, 0, Dimensions);

        //get the max distance to one of the edges and take that to compute max - dist
        var max = Mathf.Max(Vector3.Distance(p1, localPos), Vector3.Distance(p2, localPos), Vector3.Distance(p3, localPos), Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        var dist = (max - Vector3.Distance(p1, localPos))
                 + (max - Vector3.Distance(p2, localPos))
                 + (max - Vector3.Distance(p3, localPos))
                 + (max - Vector3.Distance(p4, localPos) + Mathf.Epsilon);
        //weighted sum
        var height = Mesh.vertices[Index(p1.x, p1.z)].y * (max - Vector3.Distance(p1, localPos))
                   + Mesh.vertices[Index(p2.x, p2.z)].y * (max - Vector3.Distance(p2, localPos))
                   + Mesh.vertices[Index(p3.x, p3.z)].y * (max - Vector3.Distance(p3, localPos))
                   + Mesh.vertices[Index(p4.x, p4.z)].y * (max - Vector3.Distance(p4, localPos));

        //scale
        return height * transform.lossyScale.y / dist;
    }

    private int[] GenerateTries()
    {
        var tries = new int[Mesh.vertices.Length * 6];

        //Two triangles are one tile (6)
        for (int x = 0; x < Dimensions; x++)
        {
            for (int z = 0; z < Dimensions; z++)
            {
                tries[Index(x, z) * 6 + 0] = Index(x, z);//1st triangle
                tries[Index(x, z) * 6 + 1] = Index(x + 1, z + 1);
                tries[Index(x, z) * 6 + 2] = Index(x + 1, z);
                tries[Index(x, z) * 6 + 3] = Index(x, z);//2nd triangle
                tries[Index(x, z) * 6 + 4] = Index(x, z + 1);
                tries[Index(x, z) * 6 + 5] = Index(x + 1, z + 1);
            }
        }
        return tries;
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(Dimensions + 1) * (Dimensions + 1)];

        //equally distributed verts
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                verts[Index(x, z)] = new Vector3(x, 0, z);
            }
        }
        return verts;
    }

    //generating where the pixel should come from
    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[Mesh.vertices.Length];
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                // always alternate from every vector to the next
                var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2); 
                uvs[Index(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }
        return uvs;
    }


    private int Index(int x, int z)
    {
        //basically made 2 dimensional array in 1 dimension
        //x = 0, z = 0 index = 0...  if x=0 and z=9 index 9.... x=1, z=0 index = 12.
        return x * (Dimensions + 1) + z;
    }

    //same conversation for float
    private int Index(float x, float z)
    {
        return Index((int)x, (int)z);
    }

    // Update is called once per frame
    void Update()
    {
        var verts = Mesh.vertices;
        for (int x = 0; x <= Dimensions; x++)
        {
            for (int z = 0; z <= Dimensions; z++)
            {
                var y = 0f; // Height value for our vertices
                for (int o = 0; o <Octaves.Length; o++)
                {
                    if (Octaves[o].alternate) // if alternate is true
                    {
                        // adding noise (Basically it's only x  * z, but we need to scale it - CAN CHANGE 2F for more affect
                        var perl = Mathf.PerlinNoise((x * Octaves[o].scale.x) / Dimensions, (z * Octaves[o].scale.y) / Dimensions) * Mathf.PI * 2f; 
                        //this is for moving waves (BECAUSE OF PERL -> We will always have different heights based on position x and z)
                        y += Mathf.Cos(perl + Octaves[o].speed.magnitude * Time.time) * Octaves[o].height;
                    }
                    else
                    {
                        // perl is a number between 0-1, and subtract by 0.5f here to guarantee it doesn't go over 1 (can go under)
                        var perl = Mathf.PerlinNoise((x * Octaves[o].scale.x + Time.time * Octaves[o].speed.x) / Dimensions, (z * Octaves[o].scale.y + Time.time * Octaves[o].speed.y) / Dimensions) - 0.5f;
                        y += perl * Octaves[o].height;
                    }
                }
                verts[Index(x, z)] = new Vector3(x, y, z);
            }
        }
        Mesh.vertices = verts;
        Mesh.RecalculateNormals(); // have to do this everytime we update the vertices
    }


    //specify a octave - Serializable struct
    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }

}
