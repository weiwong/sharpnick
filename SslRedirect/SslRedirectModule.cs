using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;

namespace SharpNick.SslRedirect
{
	/// <summary>
	/// Automatically manages redirection of requests from SSL to non-SSL protocols, and
	/// vice versa.
	/// </summary>
	/// <remarks>
	/// For information on how to configure this module, see <see cref="SharpNick.SslRedirect.SslRedirectConfig"/>.
	/// This module has not been tested with cross-page postback scenarios and may
	/// cease to redirect properly.
	/// 
	/// This module is written with help from this article: http://www.codeproject.com/aspnet/WebPageSecurity_v2.asp.
	/// Ideas from the article employed includes:
	/// 
	/// <list>
	///	<item>Killing the security warning dialogs by doing pseudo-redirect, instead of
	///		straight 302 responses.</item>
	///	<item>The idea of evaluating security context and then redirecting requests
	///		based on the evaluation results.</item>
	///	<item>Having 3 security types: Secure, Unsecure and Ignore.</item>
	/// </list>
	/// 
	/// Some differences include:
	/// 
	/// <list type="disc">
	///	<item>Using Regex and straight string comparisons instead of exact matches using
	///		file and directory paths.</item>
	///	<item>Separation of configuration specific classes and rule classes to reduce fragility
	///		and improve performance.</item>
	///	<item>Provide more options to ignore certain requests.</item>
	///	<item>Removed reliance on HttpContext.Current for thread-safety reasons.</item>
	///	<item>Removed a lot of features that are not required.</item>
	/// </list>
	/// 
	/// This module is otherwise completely written from scratch.
	/// </remarks>
	public class SslRedirectModule : IHttpModule
	{
		/// <summary>
		/// List of rules to abide to.
		/// </summary>
		/// <remarks>This variable is not initialized if mode is Off.</remarks>
		private SslRedirectRuleCollection _Rules;
		/// <summary>
		/// Mode this module is being set to.
		/// </summary>
		private SslRedirectMode _Mode;
		/// <summary>
		/// Flag to determine if the tracing is enabled.
		/// </summary>
		private bool _Trace;

		/// <summary>
		/// Protocol prefix when using SSL.
		/// </summary>
		private const string SecureProtocolPrefix = "https://";
		/// <summary>
		/// Protocol prefix when not using SSL.
		/// </summary>
		private const string UnsecureProtocolPrefix = "http://";

		/// <summary>
		/// Initializes this module.
		/// </summary>
		/// <param name="context"></param>
		public void Init(HttpApplication context)
		{
			var config = SharpNickConfiguration.GetConfig();

			if (config == null || config.SslRedirectConfig == null)
			{
				throw new ConfigurationErrorsException("SslRedirectModule was not able to find configuration settings.");
			}

			_Mode = config.SslRedirectConfig.Mode;

			if (config.SslRedirectConfig.Mode != SslRedirectMode.Off)
			{
				// subscribe to beginrequest event, grab rules from configuration
				context.BeginRequest += new EventHandler(Application_BeginRequest);
				_Rules = new SslRedirectRuleCollection(config.SslRedirectConfig.Rules);
			}

			_Trace = config.SslRedirectConfig.Trace;
		}
		/// <summary>
		/// Disposes this object.
		/// </summary>
		public void Dispose()
		{
			// do nothing
		}
		/// <summary>
		/// Handles request events.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Application_BeginRequest(object sender, EventArgs e)
		{
			HttpContext context = ((HttpApplication)sender).Context;
			HttpRequest request = context.Request;

			// if mode is remote only and the request is local, ignore processing. otherwise
			// determine if request should be intercepted with redirection calls.
			if (_Mode == SslRedirectMode.RemoteOnly && request.IsLocal)
			{
				// do nothing
				Trace("Do nothing because mode is remote only or request is local");
			}
			else
			{
				HttpResponse response = context.Response;
				SecurityType type = EvaluateSecurityType(request);
				Trace("SecurityType: " + type);

				// after determining what security context the page should be served in, ensure
				// that they are served in the correct context
				if (type == SecurityType.Secure)
				{
					// ensure secure request. if not using HTTPS, redirec to one
					if (!request.IsSecureConnection)
					{
						response.StatusCode = 301;
						response.RedirectLocation = NormalizeUrl(request, true);
						response.End();
					}
				}
				else if (type == SecurityType.Unsecure)
				{
					// ensure unsecure request. the code below is used to ensure that users do not get
					// SSL redirection warnings
					if (request.IsSecureConnection)
					{
						response.Clear();
						string normalizedUrl = NormalizeUrl(request, false);

						// write redirection header
						response.AppendHeader("Refresh", "0;URL=" + normalizedUrl);

						// write javascript backup
						response.Write("<html><head><script language=\"javascript\"><!-- \n window.location=\"");
						response.Write(SanitizeForJavascript(normalizedUrl));
						response.Write("\"\n --></script></head><!--Version 1.1.0.3--></html>");

						// no further processing required
						response.End();
					}
				}
				// do nothing for ignore
			}
		}
		/// <summary>
		/// Sanitize a string to prevent cross-site scripting.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private string SanitizeForJavascript(string url)
		{
			return url.Replace("<", "%3c").Replace(">", "%3e").Replace("\"", "%22");
		}
		/// <summary>
		/// Makes sure that a path is either secure or unsecure.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="isSecure">Value that determines if the resulting URL must be using SSL or not.</param>
		/// <returns></returns>
		private static string NormalizeUrl(HttpRequest request, bool isSecure)
		{
			string result = isSecure ? "https://" : "http://";
			result += request.Url.Authority;

			// remove default.aspx
			result += Regex.Replace(request.RawUrl, @"/default\.aspx(\?.*)?$", "/$1", RegexOptions.IgnoreCase);

			// remove trailing question mark
			if (result[result.Length - 1] == '?') result = result.Remove(result.Length - 1);

			return result;
		}
		/// <summary>
		/// Determines what security type this request must be enforced to.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private SecurityType EvaluateSecurityType(HttpRequest request)
		{
			// get the relative path by removing application path from path
			string path = request.Path;
			int applicationPathLength = request.ApplicationPath.Length;
			if (applicationPathLength > 1) path = path.Remove(0, applicationPathLength);

			// go through each rule to determine if any rule applies to this path.
			// once matched, return the security context of the rule
			foreach (SslRedirectRule rule in _Rules)
			{
				if (rule.IsMatch(path)) return rule.SecurityType;
			}

			// default to unsecure
			return SecurityType.Unsecure;
		}
		private void Trace(string message)
		{
			if (_Trace) Logging.Trace(message, "SslRedirectModule");
		}
	}
}
