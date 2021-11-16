using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class MeshSquarePlacer : MonoBehaviour
{
    [SerializeField] float offset = 15;


    public void CreateSquareCount()
    {
        if(transform.childCount == 0 || transform.childCount > 1)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogError($"Child count must be 1 ! Current is {transform.childCount}");
#endif
            return;
        }

        GameObject prefab = transform.GetChild(0).gameObject;

        for (int i = 0; i < 7; i++)
            Instantiate(prefab, transform);
    }

    public void PlaceChildsBySquare()
    {
        int id = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Transform child = transform.GetChild(id);
                if (!child) break;
                child.localPosition = new Vector3(y * offset, 0, i * offset);
                id++;
            }
        }
    }
}


namespace DimaTi.EditorInspector
{
#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeshSquarePlacer))]
    public class MeshSquarePlacer_editor : Editor
    {
        MeshSquarePlacer scr;

        void OnEnable()
        {
            scr = target as MeshSquarePlacer;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.BeginVertical("Helpbox");

            if (GUILayout.Button("Copy/Paste by square"))
                scr.CreateSquareCount();

            if (GUILayout.Button("Place Childs by Square"))
                scr.PlaceChildsBySquare();
            
            GUILayout.EndVertical();
        }
    }
#endif
}

