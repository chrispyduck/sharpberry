using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using sharpberry.data.collection;
using sharpberry.data.storage;
using sharpberry.data.types;

namespace sharpberry.data.tests
{
    [TestFixture]
    public class DataCollectorTests
    {
        class Point
        {
            public int X;
            public int Y;
        }
        class MockCollector : DataCollector<Point>
        {
            public int add = 0;

            public override void Initialize()
            {
            }
            internal override void AddSampleToTrip(Trip trip, Point sample, IDataProvider dataProvider)
            {
                add++;
            }
            internal override IEnumerable<Point> GetSamples()
            {
                return Enumerable.Range(1, 10).Select(i => new Point {X = i, Y = i*4});
            }
        }

        [Test]
        public void Collect()
        {
            var testTrip = new Trip();
            var provider = new Mock<IDataProvider>();
            var collector = new MockCollector();
            Assert.AreEqual(0, collector.add);
            collector.Collect(testTrip, provider.Object);
            Assert.AreEqual(10, collector.add);
        }
    }
}
