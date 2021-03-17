using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BallSpawnerData : IComponentData
{
    public Entity BallPrefabEntitiy;
    public float3 Position;
}
