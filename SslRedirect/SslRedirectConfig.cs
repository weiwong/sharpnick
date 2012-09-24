using System.Configuration;

namespace SharpNick.SslRedirect
{
	/// <summary>
	/// Configures an SSL module.
	/// </summary>
	/// <remarks>
	/// Matches performed are not case sensitive.
	/// 
	/// All match expressions are performed against a path
	/// relative to the application. For example, if the application root is http://localhost/App/ and
	/// the request is http://localhost/App/Login.aspx?param=value, the match will be validated
	/// against /Login.aspx.
	/// 
	/// A / is automatically inserted for non-Regex matches. A / is required for Regex matches.
	/// 
	/// When not specified, the inferred type is Secure.
	/// </remarks>
	/// <example>
	/// The following example ensures the following:
	/// 
	/// <list type="disc">
	///	<item>Rules only apply to requests from remote hosts.</item>
	///	<item>Files ending with .axd will served under whatever protocol they are requested in.</item>
	///	<item>All requests to Admin/Info subfolder not served under SSL.</item>
	///	<item>All requests to Admin subfolder uses SSL.</item>
	///	<item>Login.aspx is always served using SSL.</item>
	///	<item>All other requests will not be served under SSL.</item>
	/// </list>
	/// 
	/// <code>
	/// &lt;configSections&gt;
	///	&lt;section name="ssl" type="SharpNick.SslRedirect.SslRedirectConfig, SharpNick"/&gt;
	/// &lt;/configSections&gt;
	/// &lt;ssl mode="RemoteOnly" &gt;
	///	&lt;rules&gt;
	///		&lt;add match=".+\.axd" type="Ignore" isRegex="true"/&gt;
	///		&lt;add match="/Admin/Info/?.*" type="Unsecure" isRegex="true"/&gt;
	///		&lt;add match="/Admin/?.*" type="Secure" isRegex="true"/&gt;
	///		&lt;add match="Login.aspx" /&gt;
	///	&lt;/rules&gt;
	/// &lt;/ssl&gt;
	/// </code>
	/// </example>
	/// <seealso cref="SslRedirectModule"/>
	public class SslRedirectConfig : ConfigurationElement
	{
		/// <summary>
		/// Gets the list of rules for this configuration.
		/// </summary>
		[ConfigurationProperty("rules")]
		public SslRedirectRuleConfigCollection Rules
		{
			get { return (SslRedirectRuleConfigCollection)this["rules"]; }
		}
		/// <summary>
		/// Gets the mode which this SSL module should be run on.
		/// </summary>
		[ConfigurationProperty("mode", DefaultValue=SslRedirectMode.On)]
		public SslRedirectMode Mode
		{
			get { return (SslRedirectMode)this["mode"]; }
		}
		/// <summary>
		/// Gets the trace mode which this SSL module.
		/// </summary>
		[ConfigurationProperty("trace", DefaultValue = false)]
		public bool Trace
		{
			get { return (bool)this["trace"]; }
		}
	}
}
