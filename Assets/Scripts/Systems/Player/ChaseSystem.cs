using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ChaseSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach((ref ChaserData chaser) =>
            {
                ComponentDataFromEntity<Translation> allTranslations = GetComponentDataFromEntity<Translation>(true);

                if (!allTranslations.Exists(chaser.targetEntity)) return;
                Translation targetTranslation = allTranslations[chaser.targetEntity];

                chaser.targetTranslation = targetTranslation.Value;
            })
            .ScheduleParallel();

        Entities
            .ForEach((ref Force force, in Translation translation, in ChaserData chaser) =>
            {
                var dx = chaser.targetTranslation.x - translation.Value.x;
                var dz = chaser.targetTranslation.z - translation.Value.z;
                var magnitude = math.sqrt(math.pow(dx, 2) + math.pow(dz, 2));

                force.Direction = math.float3(dx / magnitude, 0, dz / magnitude);
            })
            .ScheduleParallel();
    }
}
