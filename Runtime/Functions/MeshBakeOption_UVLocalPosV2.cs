using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DimaTi.MeshBaking
{
    [CreateAssetMenu(fileName = "UV_LocalPosV2", menuName = "ScriptableObjects/MeshBake/UV_LocalPosV2", order = 30)]
    public class MeshBakeOption_UVLocalPosV2 : AMeshBakeOption_UV
    {
        public override void Bake(ref Mesh mesh, ref MeshFilter[] meshFilters)
        {
            if (meshFilters == null || meshFilters.Length == 0 || !mesh)
                return;
            mesh.name = BakeName(mesh.name);
            List<Vector2> localPos = new List<Vector2>();

            if (IsOriginCoordFromVertex)
            {
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    Vector3[] verts = meshFilters[i].sharedMesh.vertices;
                    for (int v = 0; v < verts.Length; v++)
                        localPos.Add(new Vector2(verts[v].x * meshFilters[i].transform.localScale.x, verts[v].y * meshFilters[i].transform.localScale.y));
                }
            }
            else
            {
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    Vector3[] verts = meshFilters[i].sharedMesh.vertices;
                    for (int v = 0; v < verts.Length; v++)
                        localPos.Add(meshFilters[i].transform.position);
                }
            }
            mesh.SetUVs(IdUv, localPos);
        }
    }
}
