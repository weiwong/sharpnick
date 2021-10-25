# UniversalLoginModule

Allows an entire site to be protected behind a password. Useful for protecting test sites from public view and search engines indexing. It is _not_ meant to secure a website with sensitive data.

## How to use

Add the UniversalLoginModule to the list of modules in **Web.config**:


	<configuration>
		<system.webServer>
			<modules>
				<add name="UniversalLoginModule" type="SharpNick.UniversalLoginModule, SharpNick" />
			</modules>
		</system.webServer>
	</configuration>

Configure the module:


	<sharpNick>
		<universalLogin
			cookieName="name"
			cookieAuthValue="some random string"
			password="your password"
			validDays="365"
			enable="true"
		/>
	</sharpNick>

An explanation of the attributes:

<table>
	<thead>
		<tr>
			<th>Attribute</th>
			<th>Description</th>
			<th>Default</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td>cookieName</td>
			<td>Name of the authentication cookie</td>
			<td>UnLogAuth</td>
		</tr>
		<tr>
			<td>cookieAuthValue</td>
			<td>Value to insert into the authentication cookie. You can change this value to invalidate all previously authenticated users and force them to log in again</td>
			<td><em>n/a</em></td>
		</tr>
		<tr>
			<td>password</td>
			<td>The password used to authenticate the user</td>
			<td><em>n/a</em></td>
		</tr>
		<tr>
			<td>validDays</td>
			<td>Number of days the authentication is valid for</td>
			<td>365</td>
		</tr>
		<tr>
			<td>enable</td>
			<td>Whether to enable this module</td>
			<td>true</td>
		</tr>
	</tbody>
<table>

(see the [configuration reference](SharpNickConfiguration) on how to use the <sharpNick> node)
