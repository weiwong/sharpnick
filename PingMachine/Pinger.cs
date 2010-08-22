using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace SharpNick.PingMachine
{
	/// <summary>
	/// Module that periodically loads a list pages.
	/// </summary>
	public class Pinger
	{
		#region Variables
		/// <summary>
		/// Number of seconds to wait before timing out the request.
		/// </summary>
		private static int _PingTimeOut;
		/// <summary>
		/// Number of seconds to wait before pinging again.
		/// </summary>
		private static int _PingInterval;
		/// <summary>
		/// List of URLs to load for every ping.
		/// </summary>
		private static string[] _PingUrls;
		/// <summary>
		/// Flag that determines if this machine has already started.
		/// </summary>
		private static bool _Started = false;
		/// <summary>
		/// Multithreading lock to make sure that this machine is only started once.
		/// </summary>
		private static object _StartLock = new object();
		/// <summary>
		/// Event to fire when the current AppDomain unloads.
		/// </summary>
		private static EventHandler _DomainUnloadEventHandler;
		/// <summary>
		/// List of credentials for use when accessing URLs.
		/// </summary>
		private static CredentialCache _Credentials;
		/// <summary>
		/// Timer used to ping pages.
		/// </summary>
		private static Timer _Timer;
		#endregion

		/// <summary>
		/// Initializes the module.
		/// </summary>
		/// <param name="context"></param>
		public static void Start()
		{
			if (_Started) return;
			lock (_StartLock)
			{
				if (_Started) return;

				/// get the configuration
				var config = SharpNickConfiguration.GetConfig();

				if (config == null || config.PingMachineConfig == null)
				{
					throw new ConfigurationErrorsException("Pinger was not able to find configuration settings.");
				}

				/// assign simple configuration values
				_PingTimeOut = config.PingMachineConfig.PingTimeOut;
				_PingInterval = config.PingMachineConfig.PingInterval;

				/// add credentials to the cache
				_Credentials = new CredentialCache();
				foreach (PingMachineCredential credential in config.PingMachineConfig.Credentials)
				{
					if (!Regex.IsMatch(credential.Url, @"^https?://[\w\.]+\.\w{2,4}(:\d+)?", RegexOptions.IgnoreCase))
					{
						throw new ConfigurationErrorsException(credential + " is not a valid URL",
							credential.ElementInformation.Source, credential.ElementInformation.LineNumber);
					}

					var networkCredential = new NetworkCredential(credential.Username, credential.Password);
					_Credentials.Add(new Uri(credential.Url), "Basic", networkCredential);
				}

				/// put the rules into the static variable
				_PingUrls = config.PingMachineConfig.Urls.Cast<PingMachineUrl>().Select(n => n.Url).ToArray();

				/// start the ping timer
				if (!_Started)
				{
					var interval = _PingInterval * 60 * 1000;
					var callback = new TimerCallback(Ping);
					_Timer = new Timer(callback, null, 0, interval);
				}

				/// bind unload event to shut down pinger
				if (_DomainUnloadEventHandler == null)
				{
					_DomainUnloadEventHandler = new EventHandler(CurrentDomain_DomainUnload);
					AppDomain.CurrentDomain.DomainUnload += _DomainUnloadEventHandler;
				}

				_Started = true;
			}
		}
		/// <summary>
		/// Pings the list of pages.
		/// </summary>
		private static void Ping(object state)
		{
			try
			{
				foreach (string url in _PingUrls)
				{
					var request = (HttpWebRequest)WebRequest.Create(url);
					request.Credentials = _Credentials;
					request.Timeout = _PingTimeOut * 1000;

					var response = (HttpWebResponse)request.GetResponse();
					response.Close();
				}
			}
			catch (Exception ex)
			{
				Logging.LogError("Pinger", ex);
			}
		}
		/// <summary>
		/// Clean up resources reseved by this class.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
			Logging.Trace("Closing down services", "Pinger", "Pinger");
			if (_Timer != null) _Timer.Dispose();
		}
	}
}
