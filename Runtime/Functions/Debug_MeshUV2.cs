using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class Debug_MeshUV2 : MonoBehaviour
{
    public bool isDraw = false;
    public int idUV = 1;
}



namespace DimaTi.EditorInspector
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Debug_MeshUV2))]
    public class Editor_Debug_MeshUV2_editor : Editor
    {
        Debug_MeshUV2 scr;
        Mesh mesh;
        Vector3[] vertices;
        List<Vector3> uvs = new List<Vector3>();

        void OnEnable()
        {
            scr = target as Debug_MeshUV2;

            if (scr)
            {
                mesh = scr.GetComponent<MeshFilter>().sharedMesh;
                vertices = mesh.vertices;
                mesh.GetUVs(scr.idUV, uvs);
            }
                
        }

        void OnSceneGUI()
        {
            if (scr == null || mesh == null || !scr.isDraw)
                return;

            mesh.GetUVs(scr.idUV, uvs);

           for (int i = 0; i < vertices.Length; i++)
               Handles.Label(scr.transform.position + vertices[i], uvs[i].ToString());
           


          //  Handles.color = Color.blue;
          //  Handles.Label(scr.transform.position + Vector3.up * 2,
          //      scr.transform.position.ToString() + "\nShieldArea: " +
          //      scr.shieldArea.ToString());
          //
          //  Handles.BeginGUI();
          //  if (GUILayout.Button("Reset Area", GUILayout.Width(100)))
          //  {
          //      scr.shieldArea = 5;
          //  }
          //  Handles.EndGUI();
          //
          //
          //  Handles.DrawWireArc(scr.transform.position,
          //      scr.transform.up,
          //      -scr.transform.right,
          //      180,
          //      scr.shieldArea);
          //  scr.shieldArea =
          //      Handles.ScaleValueHandle(scr.shieldArea,
          //          scr.transform.position + scr.transform.forward * scr.shieldArea,
          //          scr.transform.rotation,
          //          1,
          //          Handles.ConeHandleCap,
          //          1);
        }
    }
#endif
}

