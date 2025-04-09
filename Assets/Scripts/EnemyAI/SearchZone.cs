using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class SearchZone : MonoBehaviour
{
    public NavMeshSurface navMeshSurface; //surface on es genera el navmesh
    public Transform[] searchPoints; //punts de cerca

    void Start()
    {
        GenerateNavMesh();
    }

    public void GenerateNavMesh()
    {
        navMeshSurface.BuildNavMesh(); //genera el navmesh i fa la build
    }
}
