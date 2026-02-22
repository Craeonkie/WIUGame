using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class C_CombineMesh : MonoBehaviour
{
    [SerializeField] bool _CreateRB;

    public void Optimize()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        var combineList = new System.Collections.Generic.List<CombineInstance>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            // Skip self MeshFilter (important)
            if (meshFilters[i].transform == transform)
                continue;

            Mesh sourceMesh = meshFilters[i].sharedMesh;

            // Skip if null
            if (sourceMesh == null)
                continue;

            // Create readable copy if needed
            if (!sourceMesh.isReadable)
            {
                Mesh readableMesh = new Mesh();
                readableMesh.vertices = sourceMesh.vertices;
                readableMesh.triangles = sourceMesh.triangles;
                readableMesh.normals = sourceMesh.normals;
                readableMesh.uv = sourceMesh.uv;
                readableMesh.colors = sourceMesh.colors;
                readableMesh.tangents = sourceMesh.tangents;
                sourceMesh = readableMesh;
            }

            CombineInstance ci = new CombineInstance();
            ci.mesh = sourceMesh;
            ci.transform = transform.worldToLocalMatrix *
                           meshFilters[i].transform.localToWorldMatrix;

            combineList.Add(ci);

            meshFilters[i].gameObject.SetActive(false);
        }

        if (combineList.Count == 0)
            return;

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combineList.ToArray(), true, true);

        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        if (_CreateRB)
            gameObject.AddComponent<Rigidbody>();
    }
    private void Start()
    {
        Optimize();
    }
}