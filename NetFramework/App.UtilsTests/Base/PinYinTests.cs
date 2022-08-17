using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils.Tests
{
    [TestClass()]
    public class PinYinTests
    {
        [TestMethod()]
        public void ToPinYinTest()
        {
            // 同样的代码，netframework ok， netcore 失败，估计字符集有问题。
            var txt = "你好,中国";
            Assert.AreEqual(txt.ToPinYin(),    "NiHao,ZhongGuo");
            Assert.AreEqual(txt.ToPinYinCap(), "NHZG");
        }
    }
}