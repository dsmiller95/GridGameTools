
using JetBrains.Annotations;

public interface IWorldComponent
{
    [CanBeNull] public IWorldComponentWriter GetWriter();
}

public interface IWorldComponentWriter
{
    public IWorldComponent BakeImmutable();
}
