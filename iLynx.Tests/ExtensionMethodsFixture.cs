using System;
using NUnit.Framework;

namespace iLynx.Tests
{
    [TestFixture]
    public class ExtensionMethodsFixture
    {
        [Test]
        public void GivenAnArrayOfBytesWhenToStringCalledThenValuesAreStringyfied()
        {
            // Arrange
            var target = new byte[]
                         {
                             0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0
                         };
            const string expected = "00:01:02:03:04:05:06:07:08:09:00";
            const string splitter = ":";

            // Act
            var result = target.ToString(splitter);

            // Assert
            Assert.AreEqual(expected, result);
        }

        //public void GivenAn
    }
}
