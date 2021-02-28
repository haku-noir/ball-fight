using Unity.Entities;
using Unity.Physics;

public class AddForceSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities
            .ForEach((ref PhysicsVelocity physicsVelocity, in PhysicsMass physicsMass, in Force force) =>
            {
                physicsVelocity.Linear += physicsMass.InverseMass * force.Direction * force.Magnitude * deltaTime;
            })
            .Run();
    }
}
