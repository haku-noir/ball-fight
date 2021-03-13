using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class PlayerCollisionSystem : SystemBase
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
            AllDamages = GetComponentDataFromEntity<Damage>(true)
        };

        var causeDamageJob = new CauseDamageJob()
        {
            AllDamages = GetComponentDataFromEntity<Damage>(false),
            AllVelocities = GetComponentDataFromEntity<PhysicsVelocity>(true),
            AllMasses = GetComponentDataFromEntity<PhysicsMass>(true),
            AllAttacks = GetComponentDataFromEntity<Attack>(true)
        };

        var firstDependency = causeDamageJob.Schedule(
            stepPhysicsWorld.Simulation,
            ref buildPhysicsWorld.PhysicsWorld,
            Dependency);

        var secondDependency = addImpulseJob.Schedule(
            stepPhysicsWorld.Simulation,
            ref buildPhysicsWorld.PhysicsWorld,
            firstDependency);

        Dependency = secondDependency;
    }

    struct AddImpulseJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<PhysicsVelocity> AllVelocities;
        [ReadOnly] public ComponentDataFromEntity<PhysicsMass> AllMasses;
        [ReadOnly] public ComponentDataFromEntity<Damage> AllDamages;

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
                var damageA = AllDamages[entityA].Value;
                var damageB = AllDamages[entityB].Value;

                var IA = (vA - vB) * (1 + damageA) / (miA + miB);
                var IB = (vB - vA) * (1 + damageB) / (miA + miB);

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
            return AllVelocities.Exists(entity) && AllMasses.Exists(entity) && AllDamages.Exists(entity);
        }
    }

    struct CauseDamageJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<Damage> AllDamages;
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

                if (AllDamages.Exists(entityA) && AllAttacks.Exists(entityB))
                {
                    var attackB = AllAttacks[entityB].Value;

                    var damage = (magA / miA - magB / miB) * attackB;
                    damage = damage < 0 ? -damage : 0;

                    var damageA2 = AllDamages[entityA];
                    damageA2.Value += damage;
                    AllDamages[entityA] = damageA2;
                }

                if (AllAttacks.Exists(entityA) && AllDamages.Exists(entityB))
                {
                    var attackA = AllAttacks[entityA].Value;

                    var damage = (magB / miB - magA / miA) * attackA;
                    damage = damage < 0 ? -damage : 0;

                    var impulseB2 = AllDamages[entityB];
                    impulseB2.Value += damage;
                    AllDamages[entityB] = impulseB2;
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