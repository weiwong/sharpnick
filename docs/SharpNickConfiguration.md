# Configuring SharpNick

For classes in SharpNick that requires configuration, include the following _configSection_ declaration in the application's configuration file:

{code:xml}
<?xml version="1.0"?>
<configuration>
	<configSections>
		<section name="sharpNick" type="SharpNick.SharpNickConfiguration, SharpNick" />
	</configSections>
</configuration>
{code:xml}

Then anywhere in the configuration file, include the <sharpNick> node within the <configuration> nodes:

{code:xml}
<sharpNick>
<!-- Whatever configuration necessary -->
</sharpNick>
{code:xml}

When installed in a [GAC](http://www.codeproject.com/dotnet/demystifygac.asp), the assembly names will include the public key token:

{code:xml}
<section
    name="sharpNick"
    type="SharpNick.SharpNickConfiguration, SharpNick, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9348ebb1899f1a3c"
/>
{code:xml}