# VersionTagger

Rewrites Javascript and CSS references in an HTML page by inserting a Murmur hash value of the files. Uses the [Response.Filter](http://msdn.microsoft.com/en-us/library/system.web.httpresponse.filter.aspx) method.

## How to use

**Step 1: Add the following lines in the Global.asax file's BeginRequest event:**

{code:c#}
if (Request.Url.LocalPath.EndsWith(".aspx")) Response.Filter = new VersionTagger(Response);
else VersionTagger.Rewrite(Context);
{code:c#}

**Step 2: For IIS7+, set _runAllManagedModulesForAllRequests_ in the Web.config to true:**

This ensures that the _BeginRequest_ event will catch any CSS/JS files.

{code:xml}
<?xml version="1.0"?>
<configuration>
	<system.webServer>
		<modules runAllManagedModulesForAllRequests="true">
		<!-- modules... -->
		</modules>
	</system.webServer>
</confirugation>
{code:xml}

**Step 3: Tell VersionTagger which files to version:**

{code:xml}
<sharpNick>
	<versionTagger>
		<entries>
			<add url="/css/main.css"/>
			<add url="/js/all.js"/>
		</entries>
	</versionTagger>
</sharpNick>
{code:xml}

(see the [configuration reference](SharpNickConfiguration) on how to use the <sharpNick> node)

**Step 4: Set file access privileges**

Set read/write access to the Javascript/CSS files VersionTagger monitors. The default username used by IIS7 application pools is **Network Service**.

**Step 5: Done!**

Observe the changes in your output HTML source code:

Before
{code:html}
<html>
	<head>
		<link type="text/css" rel="stylesheet" href="/css/main.css" />
	</head>
	<body>
		Hi!
		<script type="text/javascript" src="/js/all.js"></script>
	</body>
</html>
{code:html}

After
{code:html}
<html>
	<head>
		<link type="text/css" rel="stylesheet" href="/css/main.1234abcd.css" />
	</head>
	<body>
		Hi!
		<script type="text/javascript" src="/js/all.1234abcd.js"></script>
	</body>
</html>
{code:html}

## Manual versioning

You can also version the files manually.

{code:xml}
<sharpNick>
	<versionTagger>
		<entries>
			<add url="/css/main.css" version="10" />
			<add url="/js/all.js" version-"1" />
		</entries>
	</versionTagger>
</sharpNick>
{code:xml}

This method has several advantages:

* When initializing, VersionTagger does not parse and compute the hash values for the files it monitors
* Once initialized, VersionTagger does not need to maintain background watcher instances to monitor for file changes
* You do not need to fiddle with file access privileges for the files VersionTagger tags

But there are disadvantages:

* It can be difficult to make sure that the version numbers are correct
* The application instance will be restarted every time you change the version numbers