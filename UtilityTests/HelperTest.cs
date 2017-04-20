using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utility;

namespace UtilityTests
{
	[TestClass]
	public class HelperTest
	{
		[TestMethod]
		public void CopyFolderTest()
		{
			if(Directory.Exists("dir"))
				Directory.Delete("dir", true);
			if(Directory.Exists("dir2"))
				Directory.Delete("dir2", true);
			Directory.CreateDirectory("dir");
			Directory.CreateDirectory("dir2");
			var tempFile = File.CreateText("dir\\ff.txt");
			tempFile.WriteLine("f");
			tempFile.Close();
			Helper.CopyFolder("dir", "dir2");
			Assert.IsTrue(File.Exists("dir2\\ff.txt"));
		}

		[TestMethod]
		public void ValidUrlTest() => Assert.IsTrue(Helper.IsValidUrl("https://google.com"));

		[TestMethod]
		public void WindowsBuiltTest() => Assert.IsNotNull(Helper.GetWindowsBuild());

		[TestMethod]
		public void WindowsVersionTest() => Assert.IsNotNull(Helper.GetWindowsVersion());

		[TestMethod]
		public void Windows10Test() => Assert.IsNotNull(Helper.IsWindows10());
	}
}
