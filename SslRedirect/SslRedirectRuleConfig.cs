using System.Configuration;

namespace SharpNick.SslRedirect
{
	/// <summary>
	/// Defines a rule of matching a request path to a security type.
	/// </summary>
	public class SslRedirectRuleConfig : ConfigurationElement
	{
		/// <summary>
		/// Gets the match expression.
		/// </summary>
		[ConfigurationProperty("match", IsRequired=true)]
		public string MatchExpression
		{
			get { return (string)this["match"]; }
		}
		/// <summary>
		/// Gets the value that determines if the match expression is a regular expression.
		/// </summary>
		[ConfigurationProperty("isRegex", DefaultValue=false)]
		public bool IsRegex
		{
			get { return (bool)this["isRegex"]; }
		}
		/// <summary>
		/// Gets the value that determines what security type for requests made for this matching
		/// expression is.
		/// </summary>
		[ConfigurationProperty("type", DefaultValue=SecurityType.Secure)]
		public SecurityType SecurityType
		{
			get { return (SecurityType)this["type"]; }
		}
	}
	/// <summary>
	/// Defines a collection of SSL configuration rules.
	/// </summary>
	public class SslRedirectRuleConfigCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new SslRedirectRuleConfig();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return element;
		}
	}
}
