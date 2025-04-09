using UnityEngine;

[CreateAssetMenu(fileName = "BTFlee", menuName = "BehaviourTree/Flee")]
public class BTFlee : BTNode
{
    public override bool Execute(EnemyAI enemy)
    {
        if (enemy.health < 50) //si esta ferit a menos de 50
        {
            Vector3 fleeDirection = enemy.transform.position - enemy.player.position; //vector que apunta a la direcció contraria al jugador
            enemy.agent.SetDestination(enemy.transform.position + fleeDirection.normalized * 10f); //li passa la posició a la que ha de fugir (la seva posició + la direcció normalitzada per 10)
            return true; //si torna true, vol dir que el node ha estat exitós
        }
        return false; //si no es compleix la condició, torna false
    }
}
