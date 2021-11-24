using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DimaTi.MeshBaking
{
    [CreateAssetMenu(fileName = "Color_Random", menuName = "ScriptableObjects/MeshBake/Color_Random", order = 30)]
    public class MeshBakeOption_RandomColorByMesh : AMeshBakeOption
    {
        protected override string BakeName(string bakedMeshName) => $"ColorRandom_" + bakedMeshName;
        public override void Bake(ref Mesh mesh, ref MeshFilter[] meshFilters)
        {
            if (meshFilters == null || meshFilters.Length == 0 || !mesh)
                return;


            Debug.Log("MeshBakeOption_RandomColorByMesh");
            mesh.name = BakeName(mesh.name);
            Color randomColor;
            List<Color> colors = new List<Color>();
            int vertexCount;

            for (int i = 0; i < meshFilters.Length; i++)
            {
                randomColor = Random.ColorHSV();
                vertexCount = meshFilters[i].sharedMesh.vertexCount;

                for (int v = 0; v < vertexCount; v++)
                    colors.Add(randomColor);
            }
            mesh.SetColors(colors);
        }
    }
}
