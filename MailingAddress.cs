﻿using System;

namespace SharpNick
{
	/// <summary>
	/// Holds information about a mailing address.
	/// </summary>
	[Serializable]
	public class MailingAddress : IEquatable<MailingAddress>
	{
		/// <summary>
		/// Gets or sets the first street address line of this location.
		/// </summary>
		public string Street1 { get; set; }
		/// <summary>
		/// Gets or sets the second street address line of this location.
		/// </summary>
		public string Street2 { get; set; }
		/// <summary>
		/// Gets or sets the name of the city of this location.
		/// </summary>
		public string City { get; set; }
		/// <summary>
		/// Gets or sets the two character state of this location.
		/// </summary>
		public string State { get; set; }
		/// <summary>
		/// Gets or sets the postal code of this location.
		/// </summary>
		public string Postcode { get; set; }
		/// <summary>
		/// Gets or sets the two character country code of this location.
		/// </summary>
		public string Country { get; set; }
		/// <summary>
		/// Gets the value that determines if this instance of MailingAddress is actually
		/// equivalent to another instance of mailing address.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(MailingAddress other)
		{
			if (other == null) return false;
			return other.GetHashCode() == this.GetHashCode();
		}
		/// <summary>
		/// Gets the hash code of this location.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			var compareKey = NeutralizeProperty(Street1) + NeutralizeProperty(Street2) +
				NeutralizeProperty(City) + NeutralizeProperty(State) + NeutralizeProperty(Country)
				+ NeutralizeProperty(Postcode);
			return compareKey.GetHashCode();
		}
		/// <summary>
		/// Removes extra spaces (including two or more consecutive spaces) and lower
		/// case the input to facilitate instance comparisons. 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private string NeutralizeProperty(string input)
		{
			if (string.IsNullOrEmpty(input)) return string.Empty;
			input = input.Trim();
			if (input.Length == 0) return string.Empty;
			return input.ConsolidateSpaces().ToLower();
		}
		/// <summary>
		/// Determines whether this MailingAddress instance has a value in any of its properties.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public static bool IsEmpty(MailingAddress address)
		{
			if (address == null) return true;
			return string.IsNullOrEmpty(address.Street1) && string.IsNullOrEmpty(address.Street2) &&
				string.IsNullOrEmpty(address.City) && string.IsNullOrEmpty(address.State)
				&& string.IsNullOrEmpty(address.Country) && string.IsNullOrEmpty(address.Postcode);
		}
	}
}
