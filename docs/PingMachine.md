# PingMachine

Periodically pings a list of URLs to keep websites alive.

## How to use

**Step 1: Configure PingMachine**

{code:xml}
<sharpNick>
	<pingMachine>
		<urls>
			<add url="http://www.sharpnick.com/"/>
		</urls>
	</pingMachine>
</sharpNick>
{code:xml}

(see [configuration reference](SharpNickConfiguration) to use the <sharpNick> node)

**Step 2: Start the machine**

Global.asax
{code:c#}
void Application_Start(object sender, EventArgs e) 
{
	Pinger.Start();
}
{code:c#}

**Step 3: Done!**

## For websites protected by 401 challenge

If the domain requires a username and password, enter them in the <credentials> code under <pingMachine>:

{code:xml}
<sharpNick>
	<pingMachine>
		<credentials>
			<add url="http://www.sharpnick.com/"
				username="username" password="password" />
		</credentials>
	</pingMachine>
</sharpNick>
{code:xml}

## Increase frequency of ping

PingMachine pings every URL every 4 minutes by default, which is sufficient to keep ASP.NET websites with default settings alive. If you want to change the ping interval, do so in the <pingMachine> node.

To set ping interval to 20 seconds:

{code:xml}
<sharpNick>
	<pingMachine pingInterval="20">
		<!-- Other lines of configuration -->
	</pingMachine>
</sharpNick>
{code:xml}