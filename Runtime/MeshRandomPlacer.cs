using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class MeshRandomPlacer : MonoBehaviour
{
    [SerializeField] GameObject prefab;

    [SerializeField] int count;
    [SerializeField] Vector2 arrea = new Vector2(10, 10);
    [SerializeField] Vector2 scale_randomExtremums = new Vector2(0.25f, 0.5f);

    public void Random_Place()
    {
        if (!prefab) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere;
            randomPos.x *= arrea.x;
            randomPos.y = 0;
            randomPos.z *= arrea.y;

            randomPos += transform.position;

            GameObject go = Instantiate(prefab, randomPos, Quaternion.identity, transform);
           // MeshFilter mf = go.GetComponent<MeshFilter>();
           // if (mf)
           // {
           //     Mesh mesh = mf.sharedMesh.Clone();
           //     Color[] colors = new Color[mesh.vertexCount];
           //     for (int k = 0; k < colors.Length; k++)
           //         colors[k] = Random.ColorHSV();
           //     mesh.colors = colors;
           //     mf.mesh = mesh;
           // }
        }
    }

    public void Random_Color()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshFilters.Length; i++)
        {
            Color color = Random.ColorHSV();
            Mesh mesh = meshFilters[i].sharedMesh.Clone();
            Color[] colors = new Color[mesh.vertexCount];
            for (int k = 0; k < colors.Length; k++)
                colors[k] = color;
            mesh.colors = colors;
            meshFilters[i].mesh = mesh;
        }
    }

    public void Random_Scale()
    {
        
        for (int i = 0; i < transform.childCount; i++)
        {
            float random = Random.Range(scale_randomExtremums.x, scale_randomExtremums.y);
            transform.GetChild(i).localScale = new Vector3(random, random, random);
        }
    }


}



namespace DimaTi.EditorInspector
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeshRandomPlacer))]
    public class MeshRandomPlacer_editor : Editor
    {
        MeshRandomPlacer scr;

        void OnEnable()
        {
            scr = target as MeshRandomPlacer;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.BeginVertical("Helpbox");
            if (GUILayout.Button("Create Random"))
                scr.Random_Place();

            if (GUILayout.Button("Random Color"))
                scr.Random_Color();

            if (GUILayout.Button("Random scale"))
                scr.Random_Scale();

            GUILayout.EndVertical();
        }
    }
#endif
}
