using UnityEngine;

[CreateAssetMenu(fileName = "BTAttack", menuName = "BehaviourTree/Attack")]
public class BTAttack : BTNode
{
    public float attackRange = 2f; // ajusta según lo necesario
    [SerializeField] private int upperBodyLayerIndex = 1; // Index del layer d'aim (0 es el base layer, 1 es el d'aim)
    float attackWeight = 0f; // inicialitzem el weight a 0 perque no ataqui al principi

    public override bool Execute(EnemyAI enemy)
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
        Debug.Log("Distance: " + distance); //per veure la distancia entre el enemic i el player

        if (distance <= attackRange)
        {
            Debug.Log("Attack"); //per veure si ataca
            attackWeight = 1f;
            float currentWeight = enemy.animator.GetLayerWeight(upperBodyLayerIndex); //layer 1 es el d'aim
            float newWeight = Mathf.Lerp(currentWeight, attackWeight, Time.deltaTime * 5f); //disminueixo el weight del layer d'aim
            enemy.animator.SetLayerWeight(upperBodyLayerIndex, newWeight); //actualitza el weight del layer d'aim
            return true;
        }

        else
        {
            attackWeight = 0f;
            float currentWeight = enemy.animator.GetLayerWeight(upperBodyLayerIndex); //layer 1 es el d'aim
            float newWeight = Mathf.Lerp(currentWeight, attackWeight, Time.deltaTime * 5f); //disminueixo el weight del layer d'aim
            enemy.animator.SetLayerWeight(upperBodyLayerIndex, newWeight); //actualitza el weight del layer d'aim
        }

        return false;
    }
}
