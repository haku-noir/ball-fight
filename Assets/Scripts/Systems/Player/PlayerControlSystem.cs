using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlayerControlSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        Entities
            .WithAll<PlayerTag>()
            .ForEach((ref Force force) =>
            {
                force.Direction = math.float3(horizontalInput, 0, verticalInput);
            })
            .Run();
    }
}
