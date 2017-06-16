
# MailingAddress

A class used to conveniently pass address information.

## Example Use

{code:c#}
var address = new MailingAddress()
{
	Street1 = "Microsoft Corporation",
	Street2 = "One Microsoft Way",
	City = "Redmond",
	State = "WA",
	Postcode = "98052-6399",
	Country = "US"
};

HypotheticalClass.ProcessAddress(address);
{code:c#}

## Making comparisons

{code:c#}
var address1 = new MailingAddress()
{
	Street1 = "Microsoft Corporation",
	Street2 = "One Microsoft Way",
	City = "Redmond",
	State = "WA",
	Postcode = "98052-6399",
	Country = "US"
};

// equator takes care of extra spaces and different capitalizations
var address2 = new MailingAddress()
{
	Street1 = "microsoft    corporation",
	Street2 = "one microsoft way",
	City = "redmond",
	State = "wa",
	Postcode = "98052-6399",
	Country = "us  "
};

Console.WriteLine(address1.Equals(address2)); // true

// but it's not smart enough to infer numbers
var address3 = new MailingAddress()
{
	Street1 = "Microsoft Corporation",
	Street2 = "1 Microsoft Way",
	City = "Redmond",
	State = "WA",
	Postcode = "98052-6399",
	Country = "US"
};

Console.WriteLine(address1.Equals(address3)); // false
{code:c#}

## Validation

MailingAddress class does not support validation of addresses. However, you can use [GeographyWizard](GeographyWizard) to figure out if the states and countries entered are correct.