using UnityEngine;
using Unity.AI.Navigation;
public class GenerateMesh : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshSurface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
