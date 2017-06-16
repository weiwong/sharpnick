# CountryLookup

Returns a country code based on an IP address.

## How to use

**Step 1: Configure MaxMind license key**

{code:xml}
<sharpNick>
	<countryLookup maxMindLicenseKey="abcdefg01234" />
</sharpNick>
{code:xml}

See [configuration reference](SharpNickConfiguration) to use the <sharpNick> node. To get a MaxMind license key, [sign up](http://www.maxmind.com/app/country) a site license with update subscriptions.

**Step 2: Set read/write permissions on App_Data**

Check this [MSDN article](http://msdn.microsoft.com/en-us/library/445z2s49(VS.80).aspx#sectionToggle1) to find out how to do that, if you've not done that already.

**Step 3: Use!**

{code:c#}
var country = CountryLookup.GetCountryCode("164.71.1.146");
Console.WriteLine(country); // US
{code:c#}

## Automatic updating

The MaxMind geoip database will go out of date within weeks. MaxMind provides a new database about every week. This class automatically downloads and replaces the old database file at 7am (computer time zone) every day.

This class downloads the database file on first use, which blocks off the calling thread (and any other new calling threads) until the file is downloaded and made available.

## Location of database file

On a ASP.NET web application, the file will be downloaded to the app's App_Data directory. For non web applications, the file will be downloaded to the executing directory.

## Tips

Use [GeographyWizard](GeographyWizard) to get the name of the country returned:

{code:c#}
var country = CountryLookup.GetCountryCode("164.71.1.146");
var name = GeographyWizard.GetCountryName(country);
Console.WriteLine(name); // United States
{code:c#}

## Credits

The code that queries the database file is based on Ting Huang's Java port, and is listed on [MaxMind](http://www.maxmind.com/app/csharp).