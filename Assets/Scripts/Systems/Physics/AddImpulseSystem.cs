using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
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
        var addImpulseJob = new AddImpulseJob()
        {
            AllVelocities = GetComponentDataFromEntity<PhysicsVelocity>(false),
            AllMasses = GetComponentDataFromEntity<PhysicsMass>(true),
            AllImpulses = GetComponentDataFromEntity<Impulse>(true)
        };

        var increaseImpulseJob = new IncreaseImpulseJob()
        {
            AllImpulses = GetComponentDataFromEntity<Impulse>(false),
            AllVelocities = GetComponentDataFromEntity<PhysicsVelocity>(true),
            AllMasses = GetComponentDataFromEntity<PhysicsMass>(true),
            AllAttacks = GetComponentDataFromEntity<Attack>(true)
        };

        var firstDependency = addImpulseJob.Schedule(
            stepPhysicsWorld.Simulation,
            ref buildPhysicsWorld.PhysicsWorld,
            Dependency);

        var secondDependency = increaseImpulseJob.Schedule(
            stepPhysicsWorld.Simulation,
            ref buildPhysicsWorld.PhysicsWorld,
            firstDependency);

        Dependency = secondDependency;
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
                var impulseA = AllImpulses[entityA].Magnitude;
                var impulseB = AllImpulses[entityB].Magnitude;

                var IA = (vA - vB) * (1 + impulseA) / (miA + miB);
                var IB = (vB - vA) * (1 + impulseB) / (miA + miB);

                var vA2 = AllVelocities[entityA];
                vA2.Linear += IA * miA;
                AllVelocities[entityA] = vA2;

                var vB2 = AllVelocities[entityB];
                vB2.Linear += IB * miB;
                AllVelocities[entityB] = vB2;
            }
        }

        private bool Exists(Entity entity)
        {
            return AllVelocities.Exists(entity) && AllMasses.Exists(entity) && AllImpulses.Exists(entity);
        }
    }

    struct IncreaseImpulseJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<Impulse> AllImpulses;
        [ReadOnly] public ComponentDataFromEntity<Attack> AllAttacks;
        [ReadOnly] public ComponentDataFromEntity<PhysicsVelocity> AllVelocities;
        [ReadOnly] public ComponentDataFromEntity<PhysicsMass> AllMasses;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            if (Exists(entityA) && Exists(entityB))
            {
                var magA = Magnitude(AllVelocities[entityA].Linear);
                var magB = Magnitude(AllVelocities[entityB].Linear);
                var miA = AllMasses[entityA].InverseMass;
                var miB = AllMasses[entityB].InverseMass;

                if (AllImpulses.Exists(entityA) && AllAttacks.Exists(entityB))
                {
                    var attack = AllAttacks[entityB].Value;

                    var damage = (magA / miA - magB / miB) * attack;
                    damage = damage < 0 ? -damage : 0;

                    var impulse = AllImpulses[entityA];
                    impulse.Magnitude += damage;
                    AllImpulses[entityA] = impulse;
                }

                if (AllAttacks.Exists(entityA) && AllImpulses.Exists(entityB))
                {
                    var attack = AllAttacks[entityA].Value;

                    var damage = (magB / miB - magA / miA) * attack;
                    damage = damage < 0 ? -damage : 0;

                    var impulse = AllImpulses[entityB];
                    impulse.Magnitude += damage;
                    AllImpulses[entityB] = impulse;
                }
            }
        }

        private bool Exists(Entity entity)
        {
            return AllVelocities.Exists(entity) && AllMasses.Exists(entity);
        }

        private float Magnitude(float3 vector)
        {
            return math.sqrt(math.pow(vector.x, 2) + math.pow(vector.y, 2) + math.pow(vector.z, 2));
        }
    }
}