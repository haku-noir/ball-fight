using Unity.Entities;
using Unity.Physics;

public class AddForceToPlayerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .WithAll<PlayerTag>()
            .ForEach((ref PhysicsVelocity physicsVelocity, in PhysicsMass physicsMass, in Force force) =>
            {
                physicsVelocity.Linear += physicsMass.InverseMass * force.Direction * force.Magnitude * deltaTime;
            })
            .Run();
    }
}
