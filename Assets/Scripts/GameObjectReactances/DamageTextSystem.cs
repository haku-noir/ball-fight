using Unity.Entities;
using UnityEngine;

public class DamageTextSystem : SystemBase
{
    private DamageTextBehaviour[] beahviours;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<PlayerTag>();
        RequireSingletonForUpdate<EnemyTag>();

        beahviours = GameObject.FindObjectsOfType<DamageTextBehaviour>();
    }

    protected override void OnUpdate()
    {
        var playerEntity = GetSingletonEntity<PlayerTag>();
        var enemyEntity = GetSingletonEntity<EnemyTag>();

        foreach(DamageTextBehaviour behaviour in beahviours)
        {
            if (behaviour.gameObject.name == "PlayerDamageText")
                behaviour.React(EntityManager.GetComponentData<Damage>(playerEntity));

            if (behaviour.gameObject.name == "EnemyDamageText")
                behaviour.React(EntityManager.GetComponentData<Damage>(enemyEntity));
        }
    }
}
