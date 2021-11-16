using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimaTi.Baking
{
    [RequireComponent(typeof(MeshFilter))]
    public class BakeLocalPositionToColor : MonoBehaviour
    {
        void Awake() => BakeLocalPosToColor();

        void BakeLocalPosToColor()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            Mesh sharedMesh = mf.sharedMesh;


            Vector3[] vertices = sharedMesh.vertices;
            int[] triangles = sharedMesh.triangles;
            Vector3[] normals = sharedMesh.normals;
            Vector2[] uv = sharedMesh.uv;
            //Bounds bounds = sharedMesh.bounds;

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.uv = uv;
            mesh.RecalculateBounds();

       

            Color[] colors = new Color[vertices.Length];


            float highestVertice = vertices[0].y;
            float lowestVertice = vertices[0].y;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y > highestVertice)
                    highestVertice = vertices[i].y;

                if (vertices[i].y < lowestVertice)
                    lowestVertice = vertices[i].y;
            }

        
            for (int i = 0; i < colors.Length; i++)
                //colors[i] = new Color(bounds.center.x, bounds.center.z, Mathf.InverseLerp(lowestVertice, highestVertice, vertices[i].y), 0); 
                colors[i] = new Color(vertices[i].x, vertices[i].z, Mathf.InverseLerp(lowestVertice, highestVertice, vertices[i].y), vertices[i].y);

            mesh.colors = colors;

            List<Vector3> uv2 = new List<Vector3>();

            for (int i = 0; i < vertices.Length; i++)
                uv2.Add(new Vector3(vertices[i].x, vertices[i].y, vertices[i].z));

            mesh.SetUVs(1, uv2);

            mf.mesh = mesh;
        }

    }
}
