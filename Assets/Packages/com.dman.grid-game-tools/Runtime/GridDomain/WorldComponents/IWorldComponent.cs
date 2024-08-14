using JetBrains.Annotations;

namespace Dman.GridGameTools
{
    public interface IWorldComponent
    {
        [CanBeNull] public IWorldComponentWriter GetWriter();
    }

    public interface IWorldComponentWriter
    {
        public IWorldComponent BakeImmutable();
    }

    public interface IWorldComponentWriterWithHooks : IWorldComponentWriter, IWorldHooks
    {
    }
}