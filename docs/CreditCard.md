# CreditCard

Provides common functions performed on credit cards.

## bool CheckCardNumberWithLuhn(string cardNumber)

Determines if a credit card number validates against the [Luhn algorithm](http://en.wikipedia.org/wiki/Luhn_algorithm). This method allows you check the validity of a credit card before hitting your credit card provider.

## string GetCardType(string cardNumber)

Determines the card type based on the card number. Return values are _MASTER_, _VISA_, _DISCOVER_, _AMEX_, or null

**Example**
{code:c#}
Console.WriteLine(CreditCard.GetCardType("4000 0000 0000 0000")); // VISA
Console.WriteLine(CreditCard.GetCardType("5200123456789012")); // MASTER
{code:c#}

## string GetCardTypeName(string cardNumber)

Determines the name of the card based on its card number.

**Example**
{code:c#}
Console.WriteLine(CreditCard.GetCardTypeName("5200 1234 5678 9012")); // MasterCard
{code:c#}

## string GetCardTypeNameFromType(string typeString)

Determines the name of the name of the card based on the type string as returned by _GetCardType_.

**Example**
{code:c#}
Console.WriteLine(CreditCard.GetCardTypeNameFromType("AMEX")); // American Express
{code:c#}

||Type||Return value||
|MASTER|MasterCard|
|VISA|Visa|
|DISCOVER|Discover|
|AMEX|American Express|

## As an instance

For convenient passing of credit card parameters in your program, this class can also be instantiated and filled with the following credit card properties: Cvv, Expiry, Name, and Number.

{code:c#}
var card = new CreditCard
{
	Cvv = "111",
	Expiry = new DateTime(2010, 1, 31, 23, 59, 59), // for 01/10
	Name = "SharpNick"
	Number = "4000 1234 5678 9012";
};

Console.WriteLine(card.Type); // VISA
HypotheticalPaymentGateway.ChargeCard(card, 10);
{code:c#}