using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    public enum CombineQueue
    {
        Combine = 0,
        CombineSortingMaterials = 7,
        ByAtlasing = 1,
        ByAtlasingOnlyColors = 2,
        SeparateSubmeshes = 3,
        WithLocalPositionsVector2 = 4,
        WithLocalPositionsVector3 = 5,
        RandomColorByMesh = 6
    }
    [SerializeField] CombineQueue[] combineQueues;

    [SerializeField] bool disableCombineInStart;
    //  [SerializeField] bool combineWithSubmeshes;
    //  [SerializeField] bool isCombineByAtlasingOnlyColors;

    [SerializeField] bool enableLightProbeBlend;
    [SerializeField] bool isDeactiveMeshRenderersAfterBake;

    [HideInInspector] public List<GameObject> combainInstances; //save instancies for easy clear childs or instancies
    Vector3 position; //temp position for reset before & after combine
    Vector3 rotation; //temp rotation for reset before & after combine
    void Start()
    {
        if (disableCombineInStart) return;

        if (transform.childCount == 0)
            return;

        //   if (isCombineByAtlasingOnlyColors)
        //       CombineByAtlasingOnlyColors();

        if (combineQueues != null && combineQueues.Length > 0)
        {
            for (int i = 0; i < combineQueues.Length; i++)
                InvokeQueueCombine(combineQueues[i]);
        }

        /*  if(!combineWithSubmeshes)
              Combine();
          else
              CombineSorting();

          */
    }

    void InvokeQueueCombine(CombineQueue combineType)
    {
        switch (combineType)
        {
            case CombineQueue.Combine:
                Combine();
                break;
            case CombineQueue.CombineSortingMaterials:
                CombineSorting();
                break;
            case CombineQueue.ByAtlasing:
                CombineByAtlassing();
                break;
            case CombineQueue.ByAtlasingOnlyColors:
                CombineByAtlasingOnlyColors();
                break;
            case CombineQueue.SeparateSubmeshes:
                UnCombineSubmesh();
                break;
            case CombineQueue.WithLocalPositionsVector2:
                Combine(1);
                break;
            case CombineQueue.WithLocalPositionsVector3:
                Combine(2);
                break;
            case CombineQueue.RandomColorByMesh:
                Combine_ByRandomColorByMesh();
                break;
        }
    }

    public void Combine_WithoutReturn() => Combine();

    /*   public Mesh Combine()
       {
           MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
           MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
           if (meshFilters == null || meshFilters.Length == 0 || meshRenderers == null || meshRenderers.Length == 0)
               return null;

           PrepareRoot(true);
           MeshFilter myMF = gameObject.GetComponent<MeshFilter>();
           if (!myMF) myMF = gameObject.AddComponent<MeshFilter>();

           MeshRenderer myMR = gameObject.GetComponent<MeshRenderer>();
           if (!myMR) myMR = gameObject.AddComponent<MeshRenderer>();
           myMR.material = transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;
           myMR.lightProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.LightProbeUsage.BlendProbes : UnityEngine.Rendering.LightProbeUsage.Off;
           myMR.reflectionProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes : UnityEngine.Rendering.ReflectionProbeUsage.Off;

           Mesh mesh = Combine(meshFilters);
           mesh.name = "Combined_" + meshFilters[0].name;
           myMF.mesh = mesh;
           if (combainInstances == null) combainInstances = new List<GameObject>();
           combainInstances.Add(gameObject);

           transform.gameObject.SetActive(true);

           if (isDeactiveMeshRenderersAfterBake)
               for (int i = 0; i < meshRenderers.Length; i++)
                   meshRenderers[i].enabled = false;

           PrepareRoot(false);

           transform.localScale = Vector3.one;

           return mesh;
       }*/

    public (Mesh, MeshFilter[]) Combine(int isBakeLocalPositions = 0)
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshFilters == null || meshFilters.Length == 0 || meshRenderers == null || meshRenderers.Length == 0)
            return (null, null);


        /*   for (int i = 0; i < meshFilters.Length; i++)
           {
               if(meshFilters[i].gameObject.GetInstanceID() == gameObject.GetInstanceID())
                   Debug.Log("FINDED!!!!!!!!!!!!!!!");
           }*/

        (MeshFilter myMF, _) = Create_MyRenderFilter(meshRenderers[0]);

        Mesh mesh = Combine(meshFilters);

        if (isBakeLocalPositions != 0)
            Combine_ByLocalPositions_SameMeshes(ref mesh, ref meshFilters, isBakeLocalPositions == 1 ? false : true);

        myMF.mesh = mesh;

        if (combainInstances == null) combainInstances = new List<GameObject>();
        combainInstances.Add(gameObject);

        transform.gameObject.SetActive(true);

        CheckRenderFiltersToActive(ref meshRenderers);

        transform.localScale = Vector3.one;
        return (mesh, meshFilters);
    }

    public Mesh Combine_ByLocalPositions_SameMeshes(ref Mesh mesh, ref MeshFilter[] meshFilters, bool isUseLocalPosVector3)
    {
        if (meshFilters == null || meshFilters.Length == 0) return null;

        mesh.name = "Com_locPos_" + meshFilters[0].name;

        Vector3[] verts = meshFilters[0].sharedMesh.vertices;

        if (!isUseLocalPosVector3)
        {
            Vector2[] localPos = new Vector2[meshFilters.Length * verts.Length];

            for (int i = 0; i < meshFilters.Length; i++)
            {
                for (int v = 0; v < verts.Length; v++)
                {
                    localPos[i * verts.Length + v] = new Vector2(verts[v].x * meshFilters[i].transform.localScale.x, verts[v].y * meshFilters[i].transform.localScale.y);
                    //Debug.Log($"verts:{verts[v].x}, {verts[v].y}");
                }
            }
            mesh.uv2 = localPos;

            // for (int i = 0; i < localPos.Length; i++)
            //     Debug.Log($"localPos:{localPos[i].x}, {localPos[i].y}");
        }
        else //Добавляем vector3
        {
            List<Vector3> localPos = new List<Vector3>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                for (int v = 0; v < verts.Length; v++)
                {
                    //  localPos[i * verts.Length + v] = new Vector3(verts[v].x * meshFilters[i].transform.localScale.x, verts[v].y * meshFilters[i].transform.localScale.y, verts[v].z * meshFilters[i].transform.localScale.z);
                    localPos.Add(new Vector3(verts[v].x * meshFilters[i].transform.localScale.x, verts[v].y * meshFilters[i].transform.localScale.y, verts[v].z * meshFilters[i].transform.localScale.z));
                }

                mesh.SetUVs(1, localPos);
            }
        }

        return null;
    }

    //Helpers --------------------------------------------------
    void CheckRenderFiltersToActive(ref MeshRenderer[] meshRenderers)
    {
        if (isDeactiveMeshRenderersAfterBake)
            for (int i = 0; i < meshRenderers.Length; i++)
                meshRenderers[i].enabled = false;
    }

    (MeshFilter, MeshRenderer) Create_MyRenderFilter(MeshRenderer mr_from = null)
    {
        MeshFilter myMF = gameObject.GetComponent<MeshFilter>();
        if (!myMF) myMF = gameObject.AddComponent<MeshFilter>();

        MeshRenderer myMR = gameObject.GetComponent<MeshRenderer>();
        if (!myMR) myMR = gameObject.AddComponent<MeshRenderer>();

        //myMR.material = transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;

        // myMR.material = transform.GetComponentInChildren<Renderer>().sharedMaterial;

        if (resultMaterial)
            myMR.material = resultMaterial;
        else if (mr_from)
            myMR.material = mr_from.sharedMaterial;

        myMR.lightProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.LightProbeUsage.BlendProbes : UnityEngine.Rendering.LightProbeUsage.Off;
        myMR.reflectionProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes : UnityEngine.Rendering.ReflectionProbeUsage.Off;

        return (myMF, myMR);
    }
    //----------------------------------------------------------
    public void UnCombineSubmesh()
    {
        PrepareRoot(true);
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        for (int m = 0; m < meshFilters.Length; m++)
        {
            MeshFilter meshFilter = meshFilters[m];
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.Log("No meshFilter or sharedMesh");

                //#if UNITY_EDITOR
                //                Selection.activeObject = meshFilter.gameObject;
                //#endif

            }
            if (meshFilter == null || meshFilter.sharedMesh.subMeshCount == 0 || meshFilter.sharedMesh.subMeshCount == 1)
                continue;

            MeshRenderer meshRenderer = meshFilter.gameObject.GetComponent<MeshRenderer>();
            Material[] mats = meshRenderer.sharedMaterials;

            for (int i = 0; i < meshFilter.sharedMesh.subMeshCount; i++)
            {
                Mesh mesh = meshFilter.sharedMesh.GetSubmesh(i);
                mesh.name = (mats != null && i < mats.Length ? mats[i].name : (" Sub_" + i.ToString()));

                GameObject go = new GameObject();
                go.transform.SetParent(transform);
                go.transform.SetAsFirstSibling();
                //go.transform.localPosition = meshFilter.transform.position;
                //go.transform.localRotation = meshFilter.transform.rotation;
                go.transform.position = meshFilter.transform.position;
                go.transform.rotation = meshFilter.transform.rotation;
                go.transform.localScale = meshFilter.transform.localScale;
                go.AddComponent<MeshFilter>().mesh = mesh;
                go.AddComponent<MeshRenderer>().material = mats != null && i < mats.Length ? mats[i] : null;
                go.name = meshFilter.gameObject.name + mesh.name;
            }
            if (meshFilter.sharedMesh.subMeshCount > 0)
                DestroyImmediate(meshFilter.gameObject);
        }
        PrepareRoot(false);
    }

    public void CombineSorting()
    {
        PrepareRoot(true);
        List<Material> materials = new List<Material>();
        List<List<MeshFilter>> targets = new List<List<MeshFilter>>();

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        MeshRenderer mr;
        MeshFilter mf;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject == gameObject) continue;
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
            {
                targets[materials.IndexOf(mr.sharedMaterial)].Add(mf);
            }

            if (isDeactiveMeshRenderersAfterBake)
                mr.enabled = false;
        }

        GameObject[] gos = new GameObject[targets.Count];

        for (int i = 0; i < targets.Count; i++)
        {
            gos[i] = new GameObject();
            gos[i].transform.parent = transform;
            MeshFilter meshFilter = gos[i].AddComponent<MeshFilter>();
            meshFilter.mesh = Combine(targets[i].ToArray());
            MeshRenderer meshRenderer = gos[i].AddComponent<MeshRenderer>();
            meshRenderer.material = materials[i];
            meshRenderer.lightProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.LightProbeUsage.BlendProbes : UnityEngine.Rendering.LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = enableLightProbeBlend ? UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes : UnityEngine.Rendering.ReflectionProbeUsage.Off;
            gos[i].name = materials[i].name;
            if (combainInstances == null) combainInstances = new List<GameObject>();
            combainInstances.Add(gos[i]);
        }
        PrepareRoot(false);
    }


    [SerializeField] Shader resultShader;

    [SerializeField, Tooltip("Optional")] Material resultMaterial;

    public Texture2D CombineByAtlasingOnlyColors()
    {
        //--- Combine ------------------------------------------------------------
        UnCombineSubmesh();
        (Mesh mesh, MeshFilter[] meshFilters) = Combine();

        if (mesh == null || meshFilters == null)
        {
            Debug.Log("Mesh bake error: no result mesh or meshFilters in child");
            return null;
        }

        //--- Make Atlas ------------------------------------------------------------
        List<Material> materials = new List<Material>();
        int[] rect = new int[meshFilters.Length];
        int[] vertCounts = new int[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
            MeshRenderer meshRenderer = meshFilters[i].GetComponent<MeshRenderer>();
            if (!materials.Contains(meshRenderer.sharedMaterial))
                materials.Add(meshRenderer.sharedMaterial);

            rect[i] = materials.IndexOf(meshRenderer.sharedMaterial);
            vertCounts[i] = meshFilters[i].sharedMesh.vertexCount;
        }

        //Texture2D texture = new Texture2D(Mathf.NextPowerOfTwo(materials.Count), 1, TextureFormat.RGB24, false);
        // Texture2D texture = new Texture2D(Mathf.NextPowerOfTwo(materials.Count * 4), 4, TextureFormat.RGB24, false);
        Texture2D texture = new Texture2D(Mathf.NextPowerOfTwo(materials.Count), 1, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point
        };
        //Color tempColor;
        for (int i = 0; i < materials.Count; i++)
        {
            //Debug.Log("used color = " + materials[i].GetColor("_Color"));
            //  texture.SetPixel(i, 0, materials[i].GetColor("_Color"));
            /* tempColor = materials[i].GetColor("_Color");
             for (int k = 0; k < 1; k++)
             {
                 texture.SetPixel(i, k, tempColor);
            //     texture.SetPixel(i * 4 + 1, k, tempColor);
              //   texture.SetPixel(i * 4 + 2, k, tempColor);
            //     texture.SetPixel(i * 4 + 3, k, tempColor);
             }*/

            texture.SetPixel(i, 0, materials[i].GetColor("_Color"));
        }

        texture.Apply();

        Material mat = resultMaterial ? resultMaterial : new Material(resultShader);
        mat.SetTexture("_MainTex", texture);

        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (!mr) mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = mat;
        //--- UV offset to atlas  ---------------------------------------------------
        Vector2[] uv = new Vector2[mesh.vertexCount];

        int curSubMesh = 0;
        int vertexStamp = 0;

        float xStep = 1f / texture.width;//Mathf.InverseLerp(0, texture.width, texture.width / materials.Count);

        for (int i = 0; i < uv.Length; i++)
        {
            if (i - vertexStamp >= vertCounts[curSubMesh])
            {
                vertexStamp = i;
                curSubMesh++;
                //  Debug.Log("rect[curSubMesh] = " + rect[curSubMesh] + "/" + meshFilters[curSubMesh].GetComponent<MeshRenderer>().sharedMaterial.GetColor("_Color"));
            }
            uv[i].x = rect[curSubMesh] * xStep + xStep / 2;
            uv[i].y = 0.5f;
        }
        mesh.uv = uv;

        //--- Set mesh data to object  ---------------------------------------------------
        MeshFilter mf = GetComponent<MeshFilter>();
        if (!mr) mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        return texture;
    }

    public Mesh Combine_ByRandomColorByMesh()
    {
        (Mesh mesh, MeshFilter[] meshFilters) = Combine();

        if (mesh == null || meshFilters == null)
        {
            Debug.Log("Mesh bake error: no result mesh or meshFilters in child");
            return null;
        }

        Color randomColor;
        int vertexOffset = 0;
        int vertCount;

        Color[] colors = new Color[mesh.vertexCount];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            vertCount = meshFilters[i].mesh.vertexCount;
            randomColor = Random.ColorHSV();

            for (int k = 0; k < vertCount; k++)
                colors[vertexOffset + k] = randomColor;
            vertexOffset += vertCount;
        }

        mesh.colors = colors;
        return mesh;
    }

    public Texture2D CombineByAtlassing()
    {
        //--- Combine ------------------------------------------------------------
        (Mesh mesh, MeshFilter[] meshFilters) = Combine();

        if (mesh == null || meshFilters == null)
        {
            Debug.Log("Mesh bake error: no result mesh or meshFilters in child");
            return null;
        }

        //--- Make Atlas ------------------------------------------------------------
        List<Material> materials = new List<Material>();
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.GetInstanceID() == gameObject.GetInstanceID()) continue;
            MeshRenderer meshRenderer = meshFilters[i].GetComponent<MeshRenderer>();
            if (!materials.Contains(meshRenderer.sharedMaterial))
                materials.Add(meshRenderer.sharedMaterial);
        }

        Texture2D[] textures = new Texture2D[materials.Count];
        for (int i = 0; i < materials.Count; i++)
        {
            textures[i] = materials[i].GetTexture("_MainTex") as Texture2D;
            if (!textures[i].isReadable)
                textures[i] = CoppyUnreadableTexture(textures[i]);
        }

        Texture2D atlas = new Texture2D(128, 128);
        atlas.name = "Combined_atlas";
        Rect[] packingResult = atlas.PackTextures(textures, 0, 1024);

        Vector2[] uv = mesh.uv;
        Vector2[] uvOffset;
        int vertexOffset = 0;
        int idRect;
        int atlasWidth = atlas.width;
        int atlasHeight = atlas.height;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            idRect = materials.IndexOf(meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial);
            uvOffset = meshFilters[i].sharedMesh.uv;
            Debug.Log("idRect = " + idRect);
            for (int k = 0; k < uvOffset.Length; k++)
            {

                uvOffset[k].x = Mathf.Lerp(packingResult[idRect].xMin, packingResult[idRect].xMax, uvOffset[k].x);
                uvOffset[k].y = Mathf.Lerp(packingResult[idRect].yMin, packingResult[idRect].yMax, uvOffset[k].y);

                /*uvOffset[k].x *= (packingResult[idRect].width / atlasWidth);
                uvOffset[k].y *= (packingResult[idRect].height / atlasHeight);

                uvOffset[k].x += packingResult[idRect].x;
                uvOffset[k].y += packingResult[idRect].y;*/
            }

            for (int k = 0; k < uvOffset.Length; k++)
                uv[k + vertexOffset] = uvOffset[k];

            vertexOffset += uvOffset.Length;
        }

        mesh.uv = uv;

        Resources.UnloadUnusedAssets();
        return atlas;
    }

    Texture2D CoppyUnreadableTexture(Texture2D texture)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
        Graphics.Blit(texture, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        Texture2D myTexture2D = new Texture2D(texture.width, texture.height);
        myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        myTexture2D.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        return myTexture2D;
    }

    Mesh Combine(MeshFilter[] meshFilters)
    {
        PrepareRoot(true); //--------------------------------------------------------
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
        PrepareRoot(false); //--------------------------------------------------------
        return mesh;
    }

    public void ActiveDeactiveChilds()
    {
        if (transform.childCount == 0) return;
        bool state = !transform.GetChild(0).gameObject.activeSelf;

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(state);
    }
    void PrepareRoot(bool isPrepare)
    {
        if (isPrepare)
        {
            position = transform.position;
            rotation = transform.rotation.eulerAngles;
        }
        transform.position = isPrepare ? Vector3.zero : position;
        transform.rotation = isPrepare ? Quaternion.identity : Quaternion.Euler(rotation);
    }

    public void UnparentChilds()
    {
        Transform emptyParent = new GameObject().transform;
        emptyParent.SetParent(transform);
        emptyParent.localPosition = Vector3.zero;
        emptyParent.localRotation = Quaternion.identity;
        emptyParent.name = "Empty GameObjects";

        Transform[] allChilds = GetComponentsInChildren<Transform>();

        for (int i = 0; i < allChilds.Length; i++)
        {
            if (transform == allChilds[i]) continue;
            if (allChilds[i].GetComponent<MeshFilter>())
                allChilds[i].SetParent(transform);
            else
                allChilds[i].SetParent(emptyParent);
        }

        if (emptyParent.childCount > 0)
            emptyParent.SetAsFirstSibling();
        else
            Destroy(emptyParent.gameObject);
    }

    public void ExtractSimpleColliders()
    {
        Transform emptyParent = new GameObject().transform;
        emptyParent.SetParent(transform);
        emptyParent.localPosition = Vector3.zero;
        emptyParent.localRotation = Quaternion.identity;
        emptyParent.name = "Colliders_Box";

        BoxCollider[] allChilds = GetComponentsInChildren<BoxCollider>();

        for (int i = 0; i < allChilds.Length; i++)
        {
            GameObject go = new GameObject();
            go.name = allChilds[i].name;
            go.transform.SetParent(emptyParent);
            go.transform.position = allChilds[i].transform.position;
            go.transform.rotation = allChilds[i].transform.rotation;
            go.transform.localScale = allChilds[i].transform.localScale;

            BoxCollider boxCol = go.AddComponent<BoxCollider>();
            boxCol.size = allChilds[i].size;
            boxCol.center = allChilds[i].center;
        }

        if (emptyParent.childCount > 0)
            emptyParent.SetAsFirstSibling();
        else
            Destroy(emptyParent.gameObject);

        for (int i = 0; i < allChilds.Length; i++)
            DestroyImmediate(allChilds[i]);
    }
}