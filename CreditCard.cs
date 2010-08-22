using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpNick
{
	/// <summary>
	/// Represents a credit card and exposes convenience methods when dealingwith credit cards.
	/// </summary>
	public sealed class CreditCard
	{
		#region Properties
		/// <summary>
		/// Gets or sets the number on this credit card.
		/// </summary>
		public string Number { get; set; }
		/// <summary>
		/// Gets the type of this credit card.
		/// </summary>
		public string Type
		{
			get { return GetCardType(this.Number); }
		}
		/// <summary>
		/// Gets or sets the card verification value of this credit card.
		/// </summary>
		public string Cvv { get; set; }
		/// <summary>
		/// Gets or sets the expiry date of this credit card.
		/// </summary>
		public DateTime Expiry { get; set; }
		/// <summary>
		/// Gets or sets the name on this credit card.
		/// </summary>
		public string Name { get; set; }
		#endregion

		/// <summary>
		/// Sum table to faciliate the Luhn algorithm calculations.
		/// </summary>
		private static readonly int[][] LuhnSumTable = { new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new int[] { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 } };
		public CreditCard() { }
		/// <summary>
		/// Determine the type of a credit card given a credit card number.
		/// </summary>
		/// <remarks>
		/// American Express 	34xxxx, 37xxxx 	15
		/// VISA 	4xxxxx 	13, 16
		/// MasterCard 	51xxxx-55xxxx 	16
		/// Diner's Club/Carte Blanche 	 300xxx-305xxx, 36xxxx, 38xxxx 	 14 NOTSUPPORTED NOW
		/// Discover 	6011, 622126-622925, 644-649, 65 	16 NOTSUPPORTED NOW</remarks>
		/// <returns>"VISA", "AMEX", "MASTER", "DISCOVER" or null.</returns>
		public static string GetCardType(string cardNumber)
		{
			cardNumber = StringFormat.FilterNonDigits(cardNumber);

			int length = cardNumber.Length;
			if (length <= 2) return null;

			char a, b;
			a = cardNumber[0];
			b = cardNumber[1];

			if (length == 16)
			{
				if (a == '4') return "VISA";
				else if (a == '5' && b >= '1' && b <= '5') return "MASTER";
				else if (a == '6' && b >= '0' && b <= '6') return "DISCOVER";
			}
			else if (length == 15 && a == '3' && (b == '7' || b == '4'))
			{
				return "AMEX";
			}
			else if (length == 13 && a == '4')
			{
				return "VISA";
			}

			return null;
		}
		/// <summary>
		/// Returns the friendly name of the specified credit card number.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static string GetCardTypeName(string cardNumber)
		{
			return GetCardTypeNameFromType(GetCardType(cardNumber));
		}
        /// <summary>
        /// Returns a friendly name of the specified credit card type short string
        /// (e.g. VISA, MASTER, DISCOVER, AMEX)
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static string GetCardTypeNameFromType(string typeString)
        {
            switch (typeString)
            {
                case "VISA": return "Visa";
                case "MASTER": return "MasterCard";
                case "AMEX": return "American Express";
                case "DISCOVER": return "Discover";
            }

            return null;
        }
		/// <summary>
		/// Validates a credit card number using the Luhn algorithm.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <returns></returns>
		public static bool CheckCardNumberWithLuhn(string cardNumber)
		{
			if (string.IsNullOrEmpty(cardNumber)) throw new ArgumentNullException("cardNumber");
			cardNumber = StringFormat.FilterNonDigits(cardNumber);
			if (string.IsNullOrEmpty(cardNumber)) throw new ArgumentException("Card number does not contain any digits.", "cardNumber");

			int sum = 0;
			int flip = 0;
			int cardNumberLength = cardNumber.Length - 1;

			while (cardNumberLength >= 0)
			{
				sum += LuhnSumTable[flip & 1][cardNumber[cardNumberLength] - '0'];
				cardNumberLength--;
				flip++;
			}

			return ((sum % 10) == 0); 
		}
	}
}
