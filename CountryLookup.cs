using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using System.Reflection;

namespace SharpNick
{
	/// <summary>
	/// Class to find out what country an IP is designated to.
	/// </summary>
	/// <remarks>Code adapted from MaxMind http://www.maxmind.com/app/csharp, with enhancements including
	/// higher performance by keeping the database file open and automatic database file updating.</remarks>
	public sealed class CountryLookup
	{
		#region Constants
		private const long COUNTRY_BEGIN = 16776960;
		/// <summary>
		/// List of country codes.
		/// </summary>
		private static readonly string[] _CountryCode = 
							{ null,"AP","EU","AD","AE","AF","AG","AI","AL","AM","AN","AO","AQ","AR","AS","AT","AU","AW","AZ","BA","BB","BD","BE","BF","BG","BH","BI","BJ","BM","BN","BO","BR","BS","BT","BV","BW","BY","BZ","CA","CC","CD","CF","CG","CH","CI","CK","CL","CM","CN","CO","CR","CU","CV","CX","CY","CZ","DE","DJ","DK","DM","DO","DZ",
								"EC","EE","EG","EH","ER","ES","ET","FI","FJ","FK","FM","FO","FR","FX","GA","GB","GD","GE","GF","GH","GI","GL","GM","GN","GP","GQ","GR","GS","GT","GU","GW","GY","HK","HM","HN","HR","HT","HU","ID","IE","IL","IN","IO","IQ","IR","IS","IT","JM","JO","JP","KE","KG","KH","KI","KM","KN","KP","KR","KW","KY","KZ",
								"LA","LB","LC","LI","LK","LR","LS","LT","LU","LV","LY","MA","MC","MD","MG","MH","MK","ML","MM","MN","MO","MP","MQ","MR","MS","MT","MU","MV","MW","MX","MY","MZ","NA","NC","NE","NF","NG","NI","NL","NO","NP","NR","NU","NZ","OM","PA","PE","PF","PG","PH","PK","PL","PM","PN","PR","PS","PT","PW","PY","QA",
								"RE","RO","RU","RW","SA","SB","SC","SD","SE","SG","SH","SI","SJ","SK","SL","SM","SN","SO","SR","ST","SV","SY","SZ","TC","TD","TF","TG","TH","TJ","TK","TM","TN","TO","TL","TR","TT","TV","TW","TZ","UA","UG","UM","US","UY","UZ","VA","VC","VE","VG","VI","VN","VU","WF","WS","YE","YT","RS","ZA","ZM","ME","ZW","A1","A2",
								"O1","AX","GG","IM","JE","BL","MF"
								};
		/// <summary>
		/// List of country names.
		/// </summary>
		private static readonly string[] _CountryName = 
							{ null ,"Asia/Pacific Region","Europe","Andorra","United Arab Emirates","Afghanistan","Antigua and Barbuda","Anguilla","Albania","Armenia","Netherlands Antilles","Angola","Antarctica","Argentina","American Samoa","Austria","Australia","Aruba","Azerbaijan","Bosnia and Herzegovina","Barbados","Bangladesh","Belgium",
								"Burkina Faso","Bulgaria","Bahrain","Burundi","Benin","Bermuda","Brunei Darussalam","Bolivia","Brazil","Bahamas","Bhutan","Bouvet Island","Botswana","Belarus","Belize","Canada","Cocos (Keeling) Islands","Congo, The Democratic Republic of the","Central African Republic","Congo","Switzerland","Cote D'Ivoire",
								"Cook Islands","Chile","Cameroon","China","Colombia","Costa Rica","Cuba","Cape Verde","Christmas Island","Cyprus","Czech Republic","Germany","Djibouti","Denmark","Dominica","Dominican Republic","Algeria","Ecuador","Estonia","Egypt","Western Sahara","Eritrea","Spain","Ethiopia","Finland","Fiji","Falkland Islands (Malvinas)",
								"Micronesia, Federated States of","Faroe Islands","France","France, Metropolitan","Gabon","United Kingdom","Grenada","Georgia","French Guiana","Ghana","Gibraltar","Greenland","Gambia","Guinea","Guadeloupe","Equatorial Guinea","Greece","South Georgia and the South Sandwich Islands","Guatemala","Guam","Guinea-Bissau","Guyana",
								"Hong Kong","Heard Island and McDonald Islands","Honduras","Croatia","Haiti","Hungary","Indonesia","Ireland","Israel","India","British Indian Ocean Territory","Iraq","Iran, Islamic Republic of","Iceland","Italy","Jamaica","Jordan","Japan","Kenya","Kyrgyzstan","Cambodia","Kiribati","Comoros","Saint Kitts and Nevis",
								"Korea, Democratic People's Republic of","Korea, Republic of","Kuwait","Cayman Islands","Kazakstan","Lao People's Democratic Republic","Lebanon","Saint Lucia","Liechtenstein","Sri Lanka","Liberia","Lesotho","Lithuania","Luxembourg","Latvia","Libyan Arab Jamahiriya","Morocco","Monaco","Moldova, Republic of","Madagascar",
								"Marshall Islands","Macedonia","Mali","Myanmar","Mongolia","Macau","Northern Mariana Islands","Martinique","Mauritania","Montserrat","Malta","Mauritius","Maldives","Malawi","Mexico","Malaysia","Mozambique","Namibia","New Caledonia","Niger","Norfolk Island","Nigeria","Nicaragua","Netherlands",
								"Norway","Nepal","Nauru","Niue","New Zealand","Oman","Panama","Peru","French Polynesia","Papua New Guinea","Philippines","Pakistan","Poland","Saint Pierre and Miquelon","Pitcairn Islands","Puerto Rico","Palestinian Territory","Portugal","Palau","Paraguay","Qatar","Reunion","Romania","Russian Federation","Rwanda","Saudi Arabia",
								"Solomon Islands","Seychelles","Sudan","Sweden","Singapore","Saint Helena","Slovenia","Svalbard and Jan Mayen","Slovakia","Sierra Leone","San Marino","Senegal","Somalia","Suriname","Sao Tome and Principe","El Salvador","Syrian Arab Republic","Swaziland","Turks and Caicos Islands","Chad","French Southern Territories","Togo",
								"Thailand","Tajikistan","Tokelau","Turkmenistan","Tunisia","Tonga","Timor-Leste","Turkey","Trinidad and Tobago","Tuvalu","Taiwan","Tanzania, United Republic of","Ukraine","Uganda","United States Minor Outlying Islands","United States","Uruguay","Uzbekistan","Holy See (Vatican City State)","Saint Vincent and the Grenadines",
								"Venezuela","Virgin Islands, British","Virgin Islands, U.S.","Vietnam","Vanuatu","Wallis and Futuna","Samoa","Yemen","Mayotte","Serbia","South Africa","Zambia","Montenegro","Zimbabwe","Anonymous Proxy","Satellite Provider",
								"Other","Aland Islands","Guernsey","Isle of Man","Jersey","Saint Barthelemy","Saint Martin"};
		/// <summary>
		/// The culture to use when interpreting results from MaxMind database.
		/// </summary>
		private static readonly CultureInfo _CultureUS = new CultureInfo("en-US");
		#endregion

		#region Variables
		/// <summary>
		/// Multithreading lock for the database file.
		/// </summary>
		private static object _DatabaseFileLock = new object();
		/// <summary>
		/// Reference to the open database file.
		/// </summary>
		private static FileStream _DatabaseFile;
		/// <summary>
		/// Value that determines if this class is ready for use.
		/// </summary>
        private static bool _IsReady = false;
		/// <summary>
		/// Path to the folder containing the database file.
		/// </summary>
		private static readonly string _StorePath = GetStorePath();
		/// <summary>
		/// Name of the database file as specified in the configuration file.
		/// </summary>
		private const string _DBFileName = "MaxMindCountryIPDB.dat";
		/// <summary>
		/// Name of the database file as specified in the configuration file.
		/// </summary>
		private const string _TempDBFileName = "MaxMindCountryIPDBTemp.dat";
		/// <summary>
		/// Timer to update database file.
		/// </summary>
		private static Timer _UpdateTimer;
		/// <summary>
		/// Event to fire when the current AppDomain unloads.
		/// </summary>
		private static EventHandler _DomainUnloadEventHandler;
		/// <summary>
		/// The license key to use with MaxMind.
		/// </summary>
		private static string _LicenseKey;
		/// <summary>
		/// The path to the application's executing directory.
		/// </summary>
		private static string _AppDirectory = GetAppDirectory();
		#endregion

		private CountryLookup() { }
		/// <summary>
		/// Gets the directory to store the database file in.
		/// </summary>
		/// <returns>The current executing directory of the application if the application is not a web
		/// application, or the path to the web application's App_Data directory otherwise.</returns>
		private static string GetStorePath()
		{
			if (HostingEnvironment.ApplicationPhysicalPath == null) return Directory.GetCurrentDirectory();
			else return HostingEnvironment.MapPath("~/App_Data/");
		}
		/// <summary>
		/// Gets the directory the current application is executing in. If current application is a web
		/// application, return the root directory.
		/// </summary>
		/// <returns></returns>
		private static string GetAppDirectory()
		{
			if (HostingEnvironment.ApplicationPhysicalPath == null) return Directory.GetCurrentDirectory();
			else return HostingEnvironment.MapPath("~/");
		}
		/// <summary>
		/// Ensures that this class is ready for action.
		/// </summary>
		private static void EnsureReady()
		{
			if (_IsReady) return;
			lock (_DatabaseFileLock)
			{
				if (_IsReady) return;

				_LicenseKey = GetLicenseKey();
				var newFile = EnsureFileAvailable();
				OpenFile();

				/// bind event to close file
				if (_DomainUnloadEventHandler == null)
				{
					_DomainUnloadEventHandler = new EventHandler(CurrentDomain_DomainUnload);
					AppDomain.CurrentDomain.DomainUnload += _DomainUnloadEventHandler;
				}

				/// start timer to update the file
				if (_UpdateTimer == null && _LicenseKey != null)
				{
					var update = new TimerCallback(delegate(object state)
					{
						try
						{
							UpdateDB();
						}
						catch (Exception ex)
						{
							Logging.LogError("CountryLookup", ex);
						}
					});

					/// check every day at 7am
					var now = DateTime.Now;
					var start = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
					if (start <= now) start = start.AddDays(1);
					var due = (long)(start - now).TotalMilliseconds;

					_UpdateTimer = new Timer(update, null, due, 24 * 60 * 60 * 1000);

					/// check in one minute if the normal check doesn't start in 5 minutes
					if (!newFile && due > 5 * 60 * 1000) new Timer(update, null, 60 * 1000, Timeout.Infinite);

					try
					{
						Logging.Trace("File update timer started", "CountryLookup", _AppDirectory);
					}
					catch (Exception ex)
					{
						Logging.LogError("CountryLookup", ex);
					}
				}

				_IsReady = true;
			}
		}
		/// <summary>
		/// Ensures that the database file is ready for this class. If not, download it
		/// from MaxMind provided the license key is available.
		/// </summary>
		/// <returns></returns>
		private static bool EnsureFileAvailable()
		{
			if (_LicenseKey == null) return false;

			var countryLookupFileName = Path.Combine(_StorePath, _DBFileName);
			
			if (File.Exists(countryLookupFileName)) return false;
			UpdateDB();

			return true;
		}
		/// <summary>
		/// Gets the license key from the configuration file.
		/// </summary>
		/// <returns></returns>
		private static string GetLicenseKey()
		{
			var config = SharpNickConfiguration.GetConfig();
			if (config == null || config.CountryLookupConfig == null) return null;
			return config.CountryLookupConfig.MaxMindLicenseKey;
		}
		/// <summary>
		/// Opens the database file by assigning an opened stream to _DatabaseFile.
		/// </summary>
		private static void OpenFile()
		{
			/// open database file
			try
			{
				string countryLookupFileName = Path.Combine(_StorePath, _DBFileName);
				_DatabaseFile = new FileStream(countryLookupFileName, FileMode.Open, FileAccess.Read);
			}
			catch (Exception ex)
			{
				Logging.LogError("CountryLookup", ex);
			}
		}
		/// <summary>
		/// Clean up resources reserved by this class.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
			Logging.Trace("Closing down services", "CountryLookup", _AppDirectory);
            CloseFile();
			if (_UpdateTimer != null) _UpdateTimer.Dispose();
        }
		/// <summary>
		/// Gets the country code corresponding to the specified IP.
		/// </summary>
		/// <param name="ip">The IP address to get the country code of.</param>
		/// <returns></returns>
		public static string GetCountryCode(string ip)
		{
			try
			{
				return GetCountryCode(IPAddress.Parse(ip));
			}
			catch
			{
				return null;
			}
		}
		/// <summary>
		/// Converts an IP address into a number.
		/// </summary>
		/// <param name="addr">The IP address to convert from..</param>
		/// <returns></returns>
		private static long AddressToNumber(IPAddress addr)
		{
			long ipnum = 0;
			byte[] b = addr.GetAddressBytes();

			for (int i = 0; i < 4; ++i)
			{
				long y = b[i];
				if (y < 0) y += 256;

				ipnum += y << ((3 - i) * 8);
			}

			return ipnum;
		}
		/// <summary>
		/// Gets the country code corresponding to the specified IPAddress instance.
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static string GetCountryCode(IPAddress ip)
		{
			return (_CountryCode[(int)SeekCountry(0, AddressToNumber(ip), 31)]);
		}
		/// <summary>
		/// Gets the country name corrresponding to the specified IP.
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static string GetCountryName(string ip)
		{
			try
			{
				return GetCountryName(IPAddress.Parse(ip));
			}
			catch
			{
				return null;
			}
		}
		/// <summary>
		/// Gets the country name corrresponding to the specified IPAddress instance.
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static string GetCountryName(IPAddress addr)
		{
			return (_CountryName[(int)SeekCountry(0, AddressToNumber(addr), 31)]);
		}
		/// <summary>
		/// Looks for the index corresponding to the country code in the database file.
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="ipnum"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		private static long SeekCountry(long offset, long ipnum, int depth)
		{
			EnsureReady();

			var buf = new byte[6];
			var x = new long[2];

			if (depth < 0) return 0;

			if (_DatabaseFile == null)
			{
				throw new InvalidOperationException("Database file is not ready. Please check that the " +
					"database file exists, there are no permission issues accessing the database file or that " +
					"a license key is provided in the configuration file.");
			}

			lock (_DatabaseFileLock)
			{
				_DatabaseFile.Seek(6 * offset, 0);
				_DatabaseFile.Read(buf, 0, 6);
			}

			for (int i = 0; i < 2; i++)
			{
				x[i] = 0;

				for (int j = 0; j < 3; j++)
				{
					int y = buf[i * 3 + j];
					if (y < 0) y += 256;

					x[i] += (y << (j * 8));
				}
			}

			if ((ipnum & (1 << depth)) > 0)
			{
				if (x[1] >= COUNTRY_BEGIN)
				{
					return x[1] - COUNTRY_BEGIN;
				}

				return SeekCountry(x[1], ipnum, depth - 1);
			}
			else
			{
				if (x[0] >= COUNTRY_BEGIN)
				{
					return x[0] - COUNTRY_BEGIN;
				}

				return SeekCountry(x[0], ipnum, depth - 1);
			}
		}
		/// <summary>
		/// Clean up resources used by closing the file. File will automatically
		/// reopen if needed.
		/// </summary>
		private static void CloseFile()
		{
			lock (_DatabaseFileLock)
			{
				if (_DatabaseFile != null) _DatabaseFile.Dispose();
			}
		}
        /// <summary>
        /// Checks if the GeoIP.dat has a newer version everyday at 6am
        /// </summary>
        private static void UpdateDB()
        {
			/// quit if no license key available
			if (string.IsNullOrEmpty(_LicenseKey))
			{
				Logging.Trace("Cannot update database file because license key is not set.", "CountryLookup", "CountryLookup");
				return;
			}

			/// maxmind requires a request made to their server with a license key and
			/// the MD5 hash of the current database file
			var existingFile = Path.Combine(_StorePath, _DBFileName);
			var fileMd5 = GetMD5HashFromFile(existingFile);
            var versionFileUrl = string.Format("http://www.maxmind.com/app/update?license_key={0}&md5={1}", _LicenseKey, fileMd5);
            byte[] result;

            using (WebClient client = new WebClient())
            {
				try { Logging.Trace("Checking for updates", "CountryLookup", _AppDirectory); }
				catch (Exception ex) { Logging.LogError("CountryLookup", ex); }
					
				result = client.DownloadData(versionFileUrl);
            }

			/// maxmind returns a string if there are no new updates. if there
			/// are, replace the current file with the newly downloaded version
            if (result.Length > 27 || Encoding.ASCII.GetString(result) != "No new updates available\n")
			{
				try { Logging.Trace("Updating file", "CountryLookup", _AppDirectory); }
				catch (Exception ex) { Logging.LogError("CountryLookup", ex); }

				/// write the downloaded byte array into a file, first
				/// decompressing it
				var newFilePath = Path.Combine(_StorePath, _TempDBFileName);
                using (var fInStream = new MemoryStream(result))
                using (var zipStream = new GZipStream(fInStream, CompressionMode.Decompress))
				using (var fOutStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] tempBytes = new byte[4096];
                    int i;
                    while ((i = zipStream.Read(tempBytes, 0, tempBytes.Length)) != 0)
                    {
                        fOutStream.Write(tempBytes, 0, i);
                    }
                }

				/// replace the old file with the new
                lock (_DatabaseFileLock)
                {
					try
					{
						if (_DatabaseFile != null)
						{
							_DatabaseFile.Close();
							_DatabaseFile = null;
						}

						if (File.Exists(existingFile)) File.Delete(existingFile);
						File.Move(newFilePath, existingFile);
					}
					finally
					{
						OpenFile();
					}
                }
            }
        }
        /// <summary>
        /// Gets MD5 hash of a file.
        /// </summary>
        /// <param name="fileName">Path to the file to get the MD5 hash of.</param>
        /// <returns></returns>
        private static string GetMD5HashFromFile(string fileName)
        {
			if (File.Exists(fileName))
			{
				byte[] b = File.ReadAllBytes(fileName);
				string sum = BitConverter.ToString(new MD5CryptoServiceProvider().ComputeHash(b)).Replace("-", "").ToLower(_CultureUS);
				return sum;
			}

			return null;
        }
	}
	/// <summary>
	/// Configuration element for configuring the CountryLookup service.
	/// </summary>
	internal class CountryLookupConfig : ConfigurationElement
	{
		/// <summary>
		/// Gets the license key provided by MaxMind to get database file updates from their website.
		/// </summary>
		[ConfigurationProperty("maxMindLicenseKey", IsRequired=false)]
		public string MaxMindLicenseKey
		{
			get { return this["maxMindLicenseKey"] as string; }
		}
	}
}