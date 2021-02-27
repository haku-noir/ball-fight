using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Force : IComponentData
{
    public float3 Value;
}
