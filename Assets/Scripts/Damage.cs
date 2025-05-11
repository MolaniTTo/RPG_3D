using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage: MonoBehaviour
{
    public float damage;
    public float damageCooldown = 1f;
    private float lastPlayerHitTime = -Mathf.Infinity;
    private void OnTriggerEnter(Collider other)
    {
        /*if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            EnemyAI enemyAI = other.GetComponentInParent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.Hurt(damage);
            }
        }*/
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (Time.time - lastPlayerHitTime < damageCooldown)
                return;

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                player.Hurt(damage);
                lastPlayerHitTime = Time.time;
            }
        }
    }
}