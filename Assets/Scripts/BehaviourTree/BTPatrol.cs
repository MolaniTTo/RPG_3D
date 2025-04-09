using UnityEngine;

[CreateAssetMenu(fileName = "BTPatrol", menuName = "BehaviourTree/Patrol")]
public class BTPatrol : BTNode
{
    public override bool Execute(EnemyAI enemy)
    {
        if (enemy.agent.remainingDistance < 0.5f) //si l'agent ha arribat al punt de patrulla
        {
            enemy.NextPatrolPoint(); //pilla el següent punt de patrulla
        }
        return true; //si torna true, vol dir que el node ha estat exitós
    }
}
