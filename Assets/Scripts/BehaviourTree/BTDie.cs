using UnityEngine;

[CreateAssetMenu(fileName = "BTDie", menuName = "BehaviourTree/Die")]
public class BTDie : BTNode
{
    public override bool Execute(EnemyAI enemy)
    {
        if (enemy.health <= 0)
        {
            // Detener movimiento
            enemy.agent.isStopped = true;

            // Activar animación de muerte
            
            if (enemy.animator != null)
            {
                enemy.animator.SetTrigger("Die");
            }

            // Puedes destruir al enemigo o desactivar comportamiento
            Object.Destroy(enemy.gameObject, 3f); // opcional, destruye después de 3 seg

            return true;
        }
        return false;
    }
}
