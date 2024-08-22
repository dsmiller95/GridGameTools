using Dman.GridGameTools.DataStructures;
using NUnit.Framework;

namespace Dman.GridGameTools.Tests.DataStructures
{
    public class TestListPool
    {
        [Test]
        public void TestListPool_RentsLargestCapacity_First()
        {
            var pool = ListPool<string>.Create(3);
            var list1 = pool.Rent();
            var list2 = pool.Rent();
            list1.AddRange(new [] {"a", "b", "c", "d", "e"});
            var list1Capacity = list1.Capacity;
            list2.AddRange(new [] {"a", "b", "c", "d", "e", "f", "g"});
            var list2Capacity = list2.Capacity;
            pool.Return(list1);
            pool.Return(list2);

            var list3 = pool.Rent();
            var list4 = pool.Rent();
            var list5 = pool.Rent();
            
            Assert.AreEqual(0, list3.Count);
            Assert.AreEqual(0, list4.Count);
            Assert.AreEqual(0, list5.Count);
            
            Assert.AreEqual(list2Capacity, list3.Capacity);
            Assert.AreEqual(list1Capacity, list4.Capacity);
            Assert.AreEqual(3, list5.Capacity);
            
            Assert.AreSame(list2, list3);
            Assert.AreSame(list1, list4);
        }
    }
}