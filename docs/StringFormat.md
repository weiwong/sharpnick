# StringFormat

A suite of string formatting and validation methods.

## CleanForSearch(this string input)

{code:c#}
Console.WriteLine("Something en    français**".CleanForSearch()); // "SOMETHING EN FRANCAIS"
{code:c#}

## ConsolidateSpaces(this string input)

{code:c#}
Console.WriteLine("  a b   c ".ConsolidateSpaces()); // "a b c"
{code:c#}

## EnsureLength(object input, int length)

{code:c#}
Console.WriteLine(StringFormat.EnsureLength("12345", 4)); // "1234"
Console.WriteLine(StringFormat.EnsureLength(1.23456, 5)); // "1.234"
{code:c#}

## FilterNonDigits(this string input)

{code:c#}
Console.WriteLine("abc123efg".FilterNonDigits()); // "123"
Console.WriteLine("abcdefghi".FilterNonDigits()); // ""
{code:c#}

## FormatAddress(...)

Formats an address with an input of either a [MailingAddress](MailingAddress) instance or in separate string arguments. Uses the [GeographyWizard](GeographyWizard) to determine the name of the country, if GeographyWizard is available.

{code:c#}
var address = new MailingAddress()
{
	Street1 = "One Microsoft Way",
	City = "Redmond",
	State = "WA",
	Postcode = "91245-3432",
	Country = "US"
};

Console.WriteLine(StringFormat.FormatAddress(address));
/*
	One Microsoft Way
	Redmond, WA
	United States 91245-3432
*/
{code:c#}

## FormatCreditCardNumber(this string input, string type)

{code:c#}
Console.WriteLine(StringFormat.FormatCreditCardNumber("4111222233334444")); // "4111 2222 3333 4444"
Console.WriteLine(StringFormat.FormatCreditCardNumber(""3123 4567 8901 234")); // 3123 4567890 1234"
{code:c#}

## FormatName(this string input)

{code:c#}
Console.WriteLine("april   o'neal   ".FormatName()); // "April O'Neal"
{code:c#}

## FormatPhone(this string input)

{code:c#}
Console.WriteLine("519 5551234".FormatPhone()); // "(519) 555-1234"
Console.WriteLine("519 555 1234 555".FormatPhone()); // "(519) 555-1234 x555
{code:c#}

## GetFirstSentence(this string input), GetFirstSentence(this string input, int numberOfSentences)

{code:c#}
var text = "First sentence. Second sentence, continued. Third sentence";

Console.WriteLine(text.GetFirstSentence()); // "First sentence."
Console.WriteLine(text.GetFirstsentence(2)); // "First sentence. Second sentence, continued."
{code:c#}

## GetHtmlReadyString(this string input)

{code:c#}
var text = "First line\nSecond line\r\nThird line";

Console.WriteLine(text.GetHtmlReadyString()); // "First line<br/>Second line<br/>Third line"
{code:c#}

## GetUrlFriendlyString(this string input)

{code:c#}
Console.WriteLine("Dr. Octopus's Orders".GetUrlFriendlyString()); // "dr-octopuss-orders"
{code:c#}

## IsInteger(this string input)

{code:c#}
Console.WriteLine("abc 123".IsInteger()); // false
Console.WriteLine("123.456".IsInteger()); // false
Console.WriteLine("123".IsInteger()); // true
Console.WriteLine("  -1234  ".IsInteger()); true
{code:c#}

## IsNumber(this string input)

{code:c#}
Console.WriteLine("abc 123".IsNumber()); // false
Console.WriteLine("123.456".IsNumber()); // true
Console.WriteLine("123".IsNumber()); // true
Console.WriteLine("  -1234  ".IsNumber()); true
{code:c#}

## IsValidEmail(this string input)

{code:c#}
Console.WriteLine("aaa@bbb.com".IsValidEmail()); // true
Console.WriteLine("aaa@bbb.ccc".IsValidEmail()); // true
Console.WriteLine("  aaa@bbb.ccc ".IsValidEmail()); // true
Console.WriteLine("@bbb.ccc".IsValidEmail()); // false
{code:c#}

## NeutralizeAccents(this string input)

{code:c#}
Console.WriteLine("en français"); // "en francais"
{code:c#}

## ReplaceIgnoreCase(this string input)

{code:c#}
Console.WriteLine("Something About Code".ReplaceIgnoreCase("code", "bugs")); // "Something About bugs"
{code:c#}

## ReplaceSymbols(this string input)

{code:c#}
Console.WriteLine("Hello #neighbor!!".ReplaceSymbols(" ")); // "Hello  neighbor  "
{code:c#}

## TitleCase(this string input), TitleCase(object input)

{code:c#}
Console.WriteLine("title case".TitleCase()); // "Title Case"
Console.WriteLine(StringFormat.TitleCase(string.GetType()); // "String
{code:c#}

## ToNeutralString(object input)

{code:c#}
Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
var number = 1.234

Console.WriteLine(number); // "1,234"
Console.WriteLine(StringFormat.ToNeutralString(number)); // "1.234"
{code:c#}