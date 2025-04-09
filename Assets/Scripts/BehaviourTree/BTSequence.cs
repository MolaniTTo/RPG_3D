using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BTSequence", menuName = "BehaviourTree/Sequence")]
public class BTSequence : BTNode
{
    public List<BTNode> children = new List<BTNode>(); //llista dels nodes que formen la seqüència

    public override bool Execute(EnemyAI enemy)
    {
        foreach (BTNode node in children)
        {
            if (!node.Execute(enemy)) return false;//si un node falla, la seqüència para
        }
        return true; //si torna true, vol dir que tots els nodes han estat exitosos
    }
}
