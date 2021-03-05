using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class AddImpulseSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        base.OnCreate();

        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var job = new AddImpulseJob()
        {
            AllVelocities = GetComponentDataFromEntity<PhysicsVelocity>(false),
            AllMasses = GetComponentDataFromEntity<PhysicsMass>(true),
            AllImpulses = GetComponentDataFromEntity<Impulse>(true)
        };

        Dependency = job.Schedule(
            stepPhysicsWorld.Simulation,
            ref buildPhysicsWorld.PhysicsWorld,
            Dependency);
    }

    struct AddImpulseJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<PhysicsVelocity> AllVelocities;
        [ReadOnly] public ComponentDataFromEntity<PhysicsMass> AllMasses;
        [ReadOnly] public ComponentDataFromEntity<Impulse> AllImpulses;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            if (Exists(entityA) && Exists(entityB))
            {
                var vA = AllVelocities[entityA].Linear;
                var vB = AllVelocities[entityB].Linear;
                var miA = AllMasses[entityA].InverseMass;
                var miB = AllMasses[entityB].InverseMass;
                var bA = AllImpulses[entityA].Magnitude;
                var bB = AllImpulses[entityB].Magnitude;

                var IA = (vA - vB) * (1 + bA) / (miA + miB);
                var IB = (vB - vA) * (1 + bB) / (miA + miB);

                var vA2 = AllVelocities[entityA];
                vA2.Linear = IA * miA + vA;
                AllVelocities[entityA] = vA2;

                var vB2 = AllVelocities[entityB];
                vB2.Linear = IB * miB + vB;
                AllVelocities[entityB] = vB2;
            }
        }

        private bool Exists(Entity entity)
        {
            return AllVelocities.Exists(entity) && AllMasses.Exists(entity) && AllImpulses.Exists(entity);
        }
    }
}