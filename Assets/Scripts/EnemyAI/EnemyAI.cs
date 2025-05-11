using System;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.SearchableEditorWindow;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    
    public Transform player;
    public Transform[] patrolPoints;

    private int patrolIndex = 0;

    public BTNode rootNode;

    private Vector3 lastSeenPosition;

    private GameObject searchZoneInstance;
    public GameObject searchZonePrefab;
    public bool isChasingPlayer = false; //per saber si l'enemic esta perseguint al jugador
    public bool isFleeing = false; //per saber si l'enemic esta fugint o no

    public float health = 100f;
    public float viewRadius = 10f; //visio normal
    public float visionAngle = 90f; //es el angle de visio del enemic
    public float closeDetectionRadius = 5f; //distancia mininima on sempre detecta el player (l'escolta)

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    public Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        bool canseePlayer = CanSeePlayer(); //comprova si pot veure al player

        if (CanSeePlayer()) //si pot veure al player
        {
            lastSeenPosition = player.position; //guarda la posicio del player
            if (searchZoneInstance != null) //si ja te una zona de busqueda la destrueix perque ja no la necessita
            {
                Destroy(searchZoneInstance);
                searchZoneInstance = null;
            }
            if(animator != null && health > 50)
            {
                animator.SetBool("CanSeePlayer", true);
            }
           
        }
        else
        {
            if(animator != null && health > 50)
            {
                animator.SetBool("CanSeePlayer", false);
            }
            if (searchZoneInstance == null && health < 100) //si no pot veure al player i té menos de 100 de vida crea una zona de busqueda a la ultima posicio on va veure al player
            {
                CreateSearchZone();
            }
        }

        rootNode.Execute(this);
    }

    public void ChangeVelocity(float velocity)
    {
        agent.speed = velocity;
    }

    public void NextPatrolPoint() //funcio per anar al seguent punt de patrulla
    {
        if (searchZoneInstance == null)
        {
            //patrulla normal
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length; //agafa el següent punt de patrulla (si arriba al final torna al principi)
            agent.SetDestination(patrolPoints[patrolIndex].position); //posa el desti de l'agent al punt de patrulla
        }
        else
        {
            //patrulla dins de la zona de busqueda
            Transform[] searchPoints = searchZoneInstance.GetComponent<SearchZone>().searchPoints;
            patrolIndex = (patrolIndex + 1) % searchPoints.Length;
            agent.SetDestination(searchPoints[patrolIndex].position);
        }
    }

    public bool CanSeePlayer() //funcio per veure si l'enemic pot veure al player
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized; //direccio del enemic al player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); //distancia del enemic al player


        if (distanceToPlayer < closeDetectionRadius) //si el jugador esta molt aprop el detecta (l'escolta)
        {
            if(isFleeing == false) { SmoothLookAt(); }
            return true;
        }

        //si esta mes lluny de la distancia minima comprova si pot veure al player
        if (distanceToPlayer < viewRadius)
        {
            if (Vector3.Angle(transform.forward, directionToPlayer) < visionAngle / 2)
            {
                //si no hi ha obstacles en mitg de la linia de visio retorna true (el detecta)
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask)) //si no hi ha res entre el enemic i el player
                {
                    if(isFleeing == false) { SmoothLookAt(); }

                    return true; //retorna que si el pot veure
                }
            }
        }

        return false; //no el pot veure
    }

    void SmoothLookAt()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void CreateSearchZone()
    {
        searchZoneInstance = Instantiate(searchZonePrefab, lastSeenPosition, Quaternion.identity); //instancia el prefab de la zona de busqueda
        searchZoneInstance.GetComponent<SearchZone>().GenerateNavMesh(); //genera el navmesh de la zona de busqueda
    }

    public void Hurt(float damage)
    {
        if (damage < 0)
        {
            damage = 0;
            Destroy(gameObject); //si el dany es negatiu l'enemic mor
        }

        health -= damage;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        //dibuixa la distancia minima de deteccio

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, closeDetectionRadius);

        //dibuixa el angle de visio

        Vector3 leftLimit = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward;
        Vector3 rightLimit = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftLimit * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightLimit * viewRadius);
    }
}
