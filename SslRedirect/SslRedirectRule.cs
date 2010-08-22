using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SharpNick.SslRedirect
{
	/// <summary>
	/// Defines a rule for which a SSL redirection is handled.
	/// </summary>
	public class SslRedirectRule
	{
		/// <summary>
		/// Regex object which is used to evaluate a path.
		/// </summary>
		/// <remarks>Initialized when mode is regex.</remarks>
		private Regex _RegexMatchExpression;
		/// <summary>
		/// String to determine if this rule applies to a particular path.
		/// </summary>
		/// <remarks>Initialized when mode is not regex.</remarks>
		private string _StringMatchExpression;

		#region Properties
		private SecurityType _SecurityType;
		/// <summary>
		/// Gets or sets the type of security on the requested path.
		/// </summary>
		public SecurityType SecurityType
		{
			get { return _SecurityType; }
			set { _SecurityType = value; }
		}
		#endregion

		/// <summary>
		/// Create an instance of SslRule from a configuration element.
		/// </summary>
		/// <param name="config"></param>
		public SslRedirectRule(SslRedirectRuleConfig config)
		{
			SecurityType = config.SecurityType;

			/// if expression is regular expression, save the matching as a regex object and compile it
			/// otherwise simply store the string
			if (config.IsRegex)
			{
				_RegexMatchExpression = new Regex(config.MatchExpression,
					RegexOptions.Compiled | RegexOptions.IgnoreCase);
			}
			else
			{
				/// make sure that the match expressions starts with /
				string matchExpression = config.MatchExpression;
				if (!matchExpression.StartsWith("/")) matchExpression = "/" + matchExpression;

				_StringMatchExpression = matchExpression;
			}
		}
		/// <summary>
		/// Determines if this rule applies to the path specified.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public bool IsMatch(string path)
		{
			/// if mode is regex, _RegexMatchExpression is not null and will be used for evaluating
			/// the path. otherwise a simple string comparison is performed.
			if (_RegexMatchExpression == null) return string.Compare(path, _StringMatchExpression, true) == 0;
			return _RegexMatchExpression.IsMatch(path);
		}
	}
	/// <summary>
	/// A collection of SSL rules.
	/// </summary>
	public class SslRedirectRuleCollection : List<SslRedirectRule>
	{
		/// <summary>
		/// Creates an instance of SslRuleCollection by reading from the configuration counterpart
		/// (SslRuleConfigCollection).
		/// </summary>
		/// <param name="config"></param>
		public SslRedirectRuleCollection(SslRedirectRuleConfigCollection config)
		{
			foreach (SslRedirectRuleConfig rule in config)
			{
				this.Add(new SslRedirectRule(rule));
			}
		}
	}
}
