﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils.Tests
{
    [TestClass()]
    public class IOTests
    {
        [TestMethod()]
        public void GetFileNameTest()
        {
            string url = "http://oa.wzcc.com/oamain.aspx?a=x&b=xx";
            var name = url.GetFileName();
            var ext = url.GetFileExtension();
            var folder = url.GetFileFolder();
            var q = url.GetQuery().ToString();
            var u = url.TrimQuery();
        }

        [TestMethod()]
        public void GetNextNameTest()
        {
            var name1 = "c:\\folder\\filename.doc?x=1";

            //
            var name2 = name1.GetNextName(@"_{0}").GetNextName(@"_{0}");
            Assert.AreEqual(name2, "c:\\folder\\filename_3.doc?x=1");

            //
            var name3 = name1.GetNextName(@"-{0}").GetNextName(@"-{0}");
            Assert.AreEqual(name3, "c:\\folder\\filename-3.doc?x=1");

            //
            var name4 = name1.GetNextName(@"({0})").GetNextName(@"({0})");
            Assert.AreEqual(name4, "c:\\folder\\filename(3).doc?x=1");
        }

        [TestMethod()]
        public void GetCacheTest()
        {
            var name1 = Cacher.Get("Name", () => "Kevin");
            var name2 = Cacher.Get<string>("Name");
            Assert.AreEqual(name1, name2);

            Cacher.Set("Name", "John");
            var name3 = Cacher.Get<string>("Name");
            Assert.AreEqual(name3, "John");

            var p1 = Cacher.Get<Person>("Kevin", () => new Person("Kevin"));
            var p2 = Cacher.Get<Person>("Cherry", () => new Person("Cherry"));
            var p3 = Cacher.Get<Person>("Kevin");
            var p4 = Cacher.Get<Person>("Jerry");
            Assert.AreEqual(p1.Name, p3.Name);
            Assert.AreEqual(p4, null);
        }


        [TestMethod()]
        public void GetMimeTypeTest()
        {
            var url1 = "xxx.jpg?test=x";
            var url2 = "/a/b/xxx.doc?test=x";
            var url3 = "/a/b/xxx.xxx?test=x";
            var url4 = "/a/b/xxx?test=x";

            Assert.AreEqual(url1.GetMimeType(), @"image/jpeg");
            Assert.AreEqual(url2.GetMimeType(), @"application/msword");
            Assert.AreEqual(url3.GetMimeType(), @"application/octet-stream");
            Assert.AreEqual(url4.GetMimeType(), @"application/octet-stream");
        }

        [TestMethod()]
        public void GetFileFolderTest()
        {
            var url1 = @"xxx.jpg?test=x";
            var url2 = @"a/b/xxx.jpg?test=x";
            var url3 = @"a\b\xxx.jpg?test=x";
            Assert.AreEqual(url1.GetFileFolder(), "");
            Assert.AreEqual(url2.GetFileFolder(), @"a/b");
            Assert.AreEqual(url3.GetFileFolder(), @"a\b");
        }

        [TestMethod()]
        public void GetFileExtensionTest()
        {
            var url1 = @"xxx.jpg?test=x";
            var url2 = @"a/b/xxx.Jpg?test=x";
            var url3 = @"a\b\xxx?test=x";
            Assert.AreEqual(url1.GetFileExtension(), @".jpg");
            Assert.AreEqual(url2.GetFileExtension(), @".jpg");
            Assert.AreEqual(url3.GetFileExtension(), @"");
        }

        [TestMethod()]
        public void GetAppSettingTest()
        {
            var o1 = IO.GetAppSetting<int?>("MachineID");
            var o2 = IO.GetAppSetting<int?>("machineID");
            var o3 = IO.GetAppSetting<int?>("NotExist");
            Assert.AreEqual(o1, o2);
            Assert.AreEqual(o3, null);
        }

        [TestMethod()]
        public void IPsTest()
        {
            var ips = Net.IPs;
            ips.ForEach(t => IO.Trace(t));
        }

        [TestMethod()]
        public void ToRelativePathTest()
        {
            var root = @"c:\";
            Assert.AreEqual(@"c:\test\".ToRelativePath(root), @"\test\");
            Assert.AreEqual(@"d:\test\".ToRelativePath(root), @"");
        }

        [TestMethod()]
        public void CombinePathTest()
        {
            Assert.AreEqual(@"".CombinePath(@"index.aspx"), @"index.aspx");
            Assert.AreEqual(@"\Admins\".CombinePath(@"index.aspx"), @"\Admins\index.aspx");
            Assert.AreEqual(@"\Admins".CombinePath(@"index.aspx"), @"\Admins\index.aspx");
            Assert.AreEqual(@"/Admins\".CombinePath(@"index.aspx"), @"\Admins\index.aspx");
            Assert.AreEqual(@"\Admins\".CombinePath(@"\Test\index.aspx"), @"\Admins\Test\index.aspx");
        }

        [TestMethod()]
        public void CombineWebPathTest()
        {
            Assert.AreEqual(@"".CombineWebPath(@"index.aspx"), @"index.aspx");
            Assert.AreEqual(@"\Admins\".CombineWebPath(@"index.aspx"), @"/Admins/index.aspx");
            Assert.AreEqual(@"\Admins".CombineWebPath(@"index.aspx"), @"/Admins/index.aspx");
            Assert.AreEqual(@"/Admins\".CombineWebPath(@"index.aspx"), @"/Admins/index.aspx");
            Assert.AreEqual(@"\Admins\".CombineWebPath(@"\Test\index.aspx"), @"/Admins/Test/index.aspx");
        }

        [TestMethod()]
        public void PrepareDirectoryTest()
        {
            IO.PrepareDirectory(@"c:\test1\test.doc");
            IO.PrepareDirectory(@"c:\test2\");
            IO.PrepareDirectory(@"c:\test3");
            Assert.AreEqual(System.IO.Directory.Exists(@"c:\test1\"), true);
            Assert.AreEqual(System.IO.Directory.Exists(@"c:\test2\"), true);
            Assert.AreEqual(System.IO.Directory.Exists(@"c:\test3\"), true);
        }

        [TestMethod()]
        public void WriteFileTest()
        {
            var path = string.Format("{0}\\log.txt", Environment.CurrentDirectory);
            var txt1 = "_text_";
            IO.DeleteFile(path);
            IO.DeleteFile(path);

            // 附加文件
            IO.WriteFile(path, txt1, true);
            IO.WriteFile(path, txt1, true);
            var txt3 = IO.ReadFileText(path);
            Assert.AreEqual(txt1 + txt1, txt3);

            // 新建文件
            IO.WriteFile(path, txt1, false);
            var txt2 = IO.ReadFileText(path);
            Assert.AreEqual(txt1, txt2);
        }

    }
}