
namespace Dman.GridGameTools.WorldBuilding
{
    public interface IProvideWorldBuilder
    {
        public WorldBuilder GetBuilder();
        public void Accept(WorldBuilder worldBuilder);
    }

    public static class ProvideWorldBuilderExtensions
    {
        public static void BuildInto(this WorldBuilder builder, IProvideWorldBuilder acceptor)
        {
            acceptor.Accept(builder);
        }
    }
}