using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class DropBallTriggerSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    private EndSimulationEntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();

        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var dropBallTriggerJob = new DropBallTriggerJob()
        {
            AllBallDatas = GetComponentDataFromEntity<BallData>(true),
            AllDropCheckers = GetComponentDataFromEntity<DropCheckerTag>(true),
            EntityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer()
        };

        Dependency = dropBallTriggerJob.Schedule(
            stepPhysicsWorld.Simulation,
            ref buildPhysicsWorld.PhysicsWorld,
            Dependency);

        entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }

    struct DropBallTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<BallData> AllBallDatas;
        [ReadOnly] public ComponentDataFromEntity<DropCheckerTag> AllDropCheckers;
        
        public EntityCommandBuffer EntityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            if(AllBallDatas.Exists(entityA) && AllDropCheckers.Exists(entityB))
            {
                EntityCommandBuffer.DestroyEntity(entityA);
            }
            if (AllDropCheckers.Exists(entityA) && AllBallDatas.Exists(entityB))
            {
                EntityCommandBuffer.DestroyEntity(entityB);
            }
        }
    }
}