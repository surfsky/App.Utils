using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Utils.Tests
{
    [TestClass()]
    public class ToolTests
    {
        [TestMethod()]
        public void IsEmptyTest()
        {
            Assert.AreEqual("".IsEmpty(), true);
            Assert.AreEqual(Tool.IsEmpty(null), true);
        }
    }
}