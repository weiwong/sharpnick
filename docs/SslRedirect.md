# SSL Redirect Module

This module tells browsers to load certain pages in SSL, and other resources in non-SSL connections.

## How to use

**Step 1: Include the SSL module in web.config**

{code:xml}
<?xml version="1.0"?>
<configuration>
	<!-- For IIS 6 / ASP.NET Development Server -->
	<system.web>
		<httpModules>
			<add name="SslRedirectModule"
				type="SharpNick.SslRedirect.SslRedirectModule, SharpNick"/>
		</httpModules>
	</system.web>
	<!-- For IIS 7+ -->
	<system.webServer>
		<modules runAllManagedModulesForAllRequests="true">
			<add name="SslRedirectModule"
				type="SharpNick.SslRedirect.SslRedirectModule, SharpNick"/>
		</modules>
	</system.webServer>
</configuration>
{code:xml}

**Step 2: Configure pages**

{code:xml}
<sharpNick>
	<sslRedirects mode="On">
		<rules>
			<clear/>
			<!-- Allow unimportant files to be loaded in whatever referrer they are in -->
			<add match=".*\.(jpg|png|gif|css|js|ico)$" type="Ignore" isRegex="true"/>
			<!-- Allow ASP.NET special pages to be loaded in whatever referrer they are in -->
			<add match=".+\.axd$" type="Ignore" isRegex="true"/>
			<!-- Secure pages -->
			<add match="secure-page.aspx" isRegex="true"/>
		</rules>
	</sslRedirects>
</sharpNick>
{code:xml}

(see [configuration reference](SharpNickConfiguration) to use the <sharpNick> node)

**Step 3: You're done!**