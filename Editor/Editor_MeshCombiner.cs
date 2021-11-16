using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace DimaTi.Baking
{
    
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MeshCombiner))]
    public class MeshCombiner_editor : Editor
    {
        MeshCombiner scr;
        private DefaultAsset folderSaveMesh = null;
        private DefaultAsset folderSaveTextureAtlas = null;

        //  int idHeader = 0;

        void OnEnable()
        {
            scr = target as MeshCombiner;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.BeginVertical("Helpbox");

            OnGUI_GameObject();
            OnGUI_MeshEdit();
            OnGUI_Clears();
            OnGUI_MeshEdit_onPlayMode();
            OnGUI_SaveMesh();

            GUILayout.EndVertical();
        }


        void OnGUI_GameObject()
        {
            GUILayout.BeginVertical("Helpbox");
            GUILayout.Label("GameObject Tools");
            if (GUILayout.Button("Unparent Childs"))
                scr.UnparentChilds();

            if (GUILayout.Button("Extract Simple Colliders"))
                scr.ExtractSimpleColliders();

            if (GUILayout.Button("ActiveDeactive Childs"))
                scr.ActiveDeactiveChilds();
            GUILayout.EndVertical();
        }

        void OnGUI_Clears()
        {
            GUI.color = Color.red;
            GUILayout.BeginVertical("Helpbox");
            if (GUILayout.Button("Clear childs"))
                ClearChilds();

            GUI.color = Color.white;
            GUI.enabled = true;

            GUI.enabled = scr.combainInstances != null && scr.combainInstances.Count > 0;
            if (GUILayout.Button("Destroy Mesh"))
                ClearCombines();
            GUI.enabled = true;
            GUILayout.EndVertical();
        }

        void OnGUI_MeshEdit()
        {
            GUILayout.BeginVertical("Helpbox");
            GUILayout.Label("Mesh Tools");

            //  EditorGUILayout.BeginHorizontal();
            //  EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Child count = " + scr.transform.childCount);
            GUI.enabled = scr.transform.childCount > 0;

            GUI.color = Color.green;
            if (GUILayout.Button("Combine"))
                scr.Combine();

            if (GUILayout.Button("Combine sorting materials"))
                scr.CombineSorting();
            // EditorGUILayout.EndVertical();

            // EditorGUILayout.BeginVertical();


            GUI.color = Color.yellow;
            if (GUILayout.Button("Separate submeshes"))
                scr.UnCombineSubmesh();

            if (GUILayout.Button("Combine with localPos Vector2"))
                scr.Combine(1);
            if (GUILayout.Button("Combine with localPos Vector3"))
                scr.Combine(2);

            GUI.color = Color.white;
            GUILayout.EndVertical();
        }


        void OnGUI_MeshEdit_onPlayMode()
        {
            GUILayout.BeginVertical("Helpbox");
            GUILayout.Label("Runtime Tools");
            //  GUI.color = Color.cyan;
            // if(!folderSaveTextureAtlas)
            folderSaveTextureAtlas = (DefaultAsset)EditorGUILayout.ObjectField("Texture atlas folder Output", folderSaveTextureAtlas, typeof(DefaultAsset), false);

            GUI.enabled = folderSaveTextureAtlas;
            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("Combine by Texture Atlassing"))
                SaveTextureAtlas(scr.CombineByAtlassing());

            if (GUILayout.Button("Combine by Texture Atlassing Only Colors"))
                SaveTextureAtlas(scr.CombineByAtlasingOnlyColors());

            GUI.enabled = true;
            GUILayout.EndVertical();
        }

        void OnGUI_SaveMesh()
        {
            GUILayout.BeginVertical("Helpbox");
            GUILayout.Label("Save Tools");
            EditorGUI.BeginChangeCheck();
            folderSaveMesh = (DefaultAsset)EditorGUILayout.ObjectField("Select output Folder", folderSaveMesh, typeof(DefaultAsset), false);
            if (EditorGUI.EndChangeCheck())
                Debug.Log("AssetDatabase.GetAssetPath(folderSaveMesh) = " + AssetDatabase.GetAssetPath(folderSaveMesh));

            GUI.enabled = folderSaveMesh;
            if (GUILayout.Button("Save all child meshes"))
            {
                MeshFilter[] meshFilters = scr.GetComponentsInChildren<MeshFilter>();
                for (int i = 0; i < meshFilters.Length; i++)
                    SaveMeshAsset(meshFilters[i]);
            }

            GUI.enabled = true;
            GUILayout.EndVertical();
        }

        void SaveMeshAsset(MeshFilter mf)
        {
            if (mf)
            {
                string fullpath = AssetDatabase.GetAssetPath(folderSaveMesh);
                if (!fullpath.EndsWith("/")) fullpath += "/";
                fullpath += mf.sharedMesh.name + ".asset";

                if (!AssetDatabase.Contains(mf.sharedMesh))
                    AssetDatabase.CreateAsset(mf.sharedMesh, fullpath);
                Debug.Log("Save successfull at path : " + fullpath);
            }
            else
                Debug.Log("No mesh filter on this gameobject");
        }

        void ClearCombines()
        {
            if (scr.combainInstances == null) return;
            for (int i = 0; i < scr.combainInstances.Count; i++)
            {
                if (scr.combainInstances[i] == null) continue;
                if (scr.combainInstances[i] == scr.gameObject)
                {
                    Undo.DestroyObjectImmediate(scr.gameObject.GetComponent<MeshFilter>());
                    Undo.DestroyObjectImmediate(scr.gameObject.GetComponent<MeshRenderer>());
                }
                else
                    Undo.DestroyObjectImmediate(scr.combainInstances[i]);
            }
            scr.combainInstances.Clear();
            Resources.UnloadUnusedAssets();
        }

        void ClearChilds()
        {
            GameObject[] arrayToDestroy = new GameObject[scr.transform.childCount];
            GameObject temp;
            for (int i = 0; i < arrayToDestroy.Length; i++)
            {
                temp = scr.transform.GetChild(i).gameObject;
                if (scr.combainInstances == null || !scr.combainInstances.Contains(temp))
                    arrayToDestroy[i] = temp;
            }

            for (int i = 0; i < arrayToDestroy.Length; i++)
            {
                if (arrayToDestroy[i] != null)
                    Undo.DestroyObjectImmediate(arrayToDestroy[i]);
                // DestroyImmediate(arrayToDestroy[i]);
            }
            //while (scr.transform.childCount > 0)
            //    DestroyImmediate(scr.transform.GetChild(0).gameObject);
        }

        void SaveTextureAtlas(Texture2D texture)
        {
            if (!texture || !folderSaveTextureAtlas)
            {
                Debug.Log("No texture or folder");
                return;
            }
            string fullpath = AssetDatabase.GetAssetPath(folderSaveTextureAtlas);
            if (!fullpath.EndsWith("/")) fullpath += "/";
            fullpath += texture.name + ".png";

            File.WriteAllBytes(fullpath, texture.EncodeToPNG());

            Debug.Log("Save Texture complited");
        }
    }
    
  /*  
    [CanEditMultipleObjects]
[CustomEditor(typeof(MeshCombiner))]
public class MeshCombiner_editor : Editor
{
    MeshCombiner scr;
    private DefaultAsset folderSaveMesh = null;
    private DefaultAsset folderSaveTextureAtlas = null;

    //  int idHeader = 0;

    void OnEnable()
    {
        scr = target as MeshCombiner;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.BeginVertical("Helpbox");



        OnGUI_GameObject();
        OnGUI_MeshEdit();
        OnGUI_SaveMesh();

        folderSaveTextureAtlas = (DefaultAsset)EditorGUILayout.ObjectField("Texture atlas folder Output", folderSaveTextureAtlas, typeof(DefaultAsset), false);

    GUILayout.EndVertical();
    }


void OnGUI_GameObject()
{
    GUILayout.BeginVertical("Helpbox");
    GUILayout.Label("GameObject Tools");
    if (GUILayout.Button("Unparent Childs"))
        scr.UnparentChilds();

    if (GUILayout.Button("Extract Simple Colliders"))
        scr.ExtractSimpleColliders();

    if (GUILayout.Button("ActiveDeactive Childs"))
        scr.ActiveDeactiveChilds();
    GUILayout.EndVertical();
}

void OnGUI_MeshEdit()
{
    GUILayout.BeginVertical("Helpbox");
    GUILayout.Label("Mesh Tools");
    EditorGUILayout.LabelField("Child count = " + scr.transform.childCount);
    GUI.enabled = scr.transform.childCount > 0;

    GUI.color = Color.green;
    if (GUILayout.Button("Combine"))
        scr.Combine();

    if (GUILayout.Button("Combine with localPos Vector2"))
        scr.Combine(1);
    if (GUILayout.Button("Combine with localPos Vector3"))
        scr.Combine(2);

    if (GUILayout.Button("Combine sorting materials"))
        scr.CombineSorting();

    GUI.color = Color.cyan;
    // if(!folderSaveTextureAtlas)
    folderSaveTextureAtlas = (DefaultAsset)EditorGUILayout.ObjectField("Texture atlas folder Output", folderSaveTextureAtlas, typeof(DefaultAsset), false);

    GUI.enabled = folderSaveTextureAtlas;
    GUI.enabled = Application.isPlaying;

    if (GUILayout.Button("Combine by Texture Atlassing"))
        SaveTextureAtlas(scr.CombineByAtlassing());

    if (GUILayout.Button("Combine by Texture Atlassing Only Colors"))
        SaveTextureAtlas(scr.CombineByAtlasingOnlyColors());

    GUI.enabled = true;

    GUI.color = Color.yellow;
    if (GUILayout.Button("Separate submeshes"))
        scr.UnCombineSubmesh();

    GUI.color = Color.red;
    if (GUILayout.Button("Clear childs"))
        ClearChilds();

    GUI.color = Color.white;
    GUI.enabled = true;

    GUI.enabled = scr.combainInstances != null && scr.combainInstances.Count > 0;
    if (GUILayout.Button("Destroy Mesh"))
        ClearCombines();
    GUILayout.EndVertical();
}

void OnGUI_SaveMesh()
{
    GUILayout.BeginVertical("Helpbox");
    GUILayout.Label("Save Tools");
    EditorGUI.BeginChangeCheck();
    folderSaveMesh = (DefaultAsset)EditorGUILayout.ObjectField("Select Folder with 'Cross.asset'", folderSaveMesh, typeof(DefaultAsset), false);
    if (EditorGUI.EndChangeCheck())
        Debug.Log("AssetDatabase.GetAssetPath(folderSaveMesh) = " + AssetDatabase.GetAssetPath(folderSaveMesh));

    GUI.enabled = folderSaveMesh;
    if (GUILayout.Button("Save all child meshes"))
    {
        MeshFilter[] meshFilters = scr.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshFilters.Length; i++)
            SaveMeshAsset(meshFilters[i]);
    }

    GUI.enabled = true;
    GUILayout.EndVertical();
}

void SaveMeshAsset(MeshFilter mf)
{
    if (mf)
    {
        string fullpath = AssetDatabase.GetAssetPath(folderSaveMesh);
        if (!fullpath.EndsWith("/")) fullpath += "/";
        fullpath += mf.sharedMesh.name + ".asset";

        if (!AssetDatabase.Contains(mf.sharedMesh))
            AssetDatabase.CreateAsset(mf.sharedMesh, fullpath);
        Debug.Log("Save successfull at path : " + fullpath);
    }
    else
        Debug.Log("No mesh filter on this gameobject");
}

void ClearCombines()
{
    if (scr.combainInstances == null) return;
    for (int i = 0; i < scr.combainInstances.Count; i++)
    {
        if (scr.combainInstances[i] == null) continue;
        if (scr.combainInstances[i] == scr.gameObject)
        {
            DestroyImmediate(scr.gameObject.GetComponent<MeshFilter>());
            DestroyImmediate(scr.gameObject.GetComponent<MeshRenderer>());
        }
        else
            DestroyImmediate(scr.combainInstances[i]);
    }
    scr.combainInstances.Clear();
    Resources.UnloadUnusedAssets();
}

void ClearChilds()
{
    GameObject[] arrayToDestroy = new GameObject[scr.transform.childCount];
    GameObject temp;
    for (int i = 0; i < arrayToDestroy.Length; i++)
    {
        temp = scr.transform.GetChild(i).gameObject;
        if (scr.combainInstances == null || !scr.combainInstances.Contains(temp))
            arrayToDestroy[i] = temp;
    }

    for (int i = 0; i < arrayToDestroy.Length; i++)
    {
        if (arrayToDestroy[i] != null)
            DestroyImmediate(arrayToDestroy[i]);
    }
   //while (scr.transform.childCount > 0)
   //     DestroyImmediate(scr.transform.GetChild(0).gameObject);
}

void SaveTextureAtlas(Texture2D texture)
{
    if (!texture || !folderSaveTextureAtlas)
    {
        Debug.Log("No texture or folder");
        return;
    }
    string fullpath = AssetDatabase.GetAssetPath(folderSaveTextureAtlas);
    if (!fullpath.EndsWith("/")) fullpath += "/";
    fullpath += texture.name + ".png";

    File.WriteAllBytes(fullpath, texture.EncodeToPNG());

    Debug.Log("Save Texture complited");
}
}
    */
}