using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace SharpNick
{
	/// <summary>
	/// Convenience methods when dealing with strings.
	/// </summary>
	public static class StringFormat
	{
		/// <summary>
		/// Regex instance that determines if a string is a valid integer.
		/// </summary>
		private static readonly Regex IntegerRegex = new Regex(@"^\s*-?\d+\s*$", RegexOptions.Compiled);
		/// <summary>
		/// Regex instance that determines if a string is a valid number.
		/// </summary>
		private static readonly Regex NumberRegex = new Regex(@"^\s*-?\d+(?:\.\d+)?\s*$", RegexOptions.Compiled);
		/// <summary>
		/// A (semi-)universal culture for globally understandable formatting.
		/// </summary>
		private static readonly CultureInfo EnglishCulture = new CultureInfo("en-US");
		/// <summary>
		/// Regex instance that determines if a string is a valid email.
		/// </summary>
		private static readonly Regex EmailRegex = new Regex(@"^[-_a-z0-9'+*$^&%=~!?{}]+(?:\.[-_a-z0-9'+*$^&%=~!?{}]+)*@(?:(?![-.])[-a-z0-9.]+(?<![-.])\.[a-z]{2,6}|\d{1,3}(?:\.\d{1,3}){3})(?::\d+)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		/// <summary>
		/// Regex instance that finds string sequence of two or more space characters.
		/// </summary>
		private static readonly Regex ExtraSpacesRegex = new Regex(@" {2,}", RegexOptions.Compiled);

		/// <summary>
		/// Gets the name of the category optimized for URL.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string GetUrlFriendlyString(this string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;

			string result = input.Trim();
			if (string.IsNullOrEmpty(result)) return string.Empty;

			result = NeutralizeAccents(input);
			result = result.Replace("'", "");
			result = ReplaceSymbols(result, "-");

			// replace any two consecutive dashes with one dash
			while (result.IndexOf("--") > -1)
			{
				result = result.Replace("--", "-");
			}

			result = result.Trim('-');

			result = result.ToLower();

			return result;
		}
		/// <summary>
		/// Replaces any non-alphanumeric characters in a string with a specified
		/// replacement string.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string ReplaceSymbols(this string input, string replacement)
		{
			return ReplaceSymbols(input, replacement, false);
		}
		/// <summary>
		/// Replaces any non-alphanumeric characters in a string with a specified
		/// replacement string, with an option to leave spaces as they are.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="replacement"></param>
		/// <param name="ignoreSpaces"></param>
		/// <returns></returns>
		public static string ReplaceSymbols(this string input, string replacement, bool ignoreSpaces)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;

			var regex = ignoreSpaces ? @"[^a-zA-Z\d\s]" : @"[^a-zA-Z\d]";
			return Regex.Replace(input, regex, replacement);
		}
		/// <summary>
		/// Encodes a string so that it can be displayed into a HTML page
		/// with proper formatting (ie line breaks replaced with &lt;bt&gt; etc).
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string GetHtmlReadyString(this string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;

			var result = HttpUtility.HtmlEncode(input);
			result = Regex.Replace(result, @"(?:(?:\r\n)|\n|\r)", "<br />", RegexOptions.Compiled);

			return result;
		}
		/// <summary>
		/// Formats a string into a phone number format.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string FormatPhone(this string input)
		{
			string sanitized = FilterNonDigits(input);

			// if length is not 9, cannot format; return string as is
			if (sanitized.Length < 10) return input;

			// if length is 10 or more, format it into (xxx) yyy-zzzz format
			string result = string.Format("({0}) {1}-{2}", sanitized.Substring(0, 3),
				sanitized.Substring(3, 3), sanitized.Substring(6, 4));

			// add the rest of the numbers as extension
			if (sanitized.Length > 10) result += " x " + sanitized.Substring(10);

			return result;
		}
		/// <summary>
		/// Formats a string into card number format, delimited by spaces.
		/// </summary>
		/// <param name="input">Optional. Specifies the card type to format the input to.</param>
		/// <returns></returns>
		public static string FormatCreditCardNumber(this string input)
		{
			string sanitized = FilterNonDigits(input);

			if (input.Length == 15)
			{
				// format Amex
				return string.Format("{0} {1} {2}", sanitized.Substring(0, 4),
					sanitized.Substring(4, 6), sanitized.Substring(10));
			}

			if (input.Length == 16)
			{
				// format for Visa, mastercard
				return string.Format("{0} {1} {2} {3}", sanitized.Substring(0, 4),
					sanitized.Substring(4, 4), sanitized.Substring(8, 4),
					sanitized.Substring(12));
			}

			return input;
		}
		/// <summary>
		/// Remove non-digits from a string.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string FilterNonDigits(this string input)
		{
			return Regex.Replace(input, @"\D", "");
		}
		/// <summary>
		/// Converts an object into a culture neutral string regardless of the thread's defined
		/// culture.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string ToNeutralString(object input)
		{
			if (input == null) return string.Empty;
			if (input is float) return ((float)input).ToString(EnglishCulture);
			if (input is decimal) return ((decimal)input).ToString(EnglishCulture);
			return input.ToString();
		}
		/// <summary>
		/// Determines if an input string is an integer.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool IsInteger(this string input)
		{
			if (input == null) return false;
			input = input.Trim();
			if (input.Length == 0) return false;
			return IntegerRegex.IsMatch(input);
		}
		/// <summary>
		/// Determines if an input string is a number (negative numbers and decimals acceptable).
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool IsNumber(this string input)
		{
			if (input == null) return false;
			if (input.Length == 0) return false;
			return NumberRegex.IsMatch(input);
		}
		/// <summary>
		/// Uppercases a string, simplify French accents and removes any characters
		/// other than spaces.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string CleanForSearch(this string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;

			string output = input.ToUpper();

			// remove french accents
			output = NeutralizeAccents(output);

			// keep only alphabets, numbers and spaces
			output = Regex.Replace(output, "[^A-Z0-9 ]", "");

			return output;
		}
		/// <summary>
		/// Converts accented characters in a string to a neutral counterparts, e.g. 'Â' becomes 'A'.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string NeutralizeAccents(this string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Replace('À', 'A').Replace('Á', 'A').Replace('Â', 'A').Replace('Ã', 'A').Replace('Ä', 'A').Replace('Å', 'A')
			  .Replace('Ç', 'C')
			  .Replace('È', 'E').Replace('É', 'E').Replace('Ê', 'E').Replace('Ë', 'E').Replace('Ì', 'I')
			  .Replace('Í', 'I').Replace('Î', 'I').Replace('Ï', 'I').Replace('Ñ', 'N')
			  .Replace('Ò', 'O').Replace('Ó', 'O').Replace('Ô', 'O').Replace('Õ', 'O').Replace('Ö', 'O')
			  .Replace('Ù', 'U').Replace('Ú', 'U').Replace('Û', 'U').Replace('Ü', 'U')
			  .Replace('Ý', 'Y')
			  .Replace('à', 'a').Replace('á', 'a').Replace('å', 'a').Replace('ã', 'a').Replace('ä', 'a').Replace('â', 'a')
			  .Replace('ç', 'c')
			  .Replace('è', 'e').Replace('é', 'e').Replace('ê', 'e').Replace('ë', 'e').Replace('ì', 'i')
			  .Replace('í', 'i').Replace('î', 'i').Replace('ï', 'i').Replace('ñ', 'n')
			  .Replace('ò', 'o').Replace('ó', 'o').Replace('ô', 'o').Replace('ö', 'o')
			  .Replace('ù', 'u').Replace('ú', 'u').Replace('û', 'u').Replace('ü', 'u')
			  .Replace('ÿ', 'y')
			  .Replace("œ", "oe").Replace("Œ", "OE");
		}
		/// <summary>
		/// Formats a string into title case.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string TitleCase(object input)
		{
			if (input == null) return string.Empty;
			return TitleCase(input.ToString());
		}
		/// <summary>
		/// Formats a string into title case.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string TitleCase(this string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;

			return Regex.Replace(input, @"\b(\w)(.*?(?:('|&rsquo;|’)s)?)\b", new MatchEvaluator(delegate(Match match)
			{
				string result = match.Groups[1].Value.ToUpper(); ;
				if (match.Groups.Count > 2) result += match.Groups[2].Value.ToLower();
				return result;
			}
			));
		}
		/// <summary>
		/// Ensures that a particular object's ToString() method is of a certain length.
		/// Truncate if the length is exceeded.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string EnsureLength(object input, int length)
		{
			if (input == null) return string.Empty;
			string result = input.ToString();
			if (result.Length > length) result = result.Remove(length);
			return result;
		}
		/// <summary>
		/// Similar to String.Replace(), but case insensitive.
		/// </summary>
		/// <remarks>Based on http://www.codeproject.com/KB/string/fastestcscaseinsstringrep.aspx </remarks>
		/// <param name="original"></param>
		/// <param name="search"></param>
		/// <param name="replacement"></param>
		/// <returns></returns>
		public static string ReplaceIgnoreCase(this string original, string search, string replacement)
		{
			int count, position0, position1;
			count = position0 = position1 = 0;
			string upperString = original.ToUpper();
			string upperPattern = search.ToUpper();
			int inc = (original.Length / search.Length) *
					  (replacement.Length - search.Length);
			char[] chars = new char[original.Length + Math.Max(0, inc)];
			while ((position1 = upperString.IndexOf(upperPattern,
											  position0)) != -1)
			{
				for (int i = position0; i < position1; ++i)
					chars[count++] = original[i];
				for (int i = 0; i < replacement.Length; ++i)
					chars[count++] = replacement[i];
				position0 = position1 + search.Length;
			}
			if (position0 == 0) return original;
			for (int i = position0; i < original.Length; ++i)
				chars[count++] = original[i];
			return new string(chars, 0, count);
		}
		/// <summary>
		/// Determines if a specified string is a valid email.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool IsValidEmail(string input)
		{
			return EmailRegex.IsMatch(input);
		}
		/// <summary>
		/// Formats an address that contains only one line of street address.
		/// </summary>
		/// <param name="street"></param>
		/// <param name="city"></param>
		/// <param name="state"></param>
		/// <param name="postcode"></param>
		/// <param name="country"></param>
		/// <returns></returns>
		public static string FormatAddress(string street, string city, string state, string postcode, string country)
		{
			return FormatAddress(street, null, city, state, postcode, country);
		}
		/// <summary>
		/// Formats an address.
		/// </summary>
		/// <param name="street1"></param>
		/// <param name="country"></param>
		/// <param name="city"></param>
		/// <param name="state"></param>
		/// <param name="postcode"></param>
		/// <param name="street2"></param>
		/// <returns></returns>
		public static string FormatAddress(string street1, string street2, string city, string state, string postcode, string country)
		{
            var countryName = GeographyWizard.IsAvailable ? GeographyWizard.GetCountryName(country) : country;
			var elements = new List<string>();
			if (!string.IsNullOrEmpty(street1)) elements.Add(street1);
			if (!string.IsNullOrEmpty(street2)) elements.Add(street2);
			elements.Add(city + ", " + state);
			elements.Add(countryName + " " + postcode);

			return string.Join(Environment.NewLine, elements.ToArray());
		}
        /// <summary>
        /// Formats an address using an instance of an address.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string FormatAddress(MailingAddress address)
        {
            return FormatAddress(address.Street1, address.Street2, address.City, address.State, address.Postcode, address.Country);
        }
		/// <summary>
		/// Gets the first sentence of a string.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string GetFirstSentence(this string input)
		{
			return GetFirstSentence(input, 1);
		}
		/// <summary>
		/// Gets the first few setences of a string.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="numberOfSentences"></param>
		/// <returns></returns>
		public static string GetFirstSentence(this string input, int numberOfSentences)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			if (input.IndexOfAny(new char[] { '.', '!', '?' }) == input.Length - 1) return input;

			int index = 0;
			int count = 0;
			while (index <= input.Length)
			{
				// find the first period after the last period found
				index = input.IndexOfAny(new char[] { '.', '!', '?' }, index + 1);
				if (index == -1) break;

				// test to see if the period is just an abbreviation
				string test = input.Substring(index - 2, 2).ToLower();
				if (test == "oz" || test == "dr" || test == "fl") continue;

				++count;
				if (count == numberOfSentences) return input.Substring(0, index + 1);
			}

			return input;
		}
		/// <summary>
		/// Makes sure that a name has no extra spaces and title cased.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string FormatName(string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return input.Trim().ConsolidateSpaces().TitleCase();
		}
		/// <summary>
		/// Makes sure that a string does not contain a continguous sequence of spaces.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string ConsolidateSpaces(this string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			return ExtraSpacesRegex.Replace(input, " ");
		}
	}
}
