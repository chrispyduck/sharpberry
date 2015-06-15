using System;
using System.Collections.Specialized;
using NUnit.Framework;

namespace sharpberry.configuration.tests
{
    [TestFixture]
    public class ExtensionTests
    {
        class Test
        {
            public Test()
            {
                this.F = new Foo();
            }
            public string X { get; set; }
            public int Y { get; set; }
            public Foo F { get; set; }
        }
        class Foo
        {
            public string R { get; set; }
        }
        [Test]
        public void ImportKeyValuePair()
        {
            var test = new Test();
            test.ImportKeyValuePair("X", "testing");
            Assert.AreEqual("testing", test.X);

            test.ImportKeyValuePair("Y", "32");
            Assert.AreEqual(32, test.Y);

            test.ImportKeyValuePair("F.R", "yello");
            Assert.AreEqual("yello", test.F.R);
        }

        [Test]
        public void ParseString()
        {
            Assert.AreEqual("test", "test".ParseAs(typeof(string)));
        }

        [Test]
        public void ParseInt()
        {
            Assert.AreEqual(23256, "23256".ParseAs(typeof(int)));
        }

        [Test]
        public void ParseTimeSpan()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(123.4), TimeSpan.FromSeconds(123.4).ToString().ParseAs(typeof(TimeSpan)));
        }

        [Test]
        public void ParseStringArray()
        {
            var obj = "test1,test2;test3".ParseAs(typeof (string[]));
            Assert.IsNotNull(obj);
            var arr = (string[]) obj;
            Assert.IsNotNull(arr);
            Assert.AreEqual(3, arr.Length);
        }

        [Test]
        public void ParseIntArray()
        {
            var obj = "1,2;3".ParseAs(typeof(int[]));
            Assert.IsNotNull(obj);
            var arr = (int[])obj;
            Assert.IsNotNull(arr);
            Assert.AreEqual(3, arr.Length);
        }
    }
}
