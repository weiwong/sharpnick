using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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
	/// void Application_PreRequestHandlerExecute(object sender, EventArgs e)
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
		/// Mapping between JS and CSS file URLs and their versions.
		/// </summary>
		private static Dictionary<string, int> _Versions;
		/// <summary>
		/// Multithreading lock to ensure that configuration is not loaded more than once.
		/// </summary>
		private static object _VersionsLock = new object();
		/// <summary>
		/// Regex to determine if a URL is requesting a CSS or JS file in a format that is tagged by the VersionTagger.
		/// </summary>
		private static Regex _UrlIsCssJs = new Regex(@"^/(.+?)\.\d+\.(css|js)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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

				_Versions = new Dictionary<string, int>();
				var config = SharpNickConfiguration.GetConfig();

				if (config != null && config.VersionTaggerConfig != null)
				{
					foreach (VersionTaggerEntry entry in config.VersionTaggerConfig.Entries)
					{
						_Versions[entry.Url] = entry.Version;
					}
				}
			}
		}
		/// <summary>
		/// Rewrites the request's path to the underlying JS or CSS file.
		/// </summary>
		/// <param name="context"></param>
		public static void Rewrite(HttpContext context)
		{
			var url = context.Request.RawUrl;
			var match = _UrlIsCssJs.Match(url);
			if (match.Success) context.RewritePath(string.Format("/{0}.{1}", match.Groups[1].Value, match.Groups[2].Value));
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
				string replacement = string.Format("{0}{1}.{2}",
					url.Remove(url.Length - extension.Length), _Versions[url], extension);

				/// use different replacement methods for different files
				/// as their inclusion methods are different
				if (string.Compare(extension, "css", true) == 0) html = html.Replace("href=\"" + url + "\"", "href=\"" + replacement + "\"");
				if (string.Compare(extension, "js", true) == 0) html = html.Replace("src=\"" + url + "\"", "src=\"" + replacement + "\"");
			}

			byte[] output = _Encoding.GetBytes(html);

			_ResponseStream.Write(output, offset, output.Length);
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
		[ConfigurationProperty("version", IsRequired = true)]
		public int Version
		{
			get { return (int)this["version"]; }
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
