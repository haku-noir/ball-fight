using Unity.Entities;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct BallData : IComponentData
{
    public NativeString64 Name;
}
