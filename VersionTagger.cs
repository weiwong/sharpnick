using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace SharpNick
{
	/// <summary>
	/// Goes through an HTTP response stream and renames JS and CSS files to a specified
	/// version number in order to force browsers to refresh cached versions of the files.
	/// </summary>
	/// <remarks>
	/// How to Use:
	/// 
	/// Wire up in Global.asax:
	/// void Application_BeginRequest(object sender, EventArgs e)
	/// {
	///     if (Request.Url.LocalPath.EndsWith(".aspx")) Response.Filter = new VersionTagger(Response);
	///     else VersionTagger.Rewrit(Context);
	/// }
	/// </remarks>
	public class VersionTagger : Stream
	{
		private Stream _ResponseStream;
		private Encoding _Encoding;
		/// <summary>
		/// Required to override Stream but not used.
		/// </summary>
		private long _Position;
		/// <summary>
		/// Mapping between JS and CSS file URLs and their URLs with the version tag.
		/// </summary>
		private static Dictionary<string, string> _Versions;
		/// <summary>
		/// Multithreading lock to ensure that configuration is not loaded more than once.
		/// </summary>
		private static object _VersionsLock = new object();
		/// <summary>
		/// Mapping from version tagged file URLs to the non tagged versions.
		/// </summary>
		private static Dictionary<string, string> _Urls;
		/// <summary>
		/// List of file watchers.
		/// </summary>
		private static List<FileSystemWatcher> _Watchers;

		/// <summary>
		/// Creates an instance of the response filter.
		/// </summary>
		/// <param name="response"></param>
		public VersionTagger(HttpResponse response)
		{
			EnsureReady();
			_Encoding = response.Output.Encoding;
			_ResponseStream = response.Filter;
		}
		/// <summary>
		/// Ensures that this class is ready for use.
		/// </summary>
		private static void EnsureReady()
		{
			if (_Versions != null) return;
			lock (_VersionsLock)
			{
				if (_Versions != null) return;
				if (string.IsNullOrEmpty(HostingEnvironment.ApplicationPhysicalPath)) return;

				var config = SharpNickConfiguration.GetConfig();

				if (config != null && config.VersionTaggerConfig != null)
				{
					_Versions = new Dictionary<string, string>();
					_Urls = new Dictionary<string, string>();
					_Watchers = new List<FileSystemWatcher>();

					foreach (VersionTaggerEntry entry in config.VersionTaggerConfig.Entries)
					{
						/// get the version of the file
						var version = GetVersion(entry);
						Logging.Trace("Version is " + version, "VersionTagger");

						/// if the version is valid for this file
						if (!string.IsNullOrEmpty(version))
						{
							/// get the mapping for the before and after conversion, and vice
							/// versa, in two different hash tables
							var extension = entry.Url.Substring(entry.Url.LastIndexOf('.') + 1);
							var replacement = string.Format("{0}{1}.{2}",
								entry.Url.Remove(entry.Url.Length - extension.Length), version, extension);

							_Versions[entry.Url] = replacement;
							_Urls[replacement] = entry.Url;

							/// monitor the file for changes
							if (string.IsNullOrEmpty(entry.Version)) WatchFile(entry.Url);
						}
					}

					/// clean up this class when the application shuts down
					AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
				}
				else
				{
					throw new ConfigurationErrorsException("Configuration for VersionTagger cannot be found.");
				}
			}
		}
		/// <summary>
		/// Cleans up this class when the application shuts down.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
			try
			{
				foreach (var watcher in _Watchers)
				{
					watcher.Dispose();
				}
			}
			catch { }
		}
		/// <summary>
		/// Watches a file and invalidates the cache if any of them changed.
		/// </summary>
		/// <param name="url"></param>
		private static void WatchFile(string url)
		{
			var filePath = HostingEnvironment.MapPath(url);
			var file = new FileInfo(filePath);

			var result = new FileSystemWatcher(file.DirectoryName);
			result.Filter = file.Name;
			result.NotifyFilter = NotifyFilters.LastWrite;
			result.Changed += new FileSystemEventHandler(File_Changed);
			
			result.EnableRaisingEvents = true;
			_Watchers.Add(result);
		}
		/// <summary>
		/// Handle the event when any files have changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void File_Changed(object sender, FileSystemEventArgs e)
		{
			lock (_VersionsLock) _Versions = null;
		}
		/// <summary>
		/// Gets the version for the file in this entry.
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		private static string GetVersion(VersionTaggerEntry entry)
		{
			/// try to get the version number from the configuration, if available
			var filePath = HostingEnvironment.MapPath(entry.Url);
			if (!File.Exists(filePath)) throw new ConfigurationErrorsException(filePath + " cannot be found");
			if (!string.IsNullOrEmpty(entry.Version)) return entry.Version;

			/// otherwise, get the hash code for the file
			byte[] fileContents;
			var fileLength = (int)(new FileInfo(filePath).Length);

			using (var stream = new FileStream(filePath, FileMode.Open))
			using (var reader = new BinaryReader(stream))
			{
				fileContents = reader.ReadBytes(fileLength);
			}

			return MurmurHash2.Hash(fileContents).ToString("x");
		}
		/// <summary>
		/// Rewrites the request's path to the underlying JS or CSS file.
		/// </summary>
		/// <param name="context"></param>
		public static void Rewrite(HttpContext context)
		{
			EnsureReady();
			var url = context.Request.RawUrl;
			string actualUrl;
			if (_Urls.TryGetValue(url, out actualUrl)) context.RewritePath(actualUrl);
		}

		#region Filter overrides
		public override bool CanRead
		{
			get { return false; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override void Close()
		{
			_ResponseStream.Close();
		}

		public override long Position
		{
			get { return _Position; }
			set { _Position = value; }
		}

		public override void Flush()
		{
			//this.FlushPendingBuffer();
			_ResponseStream.Flush();
		}

		public override long Length
		{
			get { return 0; }
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _ResponseStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_ResponseStream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _ResponseStream.Read(buffer, offset, count);
		}
		#endregion

		/// <summary>
		/// Tags any JS or CSS files the VersionTagger handles with a version number.
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			/// if there is nothing to check, just redirect the stream
			if (_Versions.Count == 0)
			{
				_ResponseStream.Write(buffer, offset, count);
				return;
			}

			string html = new string(_Encoding.GetChars(buffer, offset, count));

			foreach (string url in _Versions.Keys)
			{
				string extension = url.Substring(url.LastIndexOf('.') + 1);

				/// use different replacement methods for different files
				/// as their inclusion methods are different
				if (string.Compare(extension, "css", true) == 0) html = html.Replace("href=\"" + url + "\"", "href=\"" + _Versions[url] + "\"");
				else if (string.Compare(extension, "js", true) == 0) html = html.Replace("src=\"" + url + "\"", "src=\"" + _Versions[url] + "\"");
			}

			byte[] output = _Encoding.GetBytes(html);

			_ResponseStream.Write(output, offset, output.Length);
		}
		/// <summary>
		/// Hash algorithm using MurmurHash. Credit to Davy Landman.
		/// http://landman-code.blogspot.com/2009/02/c-superfasthash-and-murmurhash2.html
		/// </summary>
		private class MurmurHash2
		{
			private const UInt32 m = 0x5bd1e995;
			private const Int32 r = 24;

			public static UInt32 Hash(byte[] data)
			{
				return Hash(data, 0xc58f1a7b);
			}

			public static UInt32 Hash(byte[] data, UInt32 seed)
			{
				var length = data.Length;
				if (length == 0) return 0;

				var h = seed ^ (UInt32)length;
				var currentIndex = 0;

				while (length >= 4)
				{
					var k = (UInt32)(data[currentIndex++] | data[currentIndex++] << 8 | data[currentIndex++] << 16 | data[currentIndex++] << 24);
					k *= m;
					k ^= k >> r;
					k *= m;

					h *= m;
					h ^= k;
					length -= 4;
				}

				switch (length)
				{
					case 3:
						h ^= (UInt16)(data[currentIndex++] | data[currentIndex++] << 8);
						h ^= (UInt32)(data[currentIndex] << 16);
						h *= m;
						break;
					case 2:
						h ^= (UInt16)(data[currentIndex++] | data[currentIndex] << 8);
						h *= m;
						break;
					case 1:
						h ^= data[currentIndex];
						h *= m;
						break;
					default:
						break;
				}

				// Do a few final mixes of the hash to ensure the last few
				// bytes are well-incorporated.

				h ^= h >> 13;
				h *= m;
				h ^= h >> 15;

				return h;
			}
		}
	}
	/// <summary>
	/// Configuration element for the VersionTagger.
	/// </summary>
	public class VersionTaggerConfig : ConfigurationElement
	{
		/// <summary>
		/// Gets the list of entries for this configuration.
		/// </summary>
		[ConfigurationProperty("entries")]
		public VersionTaggerEntries Entries
		{
			get { return (VersionTaggerEntries)this["entries"]; }
		}
	}
	/// <summary>
	/// List of URLs for VersionTagger to manage.
	/// </summary>
	public class VersionTaggerEntry : ConfigurationElement
	{
		/// <summary>
		/// Gets the URL to specify version with.
		/// </summary>
		[ConfigurationProperty("url", IsRequired = true)]
		public string Url
		{
			get { return (string)this["url"]; }
		}
		/// <summary>
		/// Gets the version for the file as represented by this url.
		/// </summary>
		[ConfigurationProperty("version")]
		public string Version
		{
			get { return (string)this["version"]; }
		}
	}
	/// <summary>
	/// Defines a collection of version tagger entries.
	/// </summary>
	public class VersionTaggerEntries : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new VersionTaggerEntry();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return element;
		}
	}
}
