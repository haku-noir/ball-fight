using Unity.Entities;
using Unity.Physics;
using Unity.Jobs;

public class MovePlayerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .WithAll<PlayerTag>()
            .ForEach((ref PhysicsVelocity physicsVelocity, in PhysicsMass physicsMass, in Force force) =>
            {
                physicsVelocity.Linear += physicsMass.InverseMass * force.Value * deltaTime;
            })
            .Run();
    }
}
