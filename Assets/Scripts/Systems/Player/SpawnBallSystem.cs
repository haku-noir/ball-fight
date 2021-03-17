using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SpawnBallSystem : SystemBase
{
    private EndInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var commandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        Dependency = Entities
            .WithAll<SpawnTag>()
            .ForEach((Entity entity, int entityInQueryIndex, in BallSpawnerData ballSpawnerData, in LocalToWorld localToWorld) =>
            {
                var ballInstance = commandBuffer.Instantiate(entityInQueryIndex, ballSpawnerData.BallPrefabEntitiy);

                commandBuffer.SetComponent(entityInQueryIndex, ballInstance, new Translation { Value = ballSpawnerData.Position });
                commandBuffer.RemoveComponent<SpawnTag>(entityInQueryIndex, entity);
            })
            .ScheduleParallel(Dependency);

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
