using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshUpdater : MonoBehaviour //per si volem actualitzar el navmesh en temps real
{
    private NavMeshSurface navMeshSurface;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh(); //fa el build del navmesh a l'inici
    }

    public void UpdateNavMesh()
    {
        navMeshSurface.BuildNavMesh(); //ho cridare si elimino algun obstacle o afegeixo algun
    }
}
