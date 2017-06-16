# ScriptDeferFilter

Pushes lines of Javascript code to the bottom of the page to improve user-perceived performance. See [Yahoo's explanation](http://developer.yahoo.com/performance/rules.html#js_bottom) on this technique.

**Before**
{code:html}
<html>
	<body>
		<p>Paragraph 1</p>
		<script type="text/javascript">
			alert("Hi!");
		</script>
		<p>Paragraph 2</p>
		<script type="text/javascript" src="/script.js"></script>
		<p>Paragraph 3</p>
	</body>
</html>
{code:html}

**After**
{code:html}
<html>
	<body>
		<p>Paragraph 1</p>
		<p>Paragraph 2</p>
		<p>Paragraph 3</p>
		<script type="text/javascript">
			alert("Hi!");
		</script>
		<script type="text/javascript" src="/script.js"></script>
	</body>
</html>
{code:html}

## How to use

Enter the line below in the Global.asax's BeginRequest event:

{code:c#}
if (Request.Url.LocalPath.EndsWith(".aspx")) Response.Filter = new ScriptDeferFilter(Response);
{code:c#}

## Credits

Code is based on Omar Al Zabir's [ScriptDeferFilter](http://omaralzabir.com/fast_page_loading_by_moving_asp_net_ajax_scripts_after_visible_content/) used in [PageFlakes](http://www.pageflakes.com/).