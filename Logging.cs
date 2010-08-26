using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Web;

namespace SharpNick
{
	/// <summary>
	/// Methods to log information.
	/// </summary>
	public sealed class Logging
	{
		/// <summary>
		/// Represents a step taken by a user.
		/// </summary>
		private class UserStep
		{
			private string _SessionID;
			private string _StepName;
			private DateTime _Date;

			public string SessionID
			{
				get { return _SessionID; }
				set { _SessionID = value; }
			}
			public string StepName
			{
				get { return _StepName; }
				set { _StepName = value; }
			}
			public DateTime Date
			{
				get { return _Date; }
				set { _Date = value; }
			}
		}

		#region Static variables
		/// <summary>
		/// Connection string to the log database.
		/// </summary>
		private static readonly string LogConnectionString = GetLogConnectionString();
		/// <summary>
		/// List of user steps cached for recording into the stats server.
		/// </summary>
		private static List<UserStep> _UserStepCache = new List<UserStep>();
		/// <summary>
		/// Multithreading lock for switching user steps cache.
		/// </summary>
		private static object _UserStepLock = new object();
		/// <summary>
		/// Timer to periodically user orders into the database.
		/// </summary>
		private static Timer _UserStepsTimer;
		private static DbProviderFactory ConnectionFactory;
		private static object ConnectionFactoryInstatiationLock = new object();
		private static bool _ThrowException;
		private static string _ExcludeCookieName;
		private static string _ExcludeCookieValue;
		#endregion

		private const string ConnectionStringName = "SharpNick.LogConnection";

		/// <summary>
		/// Initializes this class by tying the DomainUload event to the flush function.
		/// </summary>
		static Logging()
		{
			var config = SharpNickConfiguration.GetConfig();

			if (config != null)
			{
				_ThrowException = config.LoggingConfig.ThrowException;
				_ExcludeCookieName = config.LoggingConfig.ExcludeCookie.Name;
				_ExcludeCookieValue = config.LoggingConfig.ExcludeCookie.Value;
			}

			if (LogConnectionString != null)
			{
				AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
			}
		}
		private Logging() { }
		/// <summary>
		/// Executes when the current AppDomain unloads.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static void CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
			/// flush out any remaining caches
			LogUserSteps();
		}
		/// <summary>
		/// Method to handle user step timer firing event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void UserStepsTimer_Elapsed(object state)
		{
			LogUserSteps();
		}
		/// <summary>
		/// Gets a connection string to the log database.
		/// </summary>
		/// <returns></returns>
		private static string GetLogConnectionString()
		{
			if (ConfigurationManager.ConnectionStrings[ConnectionStringName] != null)
				return ConfigurationManager.ConnectionStrings[ConnectionStringName].ConnectionString;

			return null;
		}

		#region LogErrors
		public static void LogError(HttpContext context, string message)
		{
			LogError(context, new Exception(message));
		}
		/// <summary>
		/// Logs a web exception.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="error"></param>
		public static void LogError(HttpContext context, Exception error)
		{
			try
			{
				if (context != null && error != null)
				{
					int code = error is HttpException ? ((HttpException)error).GetHttpCode() : 500;
					HttpRequest request = context.Request;
					string sessionID = context.Session == null ? null : context.Session.SessionID;
					string referrer = request.UrlReferrer == null ? null : request.UrlReferrer.ToString();

					LogError(code, request.Url.ToString(), error, context.Session.SessionID,
						request.UserHostAddress, referrer);
				}
			}
			catch (Exception ex)
			{
				LogLastDitchError(ex);
			}
		}
		/// <summary>
		/// Logs an error.
		/// </summary>
		/// <param name="location"></param>
		/// <param name="error"></param>
		public static void LogError(string location, Exception error)
		{
			try
			{
				LogError(500, location, error, null, null, null);
			}
			catch (Exception ex)
			{
				LogLastDitchError(ex);
			}
		}
		public static void LogError(string location, string error)
		{
			try
			{
				LogError(500, location, new Exception(error), null, null, null);
			}
			catch (Exception ex)
			{
				LogLastDitchError(ex);
			}
		}

		public static void LogError(Exception ex)
		{
			try
			{
				LogError(500, ex.Source, ex, null, null, null);
			}
			catch (Exception e)
			{
				LogLastDitchError(e);
			}
		}
		private static void LogError(int code, string location, Exception error, string sessionID,
			string ip, string referrer)
		{
			var conn = GetLogConnection();
			if (conn == null)
			{
				if (_ThrowException) ThrowNoLogConnectionException();
				return;
			} 
			
			var cmd = conn.CreateCommand();
			cmd.CommandText =
				@"INSERT INTO Errors SET timestamp=?timestamp, code=?code, ip=?ip, location=?location,
				message=?message, stack=?stack, referrer=?referrer, sessionId=?sessionId";

			Exception exception = error.InnerException ?? error;
			string message = exception.Message;
			string stack = exception.StackTrace;

			cmd.Parameters.Add(cmd.CreateParameter("?timestamp", DateTime.Now));
			cmd.Parameters.Add(cmd.CreateParameter("?ip", ip));
			cmd.Parameters.Add(cmd.CreateParameter("?code", code));
			cmd.Parameters.Add(cmd.CreateParameter("?location", StringFormat.EnsureLength(location, 256)));
			cmd.Parameters.Add(cmd.CreateParameter("?message", StringFormat.EnsureLength(message, 256)));
			cmd.Parameters.Add(cmd.CreateParameter("?stack", StringFormat.EnsureLength(stack, 8000)));
			cmd.Parameters.Add(cmd.CreateParameter("?referrer", StringFormat.EnsureLength(referrer, 256)));
			cmd.Parameters.Add(cmd.CreateParameter("?sessionId", StringFormat.EnsureLength(sessionID, 256)));

			try
			{
				conn.Open();
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				LogLastDitchError(ex);
			}
			finally
			{
				conn.Dispose();
			}
		}
		#endregion

		/// <summary>
		/// Logs a trace message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="category"></param>
		public static void Trace(string message, string category)
		{
			HttpContext context = null;

            try
            {
                context = HttpContext.Current;
            }
            catch { }

			try
			{
				if (context == null)
				{
					Trace(message, category, null);
				}
				else
				{
					Trace(message, category, context.Request.RawUrl);
				}
			}
			catch (Exception ex)
			{
				LogLastDitchError(ex);
			}
		}
		/// <summary>
		/// Logs a trace message with location specified.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="category"></param>
		/// <param name="location"></param>
		public static void Trace(string message, string category, string location)
		{
			var conn = GetLogConnection();
			if (conn == null)
			{
				if (_ThrowException) ThrowNoLogConnectionException();
				return;
			} 
			
			var cmd = conn.CreateCommand();
			cmd.CommandText = @"INSERT INTO Traces (timestamp, location, category, message)
				SELECT ?timestamp, ?location, ?category, ?message";

			cmd.Parameters.Add(cmd.CreateParameter("?timestamp", DateTime.Now));
			cmd.Parameters.Add(cmd.CreateParameter("?location", StringFormat.EnsureLength(location, 500)));
			cmd.Parameters.Add(cmd.CreateParameter("?category", StringFormat.EnsureLength(category, 100)));
			cmd.Parameters.Add(cmd.CreateParameter("?message", StringFormat.EnsureLength(message, 1000)));

			try
			{
				conn.Open();
				cmd.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				LogLastDitchError(ex);
			}
			finally
			{
				conn.Dispose();
			}
		}
		/// <summary>
		/// Logs errors committed by users for internal reviewing.
		/// </summary>
		/// <param name="errorMessages"></param>
		public static void LogUserErrors(string location, Dictionary<string,string> errorsAndValues)
		{
			var conn = GetLogConnection();
			if (conn == null)
			{
				if (_ThrowException) ThrowNoLogConnectionException();
				return;
			}

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"INSERT INTO UserErrors (sessionID, location, timestamp, error, valueEntered)
				VALUES (?sessionID, ?location, ?timestamp, ?error, ?valueEntered);";
			DateTime nowTime = DateTime.Now;
			string sessionID = null;

			HttpContext context = HttpContext.Current;
			if (context != null) sessionID = context.Session.SessionID;

			cmd.Parameters.Add(cmd.CreateParameter("?sessionID", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("?location", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("?timestamp", DbType.DateTime));
			cmd.Parameters.Add(cmd.CreateParameter("?error", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("?valueEntered", DbType.String));

			try
			{
				conn.Open();

				foreach (string error in errorsAndValues.Keys)
				{
					cmd.Parameters["?sessionID"].Value = sessionID;
					cmd.Parameters["?location"].Value = location;
					cmd.Parameters["?timestamp"].Value = nowTime;
					cmd.Parameters["?error"].Value = error;
					cmd.Parameters["?valueEntered"].Value = StringFormat.EnsureLength(errorsAndValues[error], 100);

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				// log errors in case there were problems
				Logging.LogError(context, ex);
			}
			finally
			{
				if (conn.State != ConnectionState.Closed) conn.Close();
				conn.Dispose();
			}
		}
		/// <summary>
		/// Gets a log connection.
		/// </summary>
		/// <returns></returns>
		private static DbConnection GetLogConnection()
		{
			/// verify existence of connection string
			if (string.IsNullOrEmpty(LogConnectionString))
			{
				var message = string.Format("Cannot log because of missing connection string '{0}' in the configuration file.", ConnectionStringName);
				if (_ThrowException) throw new ConfigurationErrorsException(message);
				else return null;
			}

			/// create connection factory if needed
			if (ConnectionFactory == null)
			lock (ConnectionFactoryInstatiationLock)
			if (ConnectionFactory == null)
			{
				var providerName = ConfigurationManager.ConnectionStrings[ConnectionStringName].ProviderName;
				ConnectionFactory = DbProviderFactories.GetFactory(providerName);
			}

			/// return the connection created using the connection factory
			var result = ConnectionFactory.CreateConnection();
			result.ConnectionString = LogConnectionString;
			return result;
		}
		/// <summary>
		/// Logs a search into the stats database.
		/// </summary>
		/// <param name="term"></param>
		/// <param name="numResults"></param>
		public static void LogSearch(string term, int numResults, HttpContext context)
		{
			var conn = GetLogConnection();
			if (conn == null)
			{
				if (_ThrowException) ThrowNoLogConnectionException();
				return;
			}

			var cmd = conn.CreateCommand();
			cmd.CommandText =
				@"INSERT INTO Searches
				SET created=?created, term=?term, sessionId=?sessionId, referer=?referer, numResults=?numResults";

			var param = cmd.CreateParameter();
			param.DbType = DbType.DateTime;
			param.ParameterName = "?created";

			cmd.Parameters.Add(cmd.CreateParameter("?created", DateTime.Now));
			cmd.Parameters.Add(cmd.CreateParameter("?term", term));
			cmd.Parameters.Add(cmd.CreateParameter("?sessionId", context.Session.SessionID));
			cmd.Parameters.Add(cmd.CreateParameter("?referer", context.Request.UrlReferrer == null ?
				string.Empty : context.Request.UrlReferrer.ToString()));
			cmd.Parameters.Add(cmd.CreateParameter("?numResults", numResults));

			using (conn)
			{
				conn.Open();

				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					Logging.LogError(ex);
				}
			}
		}
		/// <summary>
		/// Logs a step taken by a user.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="stepName"></param>
		public static void LogUserStep(HttpContext context, string stepName)
		{
			EnsureUserStepTimerStarted();

			var step = new UserStep()
			{
				Date = DateTime.Now,
				SessionID = context.Session == null ? string.Empty : context.Session.SessionID,
				StepName = stepName
			};

			lock (_UserStepLock)
			{
				_UserStepCache.Add(step);
			}
		}
		/// <summary>
		/// Ensures that the user step timer that periodically empties its step
		/// cache into the database is running.
		/// </summary>
		private static void EnsureUserStepTimerStarted()
		{
			if (_UserStepsTimer != null) return;
			lock (_UserStepLock)
			{
				if (_UserStepsTimer != null) return;
				_UserStepsTimer = new Timer(new TimerCallback(UserStepsTimer_Elapsed), null, 0, 30 * 1000);
			}
		}
		/// <summary>
		/// Writes whatever user steps in the cache into the database.
		/// </summary>
		private static void LogUserSteps()
		{
			var conn = GetLogConnection();
			if (conn == null)
			{
				if (_ThrowException) ThrowNoLogConnectionException();
				return;
			}

			var cmd = conn.CreateCommand();
			cmd.CommandText = @"INSERT INTO UserSteps (sessionID, created, stepName)
				VALUES (?sessionID, ?created, ?stepName);";
			UserStep[] steps;

			cmd.Parameters.Add(cmd.CreateParameter("?sessionID", DbType.String));
			cmd.Parameters.Add(cmd.CreateParameter("?created", DbType.DateTime));
			cmd.Parameters.Add(cmd.CreateParameter("?stepName", DbType.String));

			lock (_UserStepLock)
			{
				steps = _UserStepCache.ToArray();
				_UserStepCache.Clear();
			}

			using (conn)
			{
				try
				{
					conn.Open();

					foreach (UserStep step in steps)
					{
						cmd.Parameters["?sessionID"].Value = step.SessionID;
						cmd.Parameters["?created"].Value = step.Date;
						cmd.Parameters["?stepName"].Value = step.StepName;

						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					LogError("Logging", ex);
				}
			}
		}
		/// <summary>
		/// Logs a session creation.
		/// </summary>
		/// <param name="context"></param>
		public static void LogSessionCreate(HttpContext context)
		{
			var conn = GetLogConnection();
			if (conn == null)
			{
				if (_ThrowException) ThrowNoLogConnectionException();
				return;
			}

			var cmd = conn.CreateCommand();

			/// set the exclusion flag to true if the browser contains a Google Analytics
			/// exclusion cookie. also get the referrer URL, if available
			var excludeCookie = context.Request.Cookies[_ExcludeCookieName];
			var exclude = excludeCookie != null && excludeCookie.Value != null && excludeCookie.Value.Contains(_ExcludeCookieValue);

			cmd.Parameters.Add(cmd.CreateParameter("?sessionID", context.Session.SessionID));
			cmd.Parameters.Add(cmd.CreateParameter("?created", DateTime.Now));
			cmd.Parameters.Add(cmd.CreateParameter("?ip", context.Request.UserHostAddress));
			cmd.Parameters.Add(cmd.CreateParameter("?userAgent", context.Request.UserAgent));
			cmd.Parameters.Add(cmd.CreateParameter("?referrer", context.Request.UrlReferrer));
			cmd.Parameters.Add(cmd.CreateParameter("?exclude", exclude));

			using (conn)
			{
				conn.Open();

				try
				{
					/// make sure that there isn't already a session ID like the one we're inserting
					cmd.CommandText = "SELECT COUNT(*) FROM UserSessions WHERE sessionID=?sessionID";
					var result = cmd.ExecuteScalar();

					if (result is DBNull || Convert.ToInt32(result) == 0)
					{
						cmd.CommandText =
							@"INSERT INTO UserSessions (sessionID, created, ip, userAgent, referrer, exclude)
								VALUES (?sessionID, ?created, ?ip, ?userAgent, ?referrer, ?exclude);";
						cmd.ExecuteNonQuery();
					}
				}
				catch (Exception ex)
				{
					LogError(context, ex);
				}
			}
		}
		/// <summary>
		/// Throws an exception when a log request is made but no log
		/// connection string is found.
		/// </summary>
		private static void ThrowNoLogConnectionException()
		{
			throw new ConfigurationErrorsException("Log connection could not be created. Did you set the SharpNick.LogConnection connection string properly?");
		}
		/// <summary>
		/// Every kind of logging is failing; dump the exception details to the console as a last resort.
		/// </summary>
		/// <param name="ex"></param>
		private static void LogLastDitchError(Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
	}
	/// <summary>
	/// A configuration element for the Logging class.
	/// </summary>
	internal class LoggingConfig : ConfigurationElement
	{
		/// <summary>
		/// Gets the value that determines whether an exception should be thrown
		/// if exception logging fails.
		/// </summary>
		[ConfigurationProperty("throwException")]
		public bool ThrowException
		{
			get { return this["throwException"] == null ? false : (bool)this["throwException"]; }
		}
		/// <summary>
		/// Gets the cookie exclusion configuration element.
		/// </summary>
		[ConfigurationProperty("excludeCookie")]
		public ExcludeCookieElement ExcludeCookie
		{
			get { return this["excludeCookie"] as ExcludeCookieElement; }
		}
	}
	/// <summary>
	/// A configuration element for determining if a request should be marked
	/// as exclusion from statistical analysis.
	/// </summary>
	internal class ExcludeCookieElement : ConfigurationElement
	{
		/// <summary>
		/// Gets the cookie name to mark a request as excluded.
		/// </summary>
		[ConfigurationProperty("name", DefaultValue="__utmv")]
		public string Name
		{
			get { return this["name"] as string; }
		}
		/// <summary>
		/// Gets the value in the cookie that marks a request as excluded.
		/// </summary>
		[ConfigurationProperty("value", DefaultValue="exclude")]
		public string Value
		{
			get { return this["value"] as string; }
		}
	}
}
