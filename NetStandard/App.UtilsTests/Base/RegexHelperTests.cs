﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace App.Utils.Tests
{
    /// <summary>
    /// 内容块
    /// </summary>
    public class ContentPart
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public ContentPart(string type, string content)
        {
            this.Type = type;
            this.Content = content;
        }
        public override string ToString()
        {
            return this.Content;
        }
    }


    [TestClass()]
    public class RegexHelperTests
    {
        //--------------------------------------------------
        // Match like
        //--------------------------------------------------
        [TestMethod()]
        public void IsMatchTest()
        {
            Assert.IsTrue(RegexHelper.IsMatch("AABB", RegexHelper.AABB));
            Assert.IsTrue(RegexHelper.IsMatch("开开心心", RegexHelper.AABB));
            Assert.IsTrue(RegexHelper.IsMatch("ABAB", RegexHelper.ABAB));
            Assert.IsTrue(RegexHelper.IsMatch("快活快活", RegexHelper.ABAB));
            Assert.IsTrue(RegexHelper.IsMatch("0", RegexHelper.Int));
            Assert.IsTrue(RegexHelper.IsMatch("90", RegexHelper.Int));
            Assert.IsTrue(RegexHelper.IsMatch("90.8", RegexHelper.Float));
            Assert.IsTrue(RegexHelper.IsMatch("-0", RegexHelper.Int));
            Assert.IsTrue(RegexHelper.IsMatch("-1", RegexHelper.NegativeInt));
        }

        [TestMethod()]
        public void LikeTest()
        {
            Assert.IsTrue("HelloWorld".IsLike("*Wor*"));
            Assert.IsTrue("HelloWorld".IsLike("%Wor%"));
            Assert.IsTrue("HelloWorld".IsLike("_____Wor*"));
            Assert.IsTrue("HelloWorld".Contains("Wor"));
        }



        //--------------------------------------------------
        // Find
        //--------------------------------------------------
        [TestMethod()]
        public void FindFirstTest()
        {
            string result1 = RegexHelper.FindFirst(@"<html><title>This is title</title></html>", @"<title>(?<title>.*?)</title>", "$1");
            string result2 = RegexHelper.FindFirst(@"<html><title>This is title</title></html>", @"<title>(?<title>.*?)</title>", "title");
            string result3 = RegexHelper.FindFirst(@"<html><title>This is title</title></html>", @"<title>(?<title>.*?)</title>", "${title}");
            string result4 = RegexHelper.FindFirst(@"<html><title>This is title</title></html>", @"<title>.*</title>", "");
            string result6 = RegexHelper.FindFirst(@"World1 Hello World2", @"Wor\S*");
            Assert.AreEqual(result1, "This is title");
            Assert.AreEqual(result2, "This is title");
            Assert.AreEqual(result3, "This is title");
            Assert.AreEqual(result4, "<title>This is title</title>");
            Assert.AreEqual(result6, "World1");
        }


        [TestMethod()]
        public void FindTest()
        {
            var result = RegexHelper.Find(@"World1 Hello World2", @"Wor\S*");
            Assert.AreEqual(result.Count, 2);
        }

        [TestMethod()]
        public void FindJoinTest()
        {
            var result = RegexHelper.FindJoin(@"World1 Hello World2", @"Wor\S*");
            Assert.AreEqual(result, "World1World2");
        }


        [TestMethod()]
        public void FindWordsTest()
        {
            var text = "hello world,你好+世界, '测试+System'";
            var list = RegexHelper.FindWords(text);
            Assert.AreEqual(list.Count, 6);
        }

        [TestMethod()]
        public void FindRepeatWordTest()
        {
            var txt = "Test  Apple Test Apple,Test Kitty Apple Test Apple";
            var list = RegexHelper.FindRepeatWords(txt);
            Assert.AreEqual(list.Count, 2);
        }



        [TestMethod()]
        public void ParseViewTest()
        {
            var text = @"
                <p>政企主流套餐</p>
                <h1>testsf</h1>
                <br/>
                <a href='x' >link</a>
                <p>
                    <h1>testsf</h1>
                    <img src='http://122.229.31.237:88/Files/Editors/62021739-5b5c-4383-820c-bea96ae36e7b.png' style='width: 32px; height: 32px;'/>
                </p>
                ";
            var items = ParseViews(text);
            Console.Write(items.ToJson());
        }

        //--------------------------------------------------
        // Replace
        //--------------------------------------------------
        // 除了img标签以外，所有的标签都清空
        public static string ReplaceMatch(Match m)
        {
            var txt = m.Value.ToLower();
            if (txt.StartsWith("<img"))
                return txt;
            return "";
        }

        /// <summary>解析标签</summary>
        public static List<ContentPart> ParseViews(string text)
        {
            // 预处理所有结对标签，弄成平面文档
            //text = text.Replace("<p>", "<br/>").Replace(@"</p>", @"\r");
            text = text.Replace("<br/>", "\r").Replace("<br>", "\r");                       // 换行符：改为回车  
            text = Regex.Replace(text, @"<!-[^>]*->", "");                                  // 注释：去掉
            text = Regex.Replace(text, @"<\/[^>]*>", "\r");                                 // 结对标签尾：改为回车
            //text = Regex.Replace(text, @"<[^>]*>", "");                                   // 结对标签头：去掉

            text = Regex.Replace(text, @"<[^>]*>", new MatchEvaluator(ReplaceMatch));                 // 除了img标签外，去除所有单标签头
            text = Regex.Replace(text, @"[\r\n]{2,}", "\r", RegexOptions.IgnoreCase);       // 合并多个回车换行
            text = Regex.Replace(text, @"[\t\v ]{2,}", " ", RegexOptions.IgnoreCase);       // 合并多个空格
            text = text.Trim();

            // 剩下的就是单标签和回车符
            List<ContentPart> items = new List<ContentPart>();
            var parts = text.Split(new string[] { "\r" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                // 尝试解析图像标签
                string pattern = @"<img\s*src=['""](?<src>[^'""]*)['""].*>";
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var m = r.Match(part);

                if (m.Success)
                    items.Add(new ContentPart("img", m.Result("${src}")));
                else
                    items.Add(new ContentPart("text", part.RemoveHtml()));
            }

            return items;
        }




        [TestMethod()]
        public void ReplaceRegexTest()
        {
            //
            var txt = "03/01/2019";
            var day1 = txt.ReplaceRegex(
                @"\b(\d{1,2})/(\d{1,2})/(\d{2,4})\b",
                "$3-$1-$2"
               );
            var day2 = txt.ReplaceRegex(
                 @"\b(?<month>\d{1,2})/(?<day>\d{1,2})/(?<year>\d{2,4})\b",
                 "${year}-${month}-${day}"
                );
            Assert.AreEqual(day1, "2019-03-01");
            Assert.AreEqual(day2, "2019-03-01");

            //
            var text1 = "world wororld worororld";
            var text2 = text1.ReplaceRegex(@"wor\w*ld", (m) => m.Length.ToString());
            Assert.AreEqual(text2, "5 7 9");
        }


        [TestMethod()]
        public void ParseUrlTest()
        {
            RegexHelper.ParseUrl("http://www.test.com:8080/", out string proto, out string host, out string port);
            Assert.AreEqual(proto, "http");
            Assert.AreEqual(host, "www.test.com");
            Assert.AreEqual(port, "8080");
            RegexHelper.ParseUrl("http://www.test.com/", out string proto2, out string host2, out string port2);
            Assert.AreEqual(proto2, "http");
            Assert.AreEqual(host2, "www.test.com");
            Assert.AreEqual(port2, "");
        }

        [TestMethod()]
        public void MMDDYY2YYMMDDTest()
        {
            Assert.AreEqual(RegexHelper.MMDDYY2YYMMDD("03/01/2019"), "2019-03-01");
        }

        //-----------------------------------------------------
        // 获取特殊规则字符串
        //-----------------------------------------------------
        [TestMethod()]
        public void FindMobileTest()
        {
            Assert.AreEqual(RegexHelper.FindMobile("Hello15305770001"), "15305770001");
        }

        [TestMethod()]
        public void FindNumberTest()
        {
            Assert.AreEqual(RegexHelper.FindNumber("Hello2000.04中国"), "2000.04");
        }

        [TestMethod()]
        public void FindCharsTest()
        {
            Assert.AreEqual(RegexHelper.FindChars("Hello2000中国"), "Hello中国");
        }

        [TestMethod()]
        public void FindEnglishCharsTest()
        {
            Assert.AreEqual(RegexHelper.FindEnglishChars("Hello2000中国"), "Hello");
        }

        [TestMethod()]
        public void FindChineseCharsTest()
        {
            Assert.AreEqual(RegexHelper.FindChineseChars("Hello2000中国"), "中国");
        }
    }
}