using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SharpNick
{
    /// <summary>
    /// Methods to determine geographical information.
    /// </summary>
    public sealed class GeographyWizard
	{
		#region Variables
		/// <summary>
		/// Mapping from full state names to state codes, including misspellings.
		/// </summary>
        private static Dictionary<string, string> StateSpellings;
		/// <summary>
		/// Mapping from full country names to country codes, including misspellings.
		/// </summary>
		private static Dictionary<string, string> CountrySpellings;
		/// <summary>
		/// Mapping from country code to full names. 
		/// </summary>
		private static Dictionary<string, string> CountryCodeToName;
		/// <summary>
		/// Mapping from state to country.
		/// </summary>
        private static Dictionary<string, string> StateToCountryMap = GetStateToCountryMap();
		/// <summary>
		/// Multi-threading lock to make sure multiple threads are not causing data corruption.
		/// </summary>
        private static object ReadyLock = new object();
		/// <summary>
		/// Value that determines if methods in this class is ready for use.
		/// </summary>
        private static bool IsReady = false;
		/// <summary>
		/// The default culture to use when culture is indeterminant.
		/// </summary>
		private static readonly CultureInfo CultureUS = new CultureInfo("en-US");
		/// <summary>
		/// The connection string name to load the geography wizard database from.
		/// </summary>
		private const string ConnectionStringName = "SharpNick.GeographyWizard";
		#endregion;

		/// <summary>
		/// Gets the value that determines if this class can actually be used.
		/// </summary>
		public static bool IsAvailable
        {
            get
            {
                return ConfigurationManager.ConnectionStrings[ConnectionStringName] != null;
            }
        }

        private GeographyWizard() {}
		/// <summary>
		/// Ensures that this class is ready for use.
		/// </summary>
        private static void EnsureReady()
        {
            if (IsReady) return;
			lock (ReadyLock)
			{
				if (IsReady) return;
				Load();
			}
        }
        /// <summary>
        /// Loads all info from the DB.
        /// </summary>
        private static void Load()
        {
			/// get the connection string from the configuration file
			var connectionString = ConfigurationManager.ConnectionStrings[ConnectionStringName];
            if (connectionString == null) throw new ConfigurationErrorsException(
				string.Format("Connection string for \"{0}\" is not set. Connection string must be defined to use GeographyWizard.", ConnectionStringName));

			/// get the connection factory specified in the connection string and instantiate
			/// several private variables
			var factory = DbProviderFactories.GetFactory(connectionString.ProviderName);
            StateSpellings = new Dictionary<string, string>();
            CountrySpellings = new Dictionary<string, string>();
			CountryCodeToName = new Dictionary<string, string>();

			/// load up all tables
            using (var conn = factory.CreateConnection())
            {
				conn.ConnectionString = connectionString.ConnectionString;
                conn.Open();

                LoadTable(conn, "SELECT `key`, `value` FROM GeographyTable WHERE type='StateNameToCode'", StateSpellings);
				LoadTable(conn, "SELECT `key`, `value` FROM GeographyTable WHERE type='CountryNameToCode'", CountrySpellings);
				LoadTableBasic(conn, "SELECT `key`, `value` FROM GeographyTable WHERE type='CountryCodeToName'", CountryCodeToName);
            }

            IsReady = true;
        }
		/// <summary>
		/// Loads a key-value pair returned by a SQL statement into a Dictionary.
		/// </summary>
		/// <param name="conn">The connection to perform the SQL statement against.</param>
		/// <param name="p"></param>
		/// <param name="targetTable"></param>
		private static void LoadTableBasic(DbConnection conn, string sql, Dictionary<string, string> targetTable)
		{
			var cmd = conn.CreateCommand();
			cmd.CommandText = sql;

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					targetTable[reader.GetString(0)] = reader.GetString(1);
				}
			}
		}
		/// <summary>
		/// Loads a key value pair returns by a SQL statement into a Dictionary. Also add
		/// each unique value as the key as well.
		/// </summary>
		/// <param name="conn"></param>
		/// <param name="sqlSelect"></param>
		/// <param name="targetTable"></param>
        private static void LoadTable(DbConnection conn, string sqlSelect, Dictionary<string, string> targetTable)
		{
			var codes = new List<string>();
			var cmd = conn.CreateCommand();
			cmd.CommandText = sqlSelect;

			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var code = reader.GetString(1);

					var name = reader.GetString(0);
					name = name.Replace(" ", "");
					name = name.ToLower(CultureUS);

					targetTable.Add(name, code);

					/// add code itself to the list of parsable names
					if (!codes.Contains(code))
						codes.Add(code.ToLower(CultureUS));
				}
			}

            /// add the postal list of codes to the table
            foreach (var code in codes)
            {
                string dump;
                if (!targetTable.TryGetValue(code, out dump))
					targetTable.Add(code.ToLower(CultureUS), code.ToUpper(CultureUS));
            }
        }
        /// <summary>
        /// Gets the two-letter code of a state.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TranslateState(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            input = input.Trim();
            if (input.Length == 0) return null;

            EnsureReady();

            input = StringFormat.NeutralizeAccents(input);
            input = input.Replace(" ", "");
			input = input.ToLower(CultureUS);

            string result;
            if (StateSpellings.TryGetValue(input, out result)) return result;

            /// final attempt by comparing first few values
            foreach (string key in StateSpellings.Keys)
            {
                if (key.StartsWith(input)) return StateSpellings[key];
            }

            return null;
        }
        /// <summary>
        /// Gets the two-letter code of a country.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TranslateCountry(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            input = input.Trim();
            if (input.Length == 0) return null;

            EnsureReady();

            input = StringFormat.NeutralizeAccents(input);
            input = input.Replace(" ", "");
			input = input.ToLower(CultureUS);

            string result;
            if (CountrySpellings.TryGetValue(input, out result)) return result;

            /// final attempt by comparing first few values
            foreach (var key in CountrySpellings.Keys)
            {
                if (key.StartsWith(input)) return CountrySpellings[key];
            }

            return null;
        }
        /// <summary>
        /// Gets the two-letter country code by state.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        public static string GetCountryCodeByState(string state)
        {
            var normalizedState = TranslateState(state);
            if (string.IsNullOrEmpty(normalizedState)) return null;

            string result;
            StateToCountryMap.TryGetValue(normalizedState, out result);

            return result;
        }
        /// <summary>
        /// Determines if a specified state belongs to a specified country.
        /// </summary>
        /// <param name="stateCode"></param>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        public static bool IsStateInCountry(string stateCode, string countryCode)
        {
            if (stateCode == null) throw new ArgumentNullException("stateCode");
            if (countryCode == null) throw new ArgumentNullException("countryCode");
            if (countryCode.Length != 2) throw new ArgumentException("countryCode must be of length 2", "countryCode");

			countryCode = countryCode.ToUpper(CultureUS);

            if (countryCode == "US" || countryCode == "CA")
            {
                if (stateCode.Length != 2) throw new ArgumentException("stateCode must be 2 characters long for US and CA states", "stateCode");
				stateCode = stateCode.ToUpper(CultureUS);

                string countryResult;
                if (!StateToCountryMap.TryGetValue(stateCode, out countryResult)) return false;
                return countryResult == countryCode;
            }
            else
            {
                throw new ArgumentOutOfRangeException("countryCode", "Only US and CA are supported");
            }
        }
		/// <summary>
		/// Gets the name of the country corresponding to the country code.
		/// </summary>
		/// <param name="countryCode"></param>
		/// <returns></returns>
		public static string GetCountryName(string countryCode)
		{
			if (countryCode == null) return null;
			if (countryCode.Length != 2) return countryCode;

			EnsureReady();

			string result;
			if (CountryCodeToName.TryGetValue(countryCode, out result)) return result;
			return countryCode;
		}
		/// <summary>
		/// Gets the value that determines if an address is a military address.
		/// </summary>
		/// <param name="state"></param>
		/// <param name="country"></param>
		/// <returns></returns>
		public static bool IsMilitaryAddress(string state, string country)
		{
			if (country == "US")
			{
				switch (state)
				{
					case "AE":
					case "AA":
					case "AP":
						return true;
				}
			}

			return false;
		}

        /// <summary>
        /// Gets the state code to country code mapping.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> GetStateToCountryMap()
        {
            var result = new Dictionary<string, string>();

            result["AB"] = "CA";
            result["BC"] = "CA";
            result["MB"] = "CA";
            result["NB"] = "CA";
            result["NL"] = "CA";
            result["NS"] = "CA";
            result["NT"] = "CA";
            result["NU"] = "CA";
            result["ON"] = "CA";
            result["PE"] = "CA";
            result["QC"] = "CA";
            result["SK"] = "CA";
            result["YT"] = "CA";

            result["AS"] = "US";
            result["AK"] = "US";
            result["AL"] = "US";
            result["AR"] = "US";
            result["AZ"] = "US";
            result["CA"] = "US";
            result["CO"] = "US";
            result["CT"] = "US";
            result["DC"] = "US";
            result["DE"] = "US";
            result["FL"] = "US";
            result["GA"] = "US";
            result["GU"] = "US";
            result["HI"] = "US";
            result["IA"] = "US";
            result["ID"] = "US";
            result["IL"] = "US";
            result["IN"] = "US";
            result["KS"] = "US";
            result["KY"] = "US";
            result["LA"] = "US";
            result["MA"] = "US";
            result["MD"] = "US";
            result["ME"] = "US";
            result["MH"] = "US";
            result["MI"] = "US";
            result["MN"] = "US";
            result["MO"] = "US";
            result["MP"] = "US";
            result["MS"] = "US";
            result["MT"] = "US";
            result["NC"] = "US";
            result["ND"] = "US";
            result["NE"] = "US";
            result["NH"] = "US";
            result["NJ"] = "US";
            result["NM"] = "US";
            result["NV"] = "US";
            result["NY"] = "US";
            result["OH"] = "US";
            result["OK"] = "US";
            result["OR"] = "US";
            result["PA"] = "US";
            result["PR"] = "US";
            result["RI"] = "US";
            result["SC"] = "US";
            result["SD"] = "US";
            result["TN"] = "US";
            result["TX"] = "US";
            result["UT"] = "US";
            result["VA"] = "US";
            result["VI"] = "US";
            result["VT"] = "US";
            result["WA"] = "US";
            result["WI"] = "US";
            result["WV"] = "US";
            result["WY"] = "US";

            return result;
        }
		/// <summary>
		/// Determines if an address is a postal box. Supports only US and Canada.
		/// </summary>
		/// <param name="street"></param>
		/// <param name="country">A two-letter country code.</param>
		/// <returns></returns>
		public static bool IsPostalBox(string street, string country)
		{
			if (string.IsNullOrEmpty(street)) return false;

			if (country == null) return false;
			if (country.Length != 2) return false;

			country = country.ToUpper(CultureUS);
			if (country != "US" && country != "CA") return false;

			/// is a postal box if address is "box" followed by
			/// an optional "#" and then a series of digits
			return Regex.IsMatch(street, @"\bbox *#? *\d+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
    }
}