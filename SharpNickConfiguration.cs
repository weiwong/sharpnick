using System.Configuration;
using SharpNick.PingMachine;
using SharpNick.SslRedirect;

namespace SharpNick
{
	internal class SharpNickConfiguration : ConfigurationSection
	{
		[ConfigurationProperty("countryLookup")]
		public CountryLookupConfig CountryLookupConfig
		{
			get { return this["countryLookup"] as CountryLookupConfig; }
		}
		[ConfigurationProperty("pingMachine")]
		public PingMachineConfig PingMachineConfig
		{
			get { return this["pingMachine"] as PingMachineConfig; }
		}
		[ConfigurationProperty("logging")]
		public LoggingConfig LoggingConfig
		{
			get { return this["logging"] as LoggingConfig; }
		}
		[ConfigurationProperty("versionTagger")]
		public VersionTaggerConfig VersionTaggerConfig
		{
			get { return this["versionTagger"] as VersionTaggerConfig; }
		}
		[ConfigurationProperty("sslRedirects")]
		public SslRedirectConfig SslRedirectConfig
		{
			get { return this["sslRedirects"] as SslRedirectConfig; }
		}
		public static SharpNickConfiguration GetConfig()
		{
			return ConfigurationManager.GetSection("sharpNick") as SharpNickConfiguration;
		}
	}
}
