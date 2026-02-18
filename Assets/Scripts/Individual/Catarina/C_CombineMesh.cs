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
        CombineInstance[] combines = new CombineInstance[meshFilters.Length];
        int i = 0;
        while (i < meshFilters.Length)
        {
            // Create a readable copy of mesh if needed
            Mesh sourceMesh = meshFilters[i].sharedMesh;
            if (sourceMesh != null && !sourceMesh.isReadable)
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

            combines[i].mesh = sourceMesh;
            combines[i].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combines);
        transform.GetComponent<MeshFilter>().sharedMesh = mesh;
        transform.gameObject.SetActive(true);

        var meshCollider = transform.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        if (_CreateRB)
        {
            gameObject.AddComponent<Rigidbody>();
        }
    }
    private void Start()
    {
        Optimize();
    }
}