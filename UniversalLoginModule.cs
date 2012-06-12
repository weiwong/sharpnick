using System;
using System.Configuration;
using System.Web;

namespace SharpNick
{
	/// <summary>
	/// Forces an entire site to be authenticated in a different level of security.
	/// </summary>
	public class UniversalLoginModule : IHttpModule
	{
		private static UniversalLoginConfig Config;

		void IHttpModule.Init(HttpApplication context)
		{
			Config = SharpNickConfiguration.GetConfig().UniversalLoginConfig;
			if (Config != null && Config.Enable) context.BeginRequest += new EventHandler(BeginRequest);
		}
		/// <summary>
		/// Intercepts a request and determines if the user is authenticated.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void BeginRequest(object sender, EventArgs e)
		{
			var context = ((HttpApplication)sender).Context;
			var request = context.Request;

			if (request.Url.AbsolutePath == "/universal-logout")
			{
				/// request logout
				var cookie = request.Cookies[Config.CookieName];
				if (cookie != null)
				{
					cookie.Expires = DateTime.Now.AddMonths(-1);
					context.Response.Cookies.Add(cookie);
				}

				context.Response.Redirect("/", true);
			}
			if (request.HttpMethod == "POST" && request.Form["action"] == "universallogin")
			{
				/// request login
				if (request.Form["password"] == Config.Password)
				{
					var cookie = new HttpCookie(Config.CookieName, Config.CookieAuthValue);
					cookie.Expires = DateTime.Now.AddDays(Config.ValidDays);
					context.Response.Cookies.Add(cookie);
				}
				else ServeLoginPage(context, "Invalid password");
			}
			else if (!Authenticate(context.Request)) ServeLoginPage(context, "");
		}
		/// <summary>
		/// Serves a login page.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="message"></param>
		private static void ServeLoginPage(HttpContext context, string message)
		{
			var response = context.Response;
			response.Expires = -1;
			response.Cache.SetCacheability(HttpCacheability.NoCache);
			response.Cache.SetNoStore();

			message = string.IsNullOrEmpty(message) ? string.Empty : string.Format("<p class=\"message\">{0}</p>", message);
			response.Write(Resources.UniversalLoginModuleLoginPage.Replace("{Message}", message));
			response.End();
		}
		/// <summary>
		/// 
		/// Authenticates a request.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		private static bool Authenticate(HttpRequest request)
		{
			var cookie = request.Cookies[Config.CookieName];
			return cookie != null && cookie.Value == Config.CookieAuthValue;
		}
		void IHttpModule.Dispose()
		{
		}
	}
	/// <summary>
	/// Configuration element for the VersionTagger.
	/// </summary>
	public class UniversalLoginConfig : ConfigurationElement
	{
		[ConfigurationProperty("cookieName", DefaultValue = "UnLogAuth")]
		public string CookieName
		{
			get { return (string)this["cookieName"]; }
		}
		[ConfigurationProperty("cookieAuthValue", IsRequired = true)]
		public string CookieAuthValue
		{
			get { return (string)this["cookieAuthValue"]; }
		}
		[ConfigurationProperty("password", IsRequired = true)]
		public string Password
		{
			get { return (string)this["password"]; }
		}
		[ConfigurationProperty("validDays", DefaultValue = 365)]
		public int ValidDays
		{
			get { return (int)this["validDays"]; }
		}
		[ConfigurationProperty("enable", DefaultValue = true)]
		public bool Enable
		{
			get { return (bool)this["enable"]; }
		}
	}
}