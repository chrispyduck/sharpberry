using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using sharpberry.configuration;
using sharpberry.data.collection;
using sharpberry.data.storage;
using sharpberry.data.types;

namespace sharpberry.data.tests
{
    [TestFixture]
    public class DataCollectionManagerTests
    {
        [Test]
        public void StartEndTrip()
        {
            var config = new Config();
            config.DataCollection.CollectionInterval = TimeSpan.FromMilliseconds(100);
            var testTrip = new Trip();
            var providerMock = new Mock<IDataProvider>();
            providerMock.Setup(x => x.StartTrip()).Returns(testTrip);
            providerMock.Setup(x => x.EndTrip(testTrip));
            var dcm = new DataCollectionManager(new IDataCollector[] { }, providerMock.Object, config);
            Assert.IsNull(dcm.CurrentTrip);
            dcm.StartTrip();
            providerMock.Verify(x => x.StartTrip(), Times.Once());
            providerMock.Verify(x => x.EndTrip(It.IsAny<Trip>()), Times.Never());
            Assert.IsNotNull(dcm.CurrentTrip);
            Assert.Throws<InvalidOperationException>(dcm.StartTrip);
            dcm.EndTrip();
            providerMock.Verify(x => x.StartTrip(), Times.Once());
            providerMock.Verify(x => x.EndTrip(It.IsAny<Trip>()), Times.Once());
            providerMock.Verify(x => x.EndTrip(testTrip), Times.Once());
        }

        [Test]
        public void TimedCollect()
        {
            var config = new Config();
            config.DataCollection.CollectionInterval = TimeSpan.FromMilliseconds(100);
            var testTrip = new Trip();
            var providerMock = new Mock<IDataProvider>();
            providerMock.Setup(x => x.StartTrip()).Returns(testTrip);
            providerMock.Setup(x => x.EndTrip(testTrip));

            var collectorMock1 = new Mock<IDataCollector>();
            var collectorMock2 = new Mock<IDataCollector>();
            var dcm = new DataCollectionManager(new IDataCollector[] { collectorMock1.Object, collectorMock2.Object }, providerMock.Object, config);

            // start trip and wait for 3 cycles
            dcm.StartTrip();
            Thread.Sleep((int) (config.DataCollection.CollectionInterval.TotalMilliseconds*3.3));

            // test for 3-4 collection calls
            collectorMock1.Verify(x => x.Collect(testTrip, providerMock.Object), Times.Between(3, 4, Range.Inclusive));
            collectorMock2.Verify(x => x.Collect(testTrip, providerMock.Object), Times.Between(3, 4, Range.Inclusive));

            // end trip and make sure there are no additional calls
            dcm.EndTrip();
            collectorMock1.ResetCalls();
            collectorMock2.ResetCalls();
            Thread.Sleep((int)(config.DataCollection.CollectionInterval.TotalMilliseconds * 3.3));
            collectorMock1.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Never());
            collectorMock2.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Never());
        }

        [Test]
        public void ManualCollect()
        {
            var config = new Config();
            config.DataCollection.CollectionInterval = TimeSpan.MaxValue;
            var testTrip = new Trip();
            var providerMock = new Mock<IDataProvider>();
            providerMock.Setup(x => x.StartTrip()).Returns(testTrip);
            providerMock.Setup(x => x.EndTrip(testTrip));

            var collectorMock1 = new Mock<IDataCollector>();
            var collectorMock2 = new Mock<IDataCollector>();
            var dcm = new DataCollectionManager(new IDataCollector[] { collectorMock1.Object, collectorMock2.Object }, providerMock.Object, config);

            // start trip
            dcm.StartTrip();
            collectorMock1.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Never());
            collectorMock2.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Never());

            // collect and verify
            dcm.Collect();
            collectorMock1.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Once());
            collectorMock2.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Once());

            Thread.Sleep(500);
            collectorMock1.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Once());
            collectorMock2.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Once());

            // end trip and make sure there are no additional calls
            dcm.EndTrip();
            collectorMock1.ResetCalls();
            collectorMock2.ResetCalls();
            Thread.Sleep(500);
            collectorMock1.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Never());
            collectorMock2.Verify(x => x.Collect(It.IsAny<Trip>(), It.IsAny<IDataProvider>()), Times.Never());
        }
    }
}
