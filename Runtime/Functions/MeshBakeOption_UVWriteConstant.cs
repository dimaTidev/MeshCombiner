using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DimaTi.MeshBaking
{
    [CreateAssetMenu(fileName = "UV_WriteConstant", menuName = "ScriptableObjects/MeshBake/UV_WriteConstant", order = 30)]
    public class MeshBakeOption_UVWriteConstant : AMeshBakeOption
    {
        [SerializeField, Range(0,7)] int idUV;
        [SerializeField] Vector2 uvData = Vector2.zero;

        public override void Bake(ref Mesh mesh, ref MeshFilter[] meshFilters)
        {
            List<Vector2> uvs = new List<Vector2>();
            int count = mesh.vertexCount;
            for (int i = 0; i < count; i++)
                uvs.Add(uvData);
            mesh.SetUVs(idUV, uvs);
        }

        protected override string BakeName(string bakedMeshName) => "";
    }
}
