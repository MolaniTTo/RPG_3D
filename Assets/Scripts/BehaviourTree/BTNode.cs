using UnityEngine;

public abstract class BTNode : ScriptableObject
{
    public abstract bool Execute(EnemyAI enemy); //m�tode que s'ha de sobreescriure en les classes filles
}