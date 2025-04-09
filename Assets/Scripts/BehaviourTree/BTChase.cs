using UnityEngine;

[CreateAssetMenu(fileName = "BTChase", menuName = "BehaviourTree/Chase")]
public class BTChase : BTNode
{
    public override bool Execute(EnemyAI enemy)
    {
        if (enemy.CanSeePlayer()) //si l'enemic pot veure al jugador
        {
            enemy.agent.SetDestination(enemy.player.position); //li passa la posici� del jugador com a dest�
            return true; //si torna true, vol dir que el node ha estat exit�s
        }
        return false; //si no es compleix la condici�, torna false
    }
}
