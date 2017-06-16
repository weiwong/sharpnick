# ControlHelper

## CleanTextBoxes(TextBox[]() textBoxes)

Trims the strings within the array of text boxes specified.

**Code in front**
{code:aspx c#}
<asp:TextBox ID="TextBox1" runat="server" Text=" text " />
<asp:TextBox ID="TextBox2" runat="server" Text="  more text   " />
{code:aspx c#}

**Code behind**
{code:c#}
var textboxes = new TextBox[]() { TextBox1, TextBox2 };
ControlHelper.CleanTextBoxes(textboxes);

Console.WriteLine(TextBox1.Text); // "text"
Console.WriteLine(TextBox2.Text); // "another text"
{code:c#}

## FindControl<T>(Control control, string id)

Finds a control of a specified type and specified ID within a control.

**Code in front**
{code:aspx c#}
<asp:Panel ID="BigPanel" runat="server">
	<asp:TextBox ID="TextBox1" runat="server" Text="text" />
</asp:Panel>
{code:aspx c#}

**Code behind**
{code:c#}
var find1 = ControlHelper.FindControl<TextBox>(BigPanel, "TextBox1");
var find2 = ControlHelper.FindControl<Control>(BigPanel, "TextBox1");
var find3 = ControlHelper.FindControl<Control>(BigPanel, "TextBox2");

Console.WriteLine(find1.Text); // "text"
Console.WriteLine(find2.Text); // compilation error - find2 needs to be cast as TextBox to access Text property
Console.WriteLine(find3 == null); // true
{code:c#}

## FindControl<T>(Control control)

Finds the first control of a specified type within a control.

**Code in front**
{code:aspx c#}
<asp:Panel ID="BigPanel" runat="server">
	<asp:TextBox ID="TextBox1" runat="server" Text="text" />
	<asp:Label ID="Label1" runat="server" Text="text" />
	<asp:Label ID="Label2" runat="server" Text="text" />
</asp:Panel>
{code:aspx c#}

**Code behind**
{code:c#}
var find1 = ControlHelper.FindControl<Label>(BigPanel);
var find2 = ControlHelper.FindControl<DropDownList>(BigPanel);

Console.WriteLine(find1.ID); // "Label1"
Console.WriteLine(find2 == null); // true
{code:c#}

## InsertMonthOptions(ListControl list)

Inserts a list of months, in full names, intro a list.

{code:c#}
var list = new DropDownList();
ControlHelper.InsertMonthOptions(list);

list.SelectedIndex = 2;
Console.WriteLine(list.SelectedValue); // 3
Console.WriteLine(list.SelectedText); // March
{code:c#}

## InsertNumberOptions(ListControl list, int start, int end, string format)

Inserts a list of numbers into a list control, from _start_ to _end_, in a format specified by _format_. See Numeric Format Strings on [MSDN](http://msdn.microsoft.com/en-us/library/427bttx3(v=VS.85).aspx).

There's a overload negating the _format_ parameter that assumes a _format_ of "#".

{code:c#}
var list1 = new DropDownList();
var list2 = new DropDownList();
ControlHelper.InsertNumberOptions(list1, 2000, 2010);
ControlHelper.InsertNumberOptions(list2, 1, 12);

list1.SelectedIndex = 4;
list2.SelectedIndex = 4;

Console.WriteLine(list1.SelectedValue); // 2004
Console.WriteLine(list2.SelectedValue); // 05
Console.WriteLine(list2.SelectedText); // 05
{code:c#}