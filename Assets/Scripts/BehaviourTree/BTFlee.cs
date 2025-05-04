using UnityEngine;

[CreateAssetMenu(fileName = "BTFlee", menuName = "BehaviourTree/Flee")]
public class BTFlee : BTNode
{

    [SerializeField] private int upperBodyLayerIndex = 1; // Index del layer d'aim
    public override bool Execute(EnemyAI enemy)
    {
        if (enemy.health < 50)
        {

            if (enemy.agent.updateRotation) { enemy.agent.updateRotation = false; }
            if(enemy.isFleeing == false) { enemy.isFleeing = true; }
           

            // Calcular dirección opuesta al jugador
            Vector3 directionToPlayer = enemy.player.position - enemy.transform.position;
            directionToPlayer.y = 0f;
            Vector3 fleeDirection = -directionToPlayer.normalized;

            // Calcular destino de huida
            Vector3 fleeTarget = enemy.transform.position + fleeDirection * 10f;
            enemy.agent.SetDestination(fleeTarget);

            // Rotar el enemigo hacia dirección de huida
            if (fleeDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(fleeDirection);
                enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            // Activar animación
            if (enemy.animator != null)
            {
                enemy.animator.SetBool("CanSeePlayer", false);
                enemy.animator.SetTrigger("Flee");
            }

            enemy.agent.speed = 3.5f;

            Debug.DrawRay(enemy.transform.position, enemy.transform.forward * 2f, Color.red);
            
            float attackWeight = 0f;
            float currentWeight = enemy.animator.GetLayerWeight(upperBodyLayerIndex); //layer 1 es el d'aim
            float newWeight = Mathf.Lerp(currentWeight, attackWeight, Time.deltaTime * 5f); //disminueixo el weight del layer d'aim
            enemy.animator.SetLayerWeight(upperBodyLayerIndex, newWeight); //actualitza el weight del layer d'aim

            return true;
        }

        return false;
    }
}
