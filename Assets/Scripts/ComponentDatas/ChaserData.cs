using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ChaserData : IComponentData
{
    public Entity targetEntity;
    public float3 targetTranslation;
}
