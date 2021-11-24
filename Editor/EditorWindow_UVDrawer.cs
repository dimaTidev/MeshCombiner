using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace DimaTi.EditorInspector
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    public class EditorWindow_UVDrawer_editor : EditorWindow
    {
        [MenuItem("Tools/Mesh/UV Debug")]
        static void Init() => ((EditorWindow_UVDrawer_editor)EditorWindow.GetWindow(typeof(EditorWindow_UVDrawer_editor))).Show();


        MeshFilter meshFilter;
        Mesh mesh;
        Material material;

        Texture texture;

        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        int idUV;

        void OnGUI()
        {
            OnGUI_DrawOptions();

            OnGUI_DrawUV();
        }

        
        void OnGUI_DrawOptions()
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("Submesh not supported");
            EditorGUILayout.LabelField("In complex uv network, uv can shown with bugs");
            EditorGUILayout.LabelField("In oversize uv, uv can draw over the window");
            GUI.color = Color.white;

            EditorGUI.BeginChangeCheck();
            meshFilter = EditorGUILayout.ObjectField(meshFilter, typeof(MeshFilter), true) as MeshFilter;
            if (EditorGUI.EndChangeCheck())
            {
                mesh = meshFilter.sharedMesh;
                material = meshFilter.GetComponent<MeshRenderer>().sharedMaterial;
                if (material)
                    texture = material.GetTexture("_MainTex");
                if (mesh)
                {
                    mesh.GetUVs(idUV, uvs);
                    mesh.GetTriangles(tris, 0);
                }
            }



            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("idUV", GUILayout.Width(100));
            idUV = EditorGUILayout.IntField(idUV);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                if (mesh)
                {
                    mesh.GetUVs(idUV, uvs);
                    mesh.GetTriangles(tris, 0);
                }
            }
        }

        Vector2 scrollPosition;

        void OnGUI_DrawUV()
        {
            if (uvs == null || uvs.Count == 0)
                return;

            float
                maxHeight = 0,
                minHeight = 0,
                maxWidth = 0,
                minWidth = 0;

            for (int i = 0; i < uvs.Count; i++)
            {
                if (uvs[i].y > maxHeight)
                    maxHeight = uvs[i].y;
                if (uvs[i].y < minHeight)
                    minHeight = uvs[i].y;

                if (uvs[i].x > maxWidth)
                    maxWidth = uvs[i].x;
                if (uvs[i].x < minWidth)
                    minWidth = uvs[i].x;
            }

            float boxSize = 500;

            maxHeight *= boxSize;
            minHeight *= boxSize;
            maxWidth *= boxSize;
            minWidth *= boxSize;

            //scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(boxSize), GUILayout.Height(boxSize));

           // GUILayoutUtility.GetRect(0, maxHeight);

            Rect pos = GUILayoutUtility.GetRect(boxSize, boxSize, GUILayout.ExpandWidth(false)); // new Rect(0,0, 500, 500);
                                                                                                 //Vector2 startPos = pos.position;
                                                                                                 // pos.x -= minWidth;

           // Rect view = new Rect(minWidth, -maxHeight, maxWidth + Mathf.Abs(minWidth) + 100, maxHeight + Mathf.Abs(minHeight) + 100); //work in oversize UV
            Rect view = new Rect(pos.x, pos.y, maxWidth + Mathf.Abs(minWidth) + 100, maxHeight + Mathf.Abs(minHeight) + 100);


           // Debug.Log($"x: {view.x}  y: {view.y}  width: {view.width}  height: {view.height}");
           // Debug.Log($"minWidth: {minWidth}  maxWidth: {maxWidth}  minHeight: {minHeight}  maxHeight: {maxHeight}");

            scrollPosition = GUI.BeginScrollView(new Rect(pos.x, pos.y, 800, 800), scrollPosition, view);

            

            if (texture)
                EditorGUI.DrawPreviewTexture(pos, texture);

            pos.width = 2;
            pos.height = 2;

            Vector2 position = pos.position;

          //  Debug.Log("uvs:" + uvs.Count);
          //  Debug.Log("tris:" + tris.Count);

            

          //  for (int i = 0; i < uvs.Count; i++)
          //  {
          //      pos.position = uvs[i] * boxSize + position;
          //      GUI.Box(pos, "");
          //  }

            for (int i = 0; i < tris.Count; i += 3)
            {
                Handles.DrawLine(uvs[tris[i]] * boxSize + position, uvs[tris[i+1]] * boxSize + position);
                Handles.DrawLine(uvs[tris[i+1]] * boxSize + position, uvs[tris[i+2]] * boxSize + position);
            }

            GUI.EndScrollView();
        }
    }
#endif
}

