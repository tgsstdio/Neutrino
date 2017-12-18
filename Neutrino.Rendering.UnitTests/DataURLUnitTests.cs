using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Neutrino.UnitTests
{
    [TestClass]
    public class DataURLUnitTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var result = DataURL.FromUri("", out DataURL actual);
            Assert.IsFalse(result);
            Assert.IsNull(actual);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var result = DataURL.FromUri("data:,A%20brief%20note", out DataURL actual);
            Assert.IsTrue(result);
            Assert.IsNotNull(actual);
            Assert.AreEqual("text/plain", actual.MediaType);
            Assert.AreEqual("US-ASCII", actual.CharSet);
            Assert.IsNotNull(actual.Data);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var result = DataURL.FromUri(
                @"data:image/gif;base64,R0lGODdhMAAwAPAAAAAAAP///ywAAAAAMAAw
                AAAC8IyPqcvt3wCcDkiLc7C0qwyGHhSWpjQu5yqmCYsapyuvUUlvONmOZtfzgFz
                ByTB10QgxOR0TqBQejhRNzOfkVJ + 5YiUqrXF5Y5lKh / DeuNcP5yLWGsEbtLiOSp
                a / TPg7JpJHxyendzWTBfX0cxOnKPjgBzi4diinWGdkF8kjdfnycQZXZeYGejmJl
                ZeGl9i2icVqaNVailT6F5iJ90m6mvuTS4OK05M0vDk0Q4XUtwvKOzrcd3iq9uis
                F81M1OIcR7lEewwcLp7tuNNkM3uNna3F2JQFo97Vriy / Xl4 / f1cf5VWzXyym7PH
                hhx4dbgYKAAA7", out DataURL actual);
            Assert.IsTrue(result);
            Assert.IsNotNull(actual);
            Assert.AreEqual("image/gif", actual.MediaType);
            Assert.AreEqual("US-ASCII", actual.CharSet);
            Assert.IsNotNull(actual.Data);
        }
    }
}
