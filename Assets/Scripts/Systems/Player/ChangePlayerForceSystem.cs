using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ChangePlayerForceSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        Entities
            .WithAll<PlayerTag>()
            .ForEach((ref Force force) =>
            {
                force.Value = math.float3(horizontalInput * 10, 0, verticalInput * 10);
            })
            .Run();
    }
}
