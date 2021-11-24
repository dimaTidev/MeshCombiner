using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

//TODO: Save texture to path

public class EditorWindow_TextureSave : EditorWindow
{
    [MenuItem("Tools/Mesh/Export data from scene")]
    static void Init() => ((EditorWindow_TextureSave)EditorWindow.GetWindow(typeof(EditorWindow_TextureSave))).Show();


    DefaultAsset folderToSave;

    Material material;

   // Texture[] textures;
    List<Texture> textures;
    List<string> textureKeys;

    MeshRenderer meshRenderer;
    Material[] materials;
    int idMaterial = 0;

    string PathToSave
    {
        get
        {
            if (!folderToSave)
                return "";
            string path = AssetDatabase.GetAssetPath(folderToSave);
            if (Path.GetExtension(path) != "")
                path = Path.GetDirectoryName(path);
            if (!path.EndsWith("/"))
                path += "/";
            return path;
        }
    }

    void OnGUI()
    {
        OnGUI_DrawOptions();

        OnGUI_DrawMaterialDetails();
    }

    void OnGUI_DrawOptions()
    {
        folderToSave = EditorGUILayout.ObjectField("folderToSave", folderToSave, typeof(DefaultAsset), true) as DefaultAsset;
        EditorGUI.BeginChangeCheck();
        meshRenderer = EditorGUILayout.ObjectField("material", material, typeof(MeshRenderer), true) as MeshRenderer;
        if (EditorGUI.EndChangeCheck())
        {
            materials = meshRenderer.sharedMaterials;
            RefreshMaterial();
        }


        if (folderToSave)
        {
            EditorGUILayout.LabelField("path to save: " + PathToSave);
        }
           

        if (materials != null && materials.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            idMaterial = EditorGUILayout.IntField("idMaterial", idMaterial);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshMaterial();
            }
        }
        else
            EditorGUILayout.LabelField("No find material on selected meshRenderer");
    }

    void RefreshMaterial()
    {
        if (idMaterial < 0)
            idMaterial = 0;
        if (idMaterial >= materials.Length)
            idMaterial = materials.Length - 1;

        material = materials[idMaterial];

        if (material)
        {
            string[] texKeys = material.GetTexturePropertyNames();


            textures = new List<Texture>();
            textureKeys = new List<string>();
           // textures = new Texture[texKeys.Length];
            for (int i = 0; i < texKeys.Length; i++)
            {
                Texture texture = material.GetTexture(texKeys[i]);
                if (texture)
                {
                    textures.Add(texture);
                    textureKeys.Add(texKeys[i]);
                }
            }
        }
    }

   

    void OnGUI_DrawMaterialDetails()
    {
        
        if (textures != null && textures.Count > 0)
        {
            float textureSize = 100;
            for (int i = 0; i < textures.Count; i++)
            {
                if (textures[i] == null)
                    continue;

                EditorGUILayout.BeginHorizontal();

                
                EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(textureSize, textureSize, GUILayout.ExpandWidth(false)), textures[i]);


                EditorGUILayout.BeginVertical();

                bool enableState = AssetDatabase.GetAssetPath(textures[i]) == "";

                GUI.color = enableState ? Color.red : Color.white;
                EditorGUILayout.ObjectField(textures[i], typeof(Texture), true);
                GUI.color = Color.white;


              //  if (AssetDatabase.GetAssetPath(textures[i]) != "")
              //      continue;

                GUI.enabled = enableState && !File.Exists(PathToSave + GetTextureNameWithExtension(textures[i]));

                if (GUILayout.Button("Save texture", GUILayout.ExpandWidth(false)))
                    SaveTexture(textures[i], PathToSave);

                if (GUILayout.Button("Save and replace texture in material", GUILayout.ExpandWidth(false)))
                {
                    SaveTexture(textures[i], PathToSave);
                    if(File.Exists(PathToSave + GetTextureNameWithExtension(textures[i])))
                    {
                        textures[i] = ReplaceTexture(PathToSave + GetTextureNameWithExtension(textures[i]));
                        ApplyTextures(ref material, ref textures, ref textureKeys);
                    } else
                        Debug.LogError("File not exist in path: " + PathToSave + GetTextureNameWithExtension(textures[i]));
                }

                GUI.enabled = enableState && File.Exists(PathToSave + GetTextureNameWithExtension(textures[i]));

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Replace texture in material", GUILayout.ExpandWidth(false)))
                {
                    textures[i] = ReplaceTexture(PathToSave + GetTextureNameWithExtension(textures[i]));
                    ApplyTextures(ref material, ref textures, ref textureKeys);
                }

                if(File.Exists(PathToSave + GetTextureNameWithExtension(textures[i])))
                    EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<Texture>(PathToSave + GetTextureNameWithExtension(textures[i])), typeof(Texture), true);

                EditorGUILayout.EndHorizontal();

                GUI.enabled = true;


                

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
        else
            EditorGUILayout.LabelField("No textures to show");
    }


    string GetTextureNameWithExtension(Texture texture)
    {
        if (!texture)
            return "";
        return (texture.name == "" ? Random.Range(0, 10000).ToString() : texture.name) + ".png";
    }

    Texture ReplaceTexture(string pathToNewTexture)
    {
        return (Texture)AssetDatabase.LoadAssetAtPath(pathToNewTexture, typeof(Texture));
    }

    void ApplyTextures(ref Material material, ref List<Texture> textures, ref List<string> textureKeys)
    {
        if (material == null || textureKeys == null || textures == null || textures.Count != textureKeys.Count)
            return;

        for (int i = 0; i < textureKeys.Count; i++)
            material.SetTexture(textureKeys[i], textures[i]);
    }

    void SaveTexture(Texture texture, string path)
    {
        if (!texture)
        {
            Debug.LogError("Texture not exist");
            return;
        }
        if (path == "")
        {
            Debug.LogError("Path not exist");
            return;
        }

        string fullpath = PathToSave;
        fullpath += GetTextureNameWithExtension(texture);

        Texture2D texture2d = (Texture2D)texture;

        if (texture2d)
        {
            File.WriteAllBytes(fullpath, texture2d.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log("Save texture done at path: " + fullpath);
        }  
        else
            Debug.LogError("Texture2D not exist");
    }
}
