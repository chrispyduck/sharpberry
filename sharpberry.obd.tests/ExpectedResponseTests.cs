using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using sharpberry.obd.Responses;

namespace sharpberry.obd.tests
{
    [TestFixture]
    public class ExpectedResponseTests
    {
        [Test]
        public void Bytes()
        {
            var four = ExpectedResponse.ByteCount(4);
            Assert.AreEqual(ResponseStatus.Incomplete, four.CheckInput("abcdef"));
            Assert.AreEqual(ResponseStatus.Valid, four.CheckInput("abcdef01"));
            Assert.AreEqual(ResponseStatus.Invalid, four.CheckInput("abcdef0102"));
        }

        [Test]
        public void Ok()
        {
            var er = ExpectedResponse.Ok;
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput("OK"));
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput("ok"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("O"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("o"));
            Assert.AreEqual(ResponseStatus.Invalid, er.CheckInput("OKIE"));
        }

        [Test]
        public void StringNoExtra()
        {
            var er = ExpectedResponse.String("abcdef");
            Assert.AreEqual(ResponseStatus.Invalid, er.CheckInput("b"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("a"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("ab"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("abc"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("abcd"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("abcde"));
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput("abcdef"));
            Assert.AreEqual(ResponseStatus.Invalid, er.CheckInput("abcdefg"));
        }

        [Test]
        public void StringAllowExtra()
        {
            var er = ExpectedResponse.String("abcdef", true);
            Assert.AreEqual(ResponseStatus.Invalid, er.CheckInput("b"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("a"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("ab"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("abc"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("abcd"));
            Assert.AreEqual(ResponseStatus.Incomplete, er.CheckInput("abcde"));
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput("abcdef"));
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput("abcdefg"));
        }

        [Test]
        public void Any()
        {
            var er = ExpectedResponse.Any;
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput(null));
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput(""));
            Assert.AreEqual(ResponseStatus.Valid, er.CheckInput("a"));
        }
    }
}
