using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DimaTi.MeshBaking
{
    [CreateAssetMenu(fileName = "UV_LocalPosV3", menuName = "ScriptableObjects/MeshBake/UV_LocalPosV3", order = 30)]
    public class MeshBakeOption_UVLocalPosV3 : AMeshBakeOption_UV
    {
        public override void Bake(ref Mesh mesh, ref MeshFilter[] meshFilters)
        {
            if (meshFilters == null || meshFilters.Length == 0 || !mesh)
                return;
            mesh.name = BakeName(mesh.name);
            List<Vector3> localPos = new List<Vector3>();

            if (IsOriginCoordFromVertex)
            {
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    Vector3[] verts = meshFilters[i].sharedMesh.vertices;
                    for (int v = 0; v < verts.Length; v++)
                        localPos.Add(new Vector3(verts[v].x * meshFilters[i].transform.localScale.x, verts[v].y * meshFilters[i].transform.localScale.y, verts[v].z * meshFilters[i].transform.localScale.z));
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
