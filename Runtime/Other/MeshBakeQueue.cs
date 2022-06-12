using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimaTi.MeshBaking
{
    public class MeshBakeQueue
    {
        [SerializeField] AMeshBakeOption[] bakeOptions;
    }

    public abstract class AMeshBakeOption : ScriptableObject
    {
        protected abstract string BakeName(string bakedMeshName);
        public abstract void Bake(ref Mesh mesh, ref MeshFilter[] meshFilters);
    }

    public abstract class AMeshBakeOption_UV : AMeshBakeOption
    {
        [SerializeField] int idUV = 1;
        [SerializeField] bool isOriginCoordFromVertex = false; //true = Write coords means is offset from vertex pos to origin, or false = offset from center bake
        protected int IdUv => idUV;
        protected bool IsOriginCoordFromVertex => isOriginCoordFromVertex;
       // public abstract void Bake(ref Mesh mesh, ref MeshFilter[] meshFilters);

        protected override string BakeName(string bakedMeshName) => $"UV{IdUv}LocalPos_" + bakedMeshName;
    }

    
    [CreateAssetMenu(fileName = "Post_Atlasing", menuName = "ScriptableObjects/MeshBake/Post_Atlasing", order = 30)]
    public class MeshBakeOption_PostAtlasing : ScriptableObject
    {
        public void Bake()
        {

        }
    }
}
