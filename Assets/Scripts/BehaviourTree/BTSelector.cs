using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BTSelector", menuName = "BehaviourTree/Selector")]
public class BTSelector : BTNode
{
    public List<BTNode> children = new List<BTNode>();

    public override bool Execute(EnemyAI enemy)
    {
        foreach (BTNode node in children)
        {
            if (node.Execute(enemy)) return true; // Si uno tiene éxito, el selector se detiene.
        }
        return false; // Si todos fallan, devuelve false.
    }
}
