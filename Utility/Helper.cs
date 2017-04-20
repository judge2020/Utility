using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Utility
{
    public static class Helper
    {

	    public static string GetValidFilePath(string dir, string name, string extension)
	    {
		    var validDir = RemoveInvalidPathChars(dir);
		    if(!Directory.Exists(validDir))
			    Directory.CreateDirectory(validDir);

		    if(!extension.StartsWith("."))
			    extension = "." + extension;

		    var path = validDir + "\\" + RemoveInvalidFileNameChars(name);
		    if(File.Exists(path + extension))
		    {
			    var num = 1;
			    while(File.Exists(path + "_" + num + extension))
				    num++;
			    path += "_" + num;
		    }

		    return path + extension;
	    }

	    public static string RemoveInvalidPathChars(string s) => RemoveChars(s, Path.GetInvalidPathChars());
	    public static string RemoveInvalidFileNameChars(string s) => RemoveChars(s, Path.GetInvalidFileNameChars());
	    public static string RemoveChars(string s, char[] c) => new Regex($"[{Regex.Escape(new string(c))}]").Replace(s, "");

	    //http://stackoverflow.com/questions/23927702/move-a-folder-from-one-drive-to-another-in-c-sharp
	    public static void CopyFolder(string sourceFolder, string destFolder)
	    {
		    if(!Directory.Exists(destFolder))
			    Directory.CreateDirectory(destFolder);
		    var files = Directory.GetFiles(sourceFolder);
		    foreach(var file in files)
		    {
			    var name = Path.GetFileName(file);
			    var dest = Path.Combine(destFolder, name);
			    File.Copy(file, dest);
		    }
		    var folders = Directory.GetDirectories(sourceFolder);
		    foreach(var folder in folders)
		    {
			    var name = Path.GetFileName(folder);
			    var dest = Path.Combine(destFolder, name);
			    CopyFolder(folder, dest);
		    }
	    }

	    //http://stackoverflow.com/questions/3769457/how-can-i-remove-accents-on-a-string
	    public static string RemoveDiacritics(string src, bool compatNorm)
	    {
		    var sb = new StringBuilder();
		    foreach(var c in src.Normalize(compatNorm ? NormalizationForm.FormKD : NormalizationForm.FormD))
		    {
			    switch(CharUnicodeInfo.GetUnicodeCategory(c))
			    {
				    case UnicodeCategory.NonSpacingMark:
				    case UnicodeCategory.SpacingCombiningMark:
				    case UnicodeCategory.EnclosingMark:
					    break;
				    default:
					    sb.Append(c);
					    break;
			    }
		    }

		    return sb.ToString();
	    }

	    public static DateTime FromUnixTime(long unixTime)
		    => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(TimeSpan.FromSeconds(unixTime)).ToLocalTime();

	    public static DateTime FromUnixTime(string unixTime)
	    {
		    long time;
		    return long.TryParse(unixTime, out time) ? FromUnixTime(time) : DateTime.Now;
	    }

	    public static bool IsWindows10()
	    {
		    try
		    {
			    var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
			    return reg != null && ((string)reg.GetValue("ProductName")).Contains("Windows 10");
		    }
		    catch(Exception ex)
		    {
			    Log.Error(ex);
			    return false;
		    }
	    }

	    public static bool TryOpenUrl(string url, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
	    {
		    try
		    {
			    Log.Info("[Utility.Helper.TryOpenUrl] " + url, memberName, sourceFilePath);
			    Process.Start(url);
			    return true;
		    }
		    catch(Exception e)
		    {
			    Log.Error("[Utility.Helper.TryOpenUrl] " + e, memberName, sourceFilePath);
			    return false;
		    }
	    }

	    public static async Task WaitForFileAccess(string path, int delay)
	    {
		    while(true)
		    {
			    try
			    {
				    using(var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				    {
					    if(stream.Name != null)
						    break;
				    }
			    }
			    catch
			    {
				    await Task.Delay(delay);
			    }
		    }
	    }

	    //See https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx for value conversion
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
	    public static Version GetInstalledDotNetVersion()
	    {
		    try
		    {
			    const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
			    using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
				    .OpenSubKey(subkey))
			    {
				    var value = ndpKey?.GetValue("Release");
				    if(value != null) return RegistryToNetVersion((int) value);
					throw new Exception("Error");
			    }
		    }
		    catch(Exception ex)
		    {
			    Log.Error(ex);
			    return new Version("0");
		    }
	    }

	    private static Version RegistryToNetVersion(int registry)
	    {
		    var returnver = new Version("0");
		    switch (registry)
		    {
			    case 378389:
				    returnver = Version.Parse("4.5");
				    break;
			    case 378675:
			    case 378758:
				    returnver = Version.Parse("4.5.1");
				    break;
			    case 379893:
				    returnver = Version.Parse("4.5.1");
				    break;
			    case 393295:
			    case 393297:
				    returnver = Version.Parse("4.6");
				    break;
			    case 394254:
			    case 394271:
				    returnver = Version.Parse("4.6.1");
				    break;
			    case 394802:
			    case 394806:
				    returnver = Version.Parse("4.6.2");
				    break;
			    case 460798:
				    returnver = Version.Parse("4.7");
				    break;
		    }
		    return returnver;
	    } 

	    public static string GetWindowsVersion()
	    {
		    try
		    {
			    var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
			    return reg == null ? "Unknown" : $"{reg.GetValue("ProductName")} {reg.GetValue("CurrentBuild")}";
		    }
		    catch(Exception ex)
		    {
			    Log.Error(ex);
			    return "Unknown";
		    }
	    }

	    public static int GetWindowsBuild()
	    {
			try
			{
				var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				return Int32.Parse(reg.GetValue("CurrentBuild") as string);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return 0;
			}
		}

	    public static bool IsValidUrl(string url)
	    {
		    Uri result;
		    return Uri.TryCreate(url, UriKind.Absolute, out result)
		           && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
	    }

	    public static IEnumerable<FileInfo> GetFileInfos(string path, bool subDir)
	    {
		    var dirInfo = new DirectoryInfo(path);
		    foreach(var fileInfo in dirInfo.GetFiles())
			    yield return fileInfo;
		    if(!subDir)
			    yield break;
		    foreach(var dir in dirInfo.GetDirectories())
		    foreach(var fileInfo in dir.GetFiles())
			    yield return fileInfo;
	    }
	}
}
