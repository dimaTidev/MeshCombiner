using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_NormalCheck : MonoBehaviour
{
    public Transform go;
    public Transform cam;
    public MeshFilter mf;

    public float value;

    public float size = 0.001f;
    public float scale = 1;
    public int iterCount = 30;

    void Start()
    {
        for (int i = 0; i < 12; i++)
        {
            Debug.Log($"Tan[{i/2f}]:" + Mathf.Tan(Mathf.Rad2Deg * (i / 2f)));
        }
    }
    
    void Update()
    {
        value = Vector3.Dot(go.forward, cam.forward);
    }

    public float[] values;
    public float[] values2;

    private void OnDrawGizmosSelected()
    {
        if (go)
            Gizmos.DrawRay(go.position, go.forward);

        if (cam)
            Gizmos.DrawRay(cam.position, cam.forward);

        if (mf)
        {
            Mesh mesh = mf.sharedMesh;

            Vector3[] vrt = mesh.vertices;
            Vector3[] norm = mesh.normals;

            for (int i = 0; i < vrt.Length; i++)
            {
                Gizmos.DrawRay(vrt[i], norm[i]);
            }
        }

        Vector3 lastPos = Vector3.zero;
      
        values = new float[iterCount];
        values2 = new float[iterCount];
      
        for (int i = 0; i < iterCount; i++)
        {
            values2[i] = (1.57f / (float)(iterCount - 1) * i);
            values[i] = 1 - Mathf.Atan(values2[i]);
            Vector3 pos = Vector3.zero + Vector3.up * values[i] * scale + Vector3.right * values[i] * scale;
      
            Gizmos.DrawSphere(pos, size);
            Gizmos.DrawLine(pos, lastPos);
            lastPos = pos;
        }
    }
}
