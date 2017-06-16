
# GeographyWizard

This class aims to provide you with answers to most geographical questions you frequently encounter on a website. It is US and Canada centric now, but can be expanded to include other geographical regions.

## How to use

**Step 1: Import translation table data into a MySQL database**

{code:powershell}
mysql MyDatabase < Setup\MySql\SetupGeographyWizard.sql
{code:powershell}

**Step 2: Provide a connection string to the translation table**

{code:xml}
<?xml version="1.0"?>
<configuration>
	<connectionStrings>
		<add name="SharpNick.GeographyWizard"
			connectionString="Datasource=localhost;Database=MyDatabase"
			providerName="MySql.Data.MySqlClient"/>
	</connectionStrings>
</configuration>
{code:xml}

**Step 3: Use!**

{code:c#}
Console.WriteLine(GeographyWizard.GetCountryName("US"); // United States
Console.WriteLine(GeographyWizard.TranslateState("Ontario"); // ON
Console.WriteLine(GeographyWizard.IsStateInCountry("Alberta"), "US")); // false
{code:c#}

## Methods

**string GetCountryCodeByState(string state)**

{code:c#}
Console.WriteLine(GeographyWizard.GetCountryCodeByState("Ontario")); // CA
{code:c#}

**string GetCountryName(string countryCode)**

{code:c#}
Console.WriteLine(GeographyWizard.GetCountryName("DE")); // Germany
{code:c#}

**bool IsMilitaryAddress(string state, string country)**

{code:c#}
Console.WriteLine(GeographyWizard.IsMilitaryAddress("AF", "US")); // true
{code:c#}

**bool IsPostalBox(string street, string country)**

{code:c#}
Console.WriteLine(GeographyWizard.IsPostalBox("P.O. Box #1234", "US")); // true
Console.WriteLine(GeographyWizard.IsPostalBox("P.O. Box #1234", "DE")); // false
{code:c#}

**bool IsStateInCountry(string stateCode, string countryCode)**

{code:c#}
Console.WriteLine(GeographyWizard.IsStateInCountry("CA", "US")); // true
Console.WriteLine(GeographyWizard.IsStateInCountry("AB", "US")); // false
{code:c#}

**string TranslateCountry(string input)**

{code:c#}
Console.WriteLine(GeographyWizard.TranslateCountry("USA")); // US
Console.WriteLine(GeographyWizard.TranslateCountry("Britain")); // UK
Console.WriteLine(GeographyWizard.TranslateCountry("Mars")); // null
{code:c#}

**string TranslateState(string input)**

{code:c#}
Console.WriteLine(GeographyWizard.TranslateState("Alberta")); // AB
Console.WriteLine(GeographyWizard.TranslateState("Masachusets")); // MA
Console.WriteLine(GeographyWizard.TranslateState("Mars")); // null
{code:c#}