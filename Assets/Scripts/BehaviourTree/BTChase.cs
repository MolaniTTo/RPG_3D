using UnityEngine;

[CreateAssetMenu(fileName = "BTChase", menuName = "BehaviourTree/Chase")]
public class BTChase : BTNode
{
    [SerializeField] private int stoppedAttakckingLayerIndex = 2; // Index del layer d'atacar quiet
    float attackWeight = 0f;
    bool readyToAttack = true; //per saber si està preparat per atacar
    public override bool Execute(EnemyAI enemy)
    {
        if (enemy.CanSeePlayer())
        {
            Vector3 targetPosition = new Vector3(enemy.player.position.x, enemy.transform.position.y, enemy.player.position.z);
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
           
            enemy.agent.SetDestination(targetPosition);

            return true;
            
        }

       
        return false;
       
    }

}
