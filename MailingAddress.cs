using System;

namespace SharpNick
{
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
			return (Street1 + Street2 + City + State + Country + Postcode).ToLower().GetHashCode();
		}
	}
}
