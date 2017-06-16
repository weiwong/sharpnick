# UniversalLoginModule

Allows an entire site to be protected behind a password. Useful for protecting test sites from public view and search engines indexing. It is _not_ meant to secure a website with sensitive data.

## How to use

Add the UniversalLoginModule to the list of modules in **Web.config**:

{code:xml}
<configuration>
	<system.webServer>
		<modules>
			<add name="UniversalLoginModule" type="SharpNick.UniversalLoginModule, SharpNick" />
		</modules>
	</system.webServer>
</configuration>
{code:xml}
Configure the module:

{code:xml}
<sharpNick>
	<universalLogin
		cookieName="name"
		cookieAuthValue="some random string"
		password="your password"
		validDays="365"
		enable="true"
	/>
</sharpNick>
{code:xml}
An explanation of the attributes:

||Attribute||Description||Default value||
|cookieName|Name of the authentication cookie|UnLogAuth|
|cookieAuthValue|Value to insert into the authentication cookie. You can change this value to invalidate all previously authenticated users and force them to log in again|_n/a_|
|password|The password used to authenticate the user|_n/a_|
|validDays|Number of days the authentication is valid for|365|
|enable|Whether to enable this module|true|
(see the [configuration reference](SharpNickConfiguration) on how to use the <sharpNick> node)

