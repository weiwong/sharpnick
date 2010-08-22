using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SharpNick
{
	/// <summary>
	/// Convenience methods to deal with WebControls.
	/// </summary>
	public sealed class ControlHelper
	{
		private ControlHelper() {}
		/// <summary>
		/// Finds a control with a specified ID within a control.
		/// </summary>
		/// <typeparam name="T">Type of the control to find.</typeparam>
		/// <param name="control">The container control to find within.</param>
		/// <param name="id">ID of the control to find.</param>
		/// <returns>A control.</returns>
		public static T FindControl<T>(Control control, string id) where T : Control
		{
			if (control == null) return null;

			foreach (Control childControl in control.Controls)
			{
				T candidate = childControl as T;

				if (candidate == null || candidate.ID != id)
				{
					candidate = FindControl<T>(childControl, id);
				}

				if (candidate != null) return candidate;
			}

			return null;
		}
		/// <summary>
		/// Finds the first instance of a control with a specified type within
		/// a specified control.
		/// </summary>
		/// <typeparam name="T">Type of control to find.</typeparam>
		/// <param name="control">The container control to find within.</param>
		/// <returns>A control of type T.</returns>
		public static T FindControl<T>(Control control) where T : Control
		{
			if (control == null) return null;

			foreach (Control childControl in control.Controls)
			{
				T candidate = childControl as T;

				if (candidate == null) candidate = FindControl<T>(childControl);
				else return candidate;
			}

			return null;
		}
		/// <summary>
		/// Insert number options into a drop down list.
		/// </summary>
		/// <param name="list">The control to populate with.</param>
		/// <param name="start">The number to begin with.</param>
		/// <param name="end">The number to stop at.</param>
		public static void InsertNumberOptions(ListControl list, int start, int end)
		{
			InsertNumberOptions(list, start, end, "#");
		}
		/// <summary>
		/// Insert number options into a drop down list using a specified format.
		/// </summary>
		/// <param name="list">The control to populate the numbers with.</param>
		/// <param name="start">The number to begin with.</param>
		/// <param name="end">The number to stop at.</param>
		/// <param name="format">The format to enumerate the numbers in.</param>
		public static void InsertNumberOptions(ListControl list, int start, int end, string format)
		{
			list.Items.Clear();
			for (int c = start; c <= end; c++)
				list.Items.Add(c.ToString(format));
		}
		/// <summary>
		/// Insert month options (in full name) into a drop down list.
		/// </summary>
		/// <param name="list"></param>
		public static void InsertMonthOptions(ListControl list)
		{
			list.Items.Clear();

			for (int i = 1; i <= 12; ++i)
			{
				list.Items.Add(new ListItem(DateTime.MinValue.AddMonths(i - 1).ToString("MMMM"), i.ToString()));
			}
		}
		/// <summary>
		/// Gets rid of extra spaces within the contents of a list of specified
		/// text boxes.
		/// </summary>
		/// <param name="textBoxes">The textboxes to clean contents of.</param>
		public static void CleanTextBoxes(TextBox[] textBoxes)
		{
			for (var i = 0; i < textBoxes.Length; ++i)
			{
				var textbox = textBoxes[i];
				textbox.Text = textbox.Text == null ? null : textbox.Text.Trim();
			}
		}
	}
}
