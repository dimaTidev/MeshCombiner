using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DimaTi.MeshBaking
{
    [CreateAssetMenu(fileName = "UV_copy", menuName = "ScriptableObjects/MeshBake/UV_copy", order = 30)]
    public class MeshBakeOption_UVCopy : AMeshBakeOption
    {
        [SerializeField]
        int
            idUVFrom = 0,
            idUVTo = 1;

        public override void Bake(ref Mesh mesh, ref MeshFilter[] meshFilters)
        {
            List<Vector2> uvs = new List<Vector2>();
            mesh.GetUVs(idUVFrom, uvs);
            mesh.SetUVs(idUVTo, uvs);
        }

        protected override string BakeName(string bakedMeshName) => "";
    }
}
