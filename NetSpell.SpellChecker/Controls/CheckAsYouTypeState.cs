using System;
using System.Windows.Forms;

namespace NetSpell.SpellChecker.Controls
{
	/// <summary>
	/// Summary description for CheckAsYouTypeState.
	/// </summary>
	public class CheckAsYouTypeState
	{
		private TextBoxBase _textBoxBaseInstance;

		public CheckAsYouTypeState(TextBoxBase textBoxBaseInstance)
		{
			_textBoxBaseInstance = textBoxBaseInstance;
		}

		public TextBoxBase TextBoxBaseInstance
		{
			get { return _textBoxBaseInstance; }
			set { _textBoxBaseInstance = value; }
		}


	}
}
