using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utility;

namespace UtilityTests
{
	[TestClass]
	public class LogTest
	{
		[TestMethod]
		public void LogTest1()
		{
			if(File.Exists("f.txt"))
				File.Delete("f.txt");
			Log.Initialize(Directory.GetCurrentDirectory(), "f.txt");
			Assert.IsTrue(File.Exists("f.txt"));
		}
	}
}
