using System.Configuration;

namespace SharpNick.PingMachine
{
	/*
		Example configuration:
		
		<pingMachine interval="4" timeOut="15">
			<credentials>
				<add url="http://www.sharpnick.com" username="username" password="password" />
			</credentials>
			<urls>
				<add url="http://www.sharpnick.com/cart.aspx"/>
			</urls>
		</pingMachine>
	*/
	/// <summary>
	/// Represents the configuration for PingMachine module.
	/// </summary>
	public class PingMachineConfig : ConfigurationElement
	{
		/// <summary>
		/// Gets the list of URLs to ping.
		/// </summary>
		[ConfigurationProperty("urls")]
		public PingMachineUrlCollection Urls
		{
			get { return (PingMachineUrlCollection)this["urls"]; }
		}
		/// <summary>
		/// Gets a list of usernames and passwords for use with certain URLs.
		/// </summary>
		[ConfigurationProperty("credentials", IsRequired=false)]
		public PingMachineCredentialCollection Credentials
		{
			get { return (PingMachineCredentialCollection)this["credentials"]; }
		}
		/// <summary>
		/// Gets the number of minutes to wait before pinging again.
		/// </summary>
		[ConfigurationProperty("interval", IsRequired=false, DefaultValue=4)]
		public int PingInterval
		{
			get { return (int)this["interval"]; }
		}
		/// <summary>
		/// Gets the number of seconds to give up connecting to a URL.
		/// </summary>
		[ConfigurationProperty("timeOut", IsRequired=false, DefaultValue=15)]
		public int PingTimeOut
		{
			get { return (int)this["timeOut"]; }
		}
	}
	/// <summary>
	/// Defines a URL to ping.
	/// </summary>
	public class PingMachineUrl : ConfigurationElement
	{
		/// <summary>
		/// Gets the URL to ping.
		/// </summary>
		[ConfigurationProperty("url", IsRequired = true)]
		public string Url
		{
			get { return (string)this["url"]; }
		}
	}
	/// <summary>
	/// Defines a collection of ping URLs.
	/// </summary>
	public class PingMachineUrlCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Creates a new instance of PingMachineUrlCollection.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new PingMachineUrl();
		}
		/// <summary>
		/// Gets the key of this element.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return element;
		}
	}
	/// <summary>
	/// Represents a username and password pair used when pinging the URLs.
	/// </summary>
	public class PingMachineCredential : ConfigurationElement
	{
		/// <summary>
		/// Gets the URL prefix to use this username and password.
		/// </summary>
		[ConfigurationProperty("url", IsKey=true)]
		public string Url
		{
			get { return (string)this["url"]; }
		}
		/// <summary>
		/// Gets the usenrame.
		/// </summary>
		[ConfigurationProperty("username")]
		public string Username
		{
			get { return (string)this["username"]; }
		}
		/// <summary>
		/// Gets the password.
		/// </summary>
		[ConfigurationProperty("password")]
		public string Password
		{
			get { return (string)this["password"]; }
		}
	}
	/// <summary>
	/// Represents a list of usernames and passwords used when pigning the URLs.
	/// </summary>
	public class PingMachineCredentialCollection : ConfigurationElementCollection
	{
		/// <summary>
		/// Crates a new instance of PingMachineCredentialCollection.
		/// </summary>
		/// <returns></returns>
		protected override ConfigurationElement CreateNewElement()
		{
			return new PingMachineCredential();
		}
		/// <summary>
		/// Gets the key that represents this element.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		protected override object GetElementKey(ConfigurationElement element)
		{
			return element;
		}
	}
}
