using UnityEngine;

[CreateAssetMenu(fileName = "BTChase", menuName = "BehaviourTree/Chase")]
public class BTChase : BTNode
{
    public override bool Execute(EnemyAI enemy)
    {
        if (enemy.CanSeePlayer()) //si l'enemic pot veure al jugador
        {
            enemy.agent.SetDestination(enemy.player.position); //li passa la posició del jugador com a destí
            return true; //si torna true, vol dir que el node ha estat exitós
        }
        return false; //si no es compleix la condició, torna false
    }
}
