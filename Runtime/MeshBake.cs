using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DimaTi.MeshBaking
{
    //TODO: optimize - List.ToArray();
    public class MeshBake : MonoBehaviour
    {
        public enum BakeType
        {
            Combine = 0, 
            Combine_SortByMaterial = 1,
            Combine_SortByShaderWithAtlasing = 2,
        }

        [SerializeField] bool isEnableBake_inStart = false;
        [SerializeField] BakeType bakeType = BakeType.Combine;
        [SerializeField] AMeshBakeOption[] bakeOptions = null;

        List<GameObject> bakedInstancies = new List<GameObject>();

        [SerializeField] List<MaterialAtlasBakeSetup> atlasBakeSetup = new List<MaterialAtlasBakeSetup>();

        [System.Serializable]
        public class MaterialAtlasBakeSetup
        {
            public Shader shader;
            public TextureData[] textureData;


            public MaterialAtlasBakeSetup(int textureSize)
            {
                shader = null;
                textureData = new TextureData[]
                {
                    new TextureData("_MainTex", 0, textureSize)
                };
            }

            [System.Serializable]
            public class TextureData
            {
                public string textureKey = "_MainTex";
                [Header("-1 if uvMap is equals in index `0`")]
                public int idUv = 0;
                public int maxTextureSize = 1024;
                //public int isTheSameIn = -1;

                [HideInInspector, System.NonSerialized] public Rect[] packRect = new Rect[0];

                public TextureData(string textureKey, int idUv, int maxTextureSize)
                {
                    this.textureKey = textureKey;
                    this.idUv = idUv;
                    this.maxTextureSize = maxTextureSize;
                }
            }
        }

        private void Start()
        {
            if (!isEnableBake_inStart)
                return;

            InvokeBake();
        }

        public void InvokeBake() => InvokeBake(gameObject, null);
        public void InvokeBake(GameObject root, MeshFilter[] meshFilters)
        {
            if (bakeType == BakeType.Combine)
                Bake(root, meshFilters);
            else if (bakeType == BakeType.Combine_SortByMaterial)
                Combine_WithSortByMaterial(root, meshFilters);
            else if (bakeType == BakeType.Combine_SortByShaderWithAtlasing)
                Combine_SortByShaderAndAtlasing(root, meshFilters);
        }


        #region Bakes ContextMenu
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        [ContextMenu("Simple combine")]
        void Bake() => Bake(gameObject, null);
        [ContextMenu("Combine with sort material")]
        void Combine_WithSortByMaterial() => Combine_WithSortByMaterial(gameObject, null);
        [ContextMenu("Combine Atlasing sort by shader")]
        void Combine_SortByShaderAndAtlasing() => Combine_SortByShaderAndAtlasing(gameObject, null);
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #endregion

        #region Bakes
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        void Bake(GameObject root, MeshFilter[] meshFilters) //(Mesh, MeshFilter[])
        {
            ClearLastBake();

            if(meshFilters == null || meshFilters.Length == 0)
                meshFilters = GetMeshFilters(gameObject);

            if (meshFilters == null || meshFilters.Length == 0 || meshFilters[0] == null || !meshFilters[0].GetComponent<MeshRenderer>())
                return;// (null, null);

            (MeshFilter myMF, _) = Create_MyRenderFilter(root, false, meshFilters[0].gameObject.layer, meshFilters[0].GetComponent<MeshRenderer>()?.sharedMaterial);

            Mesh mesh = Combine(root, meshFilters);

            myMF.mesh = mesh;

            //  if (combainInstances == null) combainInstances = new List<GameObject>(); //For reset in editor
            //  combainInstances.Add(gameObject);

            if (isDeactiveMeshRenderersAfterBake)
            {
                for (int i = 0; i < meshFilters.Length; i++)
                    meshFilters[i].GetComponent<MeshRenderer>().enabled = false;
            }
                
            transform.gameObject.SetActive(true);

          //  CheckRenderFiltersToActive(ref meshRenderers); //deactive if only deactive mesh renders

            //transform.localScale = Vector3.one;
            return;// (mesh, meshFilters);
        }
        void Combine_WithSortByMaterial(GameObject root, MeshFilter[] meshFilters) //CombineSorting
        {
            ClearLastBake();
            PrepareRoot(root, true);
            List<Material> materials = new List<Material>();
            List<List<MeshFilter>> targets = new List<List<MeshFilter>>();

            if (meshFilters == null || meshFilters.Length == 0)
                meshFilters = GetMeshFilters(gameObject);

            MeshRenderer mr;
            MeshFilter mf;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].gameObject == gameObject) 
                    continue;
                mr = meshFilters[i].GetComponent<MeshRenderer>();
                mf = meshFilters[i];
                if (!mr || !mf)
                {
                    Debug.Log("No meshFilter or meshRenderer");
                    continue;
                }
                if (!materials.Contains(mr.sharedMaterial))
                {
                    materials.Add(mr.sharedMaterial);
                    targets.Add(new List<MeshFilter>());
                    targets[materials.Count - 1].Add(mf);
                }
                else
                    targets[materials.IndexOf(mr.sharedMaterial)].Add(mf);

                if (isDeactiveMeshRenderersAfterBake)
                    mr.enabled = false;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                Create_MyRenderFilter(root, true, targets[i][0].gameObject.layer, materials[i], Combine(root, targets[i].ToArray()));

              //  if (combainInstances == null) combainInstances = new List<GameObject>();
              //  combainInstances.Add(gos[i]);
            }
            PrepareRoot(root, false);
        }
        void Combine_SortByShaderAndAtlasing(GameObject root, MeshFilter[] meshFilters)
        {
            ClearLastBake();
            PrepareRoot(root, true);
            
            Dictionary<int, Shader> shaders = new Dictionary<int, Shader>();
            Dictionary<int, List<MeshFilter>> filters = new Dictionary<int, List<MeshFilter>>();
            Dictionary<int, List<Material>> renderers = new Dictionary<int, List<Material>>();

            if (meshFilters == null || meshFilters.Length == 0)
               meshFilters = GetMeshFilters(gameObject);

            MeshRenderer mr;
            MeshFilter mf;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].gameObject == gameObject)
                    continue;
                mr = meshFilters[i].GetComponent<MeshRenderer>();
                mf = meshFilters[i];
                if (!mr || !mf || !mr.sharedMaterial || !mr.sharedMaterial.shader)
                {
                    Debug.Log("No meshFilter or meshRenderer or material or shader");
                    continue;
                }
                if (!shaders.ContainsKey(mr.sharedMaterial.shader.GetInstanceID()))
                {
                    shaders.Add(mr.sharedMaterial.shader.GetInstanceID(), mr.sharedMaterial.shader);
                    filters.Add(mr.sharedMaterial.shader.GetInstanceID(), new List<MeshFilter>());
                    renderers.Add(mr.sharedMaterial.shader.GetInstanceID(), new List<Material>());
                }

                filters[mr.sharedMaterial.shader.GetInstanceID()].Add(mf);
                renderers[mr.sharedMaterial.shader.GetInstanceID()].Add(mr.sharedMaterial);

                if (isDeactiveMeshRenderersAfterBake)
                    mr.enabled = false;
            }

            foreach (var meshFilter in filters)
            {
                if (meshFilter.Value == null || meshFilter.Value.Count == 0)
                    continue;

                Mesh mesh = Combine(root, meshFilter.Value.ToArray());
                Material material = new Material(renderers[meshFilter.Key][0]);

                int id = atlasBakeSetup.FindIndex(x => x.shader.GetInstanceID() == shaders[meshFilter.Key].GetInstanceID());

                CombineByAtlassing(mesh, meshFilter.Value, renderers[meshFilter.Key], ref material, id >= 0 ? atlasBakeSetup[id] : null);  // Atlasing   

              //  if (id >= 0 && atlasBakeSetup[id].textureData.Length > 0)
              //  {
              //      for (int i = 0; i < atlasBakeSetup[id].textureData.Length; i++)
              //      {
              //          material.SetTexture(atlasBakeSetup[id].textureData[i].textureKey, 
              //              CombineByAtlassing(mesh, 
              //              meshFilter.Value, renderers[meshFilter.Key], 
              //              atlasBakeSetup[id].textureData[i].textureKey, 
              //              atlasBakeSetup[id].textureData[i].idUv, 
              //              atlasBakeSetup[id].textureData[i].maxTextureSize)
              //              );  // Atlasing
              //      }
              //  }
              //  else
              //      material.SetTexture("_MainTex", CombineByAtlassing(mesh, meshFilter.Value, renderers[meshFilter.Key]));  // Atlasing   
 
                Create_MyRenderFilter(root, true, meshFilter.Value[0].gameObject.layer, material, mesh);
            }

            PrepareRoot(root, false);
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #endregion

        Mesh Combine(GameObject root, MeshFilter[] meshFilters)
        {
            PrepareRoot(root, true); //--------------------------------------------------------
            if (meshFilters == null || meshFilters.Length == 0)
            {
                print("Not has meshes to combine: " + gameObject.name);
                return null;
            }

            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                i++;
            }

            if (!isDeactiveMeshRenderersAfterBake)
                for (int k = 0; k < meshFilters.Length; k++)
                    meshFilters[k].gameObject.SetActive(false);

            Mesh mesh = new Mesh();
            mesh.name = "Combined_" + meshFilters[0].name;
            mesh.CombineMeshes(combine);

            // Aditive bakeOptions
            if(bakeOptions != null)
                for (int k = 0; k < bakeOptions.Length; k++)
                    bakeOptions[k].Bake(ref mesh, ref meshFilters);

            PrepareRoot(root, false); //--------------------------------------------------------
            return mesh;
        }

        void CombineByAtlassing(Mesh mesh, List<MeshFilter> meshFilters, List<Material> materials, ref Material outMaterial, MaterialAtlasBakeSetup bakeSetup)
        {
             if (bakeSetup == null)
                 bakeSetup = new MaterialAtlasBakeSetup(1024);

            for (int i = 0; i < bakeSetup.textureData.Length; i++)
            {
                if (bakeSetup.textureData[i].idUv == -1)
                    // bakeSetup.textureData[i].packRect = bakeSetup.textureData[bakeSetup.textureData[i].isTheSameIn].packRect;
                    bakeSetup.textureData[i].packRect = bakeSetup.textureData[0].packRect;
                  
                outMaterial.SetTexture(bakeSetup.textureData[i].textureKey,
                           CombineByAtlassing(mesh, meshFilters, materials, 
                           ref bakeSetup.textureData[i].packRect, 
                           bakeSetup.textureData[i].textureKey,
                           bakeSetup.textureData[i].idUv,
                           bakeSetup.textureData[i].maxTextureSize)
                           );  // Atlasing
            }
        }
        Texture CombineByAtlassing(Mesh mesh, List<MeshFilter> meshFilters, List<Material> materials, ref Rect[] packRect, string textureKey = "_MainTex", int idUV = 0, int maxTextureSize = 1024)
       // public Texture2D CombineByAtlassing(Mesh mesh, List<MeshFilter> meshFilters, List<Material> materials, MaterialAtlasBakeSetup bakeSetup)
        {
            if (mesh == null || meshFilters == null || materials == null)
            {
                Debug.Log("Mesh bake error: no result mesh or meshFilters in child");
                return null;
            }

           

            Texture2D[] textures = new Texture2D[materials.Count];
            for (int i = 0; i < materials.Count; i++)
            {
               // Debug.Log($"textureKey: {textureKey} in material: {materials[i].name}");
                textures[i] = materials[i].GetTexture(textureKey) as Texture2D;
                if (textures[i] && !textures[i].isReadable)
                    textures[i] = MeshCombiner.CoppyUnreadableTexture(textures[i]);
            }

            Texture atlas;

            if (packRect == null || packRect.Length == 0)
            {
                Texture2D t2d = new Texture2D(0, 0);
                packRect = t2d.PackTextures(textures, 0, maxTextureSize);
                atlas = t2d;
            }
            else
            {
                RenderTexture rt = new RenderTexture(maxTextureSize, maxTextureSize, 0);
                GLRender.DrawAtlas(ref rt, ref packRect, ref textures);
                atlas = rt;
            }

            atlas.name = "Combined_atlas";

            if(idUV >= 0)
            {
                List<Vector2> uv = new List<Vector2>();
                mesh.GetUVs(idUV, uv);

                if (uv == null || uv.Count == 0)
                {
                    if(idUV != 0)
                        mesh.GetUVs(0, uv);
                    if (uv == null || uv.Count == 0)
                    {
                        Vector2 tempV2 = Vector2.zero;
                        uv = new List<Vector2>();
                        for (int i = 0; i < mesh.vertexCount; i++)
                            uv.Add(tempV2);
                    }   
                }
                    

                Vector2[] uvOffset;
                int vertexOffset = 0;
                int idRect;
                // int atlasWidth = atlas.width;
                // int atlasHeight = atlas.height;

                for (int i = 0; i < meshFilters.Count; i++)
                {
                    if (!meshFilters[i].sharedMesh)
                    {
                        Debug.LogWarning("Null Exeption: can't find shared mesh on: " + meshFilters[i].name);
                        continue;
                    }
                       

                    idRect = materials.IndexOf(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial);
                    Debug.Log("idRect : " + idRect);
                    uvOffset = meshFilters[i].sharedMesh.uv;
                    for (int k = 0; k < uvOffset.Length; k++)
                    {
                        uvOffset[k].x = Mathf.Lerp(packRect[idRect].xMin, packRect[idRect].xMax, uvOffset[k].x);
                        uvOffset[k].y = Mathf.Lerp(packRect[idRect].yMin, packRect[idRect].yMax, uvOffset[k].y);
                        /*uvOffset[k].x *= (packingResult[idRect].width / atlasWidth);
                        uvOffset[k].y *= (packingResult[idRect].height / atlasHeight);

                        uvOffset[k].x += packingResult[idRect].x;
                        uvOffset[k].y += packingResult[idRect].y;*/
                    }

                    for (int k = 0; k < uvOffset.Length; k++)
                        uv[k + vertexOffset] = uvOffset[k];

                    vertexOffset += uvOffset.Length;
                }
                mesh.SetUVs(idUV, uv);
            }

            Resources.UnloadUnusedAssets();
            return atlas;
        }

        #region helpers
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        [Header("______________________________")]
        [SerializeField] bool enableLightProbeBlend = false;
        [SerializeField] Material resultMaterial = null;
        [SerializeField] bool isDeactiveMeshRenderersAfterBake = true;
        [SerializeField] bool isCrateNewMaterialInstance = false;


        //  void DeactiveMeshRenderers(ref MeshRenderer[] meshRenderer)
        //  {
        //       if (isDeactiveMeshRenderersAfterBake)
        //           for (int i = 0; i < meshRenderer.Length; i++)
        //              meshRenderer[i].enabled = false;
        //  }

        MeshFilter[] GetMeshFilters(GameObject target) => target ? target.GetComponentsInChildren<MeshFilter>() : new MeshFilter[0];

        (MeshFilter, MeshRenderer) Create_MyRenderFilter(GameObject root, bool isCreateInChild = false, int layer = 0, Material sharedMaterial = null, Mesh mesh = null)
        {
            MeshFilter myMF;
            MeshRenderer myMR;

            if (!root)
                root = gameObject;

            if (!isCreateInChild)
            {
                myMF = root.GetComponent<MeshFilter>();
                if (!myMF) myMF = root.AddComponent<MeshFilter>();
                myMR = root.GetComponent<MeshRenderer>();
                if (!myMR) myMR = root.AddComponent<MeshRenderer>();
            }
            else
            {
                GameObject go;
                if (prefabCombinedInstance)
                {
                    go = Instantiate(prefabCombinedInstance, root.transform);

                    myMF = go.GetComponent<MeshFilter>();
                    if (!myMF) myMF = go.AddComponent<MeshFilter>();
                    myMR = go.GetComponent<MeshRenderer>();
                    if (!myMR) myMR = go.AddComponent<MeshRenderer>();
                } 
                else
                {
                    go = new GameObject();
                    myMF = go.AddComponent<MeshFilter>();
                    myMR = go.AddComponent<MeshRenderer>();
                }

                go.name = sharedMaterial ? sharedMaterial.name : mesh ? mesh.name : "BakedMesh";
                go.layer = layer;
                go.transform.SetParent(root.transform);
                go.transform.SetAsFirstSibling();
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                bakedInstancies.Add(go);
            }

            //myMR.material = transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;

            // myMR.material = transform.GetComponentInChildren<Renderer>().sharedMaterial;

            if (resultMaterial)
                myMR.material = !isCrateNewMaterialInstance ? resultMaterial : new Material(resultMaterial);
            else if (sharedMaterial)
                myMR.material = !isCrateNewMaterialInstance ? sharedMaterial : new Material(sharedMaterial);

            myMR.lightProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.LightProbeUsage.BlendProbes : UnityEngine.Rendering.LightProbeUsage.Off;
            myMR.reflectionProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes : UnityEngine.Rendering.ReflectionProbeUsage.Off;

            if (mesh)
                myMF.mesh = mesh;

            return (myMF, myMR);
        }

        void SendEndBake()
        {
            if (bakedInstancies != null | bakedInstancies.Count > 0)
                for (int i = 0; i < bakedInstancies.Count; i++)
                {
                    if (bakedInstancies[i] && bakedInstancies[i].GetComponent<MeshBake_DoneEvent>())
                        bakedInstancies[i].GetComponent<MeshBake_DoneEvent>().OnBakeDone();
                }
        }

        void ClearLastBake()
        {
            if(bakedInstancies != null| bakedInstancies.Count > 0)
                for (int i = bakedInstancies.Count - 1; i >= 0; i--)
                {
                    if (bakedInstancies[i])
                    {
                        Destroy(bakedInstancies[i]);
                        bakedInstancies.RemoveAt(i);
                    }   
                }
        }

        [SerializeField] GameObject prefabCombinedInstance;

        Transform parent; //temp for reset before & after combine
        Vector3 position; //temp position for reset before & after combine
        Vector3 rotation; //temp rotation for reset before & after combine
        Vector3 oldScale; //temp rotation for reset before & after combine
        bool isPreparedToBake = false; //temp for reset before & after combine
        int siblingIndex; //temp for reset before & after combine

        void PrepareRoot(GameObject rootGO, bool isPrepare)
        {
            Transform root = rootGO ? rootGO.transform : transform;

            if (isPrepare)
            {
                if (!isPreparedToBake)
                {
                    parent = root.parent;
                    siblingIndex = root.GetSiblingIndex();
                    oldScale = root.localScale;

                    if (parent != null)
                        root.SetParent(null);
                    position = root.position;
                    rotation = root.rotation.eulerAngles;


                    root.position = Vector3.zero;
                    root.rotation = Quaternion.identity;
                    root.localScale = Vector3.one;
                    isPreparedToBake = true;
                }
            }
            else if (isPreparedToBake)
            {
                root.position = position;
                root.rotation = Quaternion.Euler(rotation);
                if (parent)
                    root.SetParent(parent);
                root.SetSiblingIndex(siblingIndex);
                root.localScale = oldScale;
                isPreparedToBake = false;

                SendEndBake();
            }
        }
        //----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        #endregion
    }
}
