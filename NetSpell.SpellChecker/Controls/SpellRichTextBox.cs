using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace NetSpell.SpellChecker.Controls
{
	/// <summary>
	/// Summary description for SpellRichTextBox.
	/// </summary>
	public class SpellRichTextBox : RichTextBox
	{
		private bool _EnableSpellCheck = true;
		private Pen _errorPen = new Pen(Color.Red, 1);
		private Spelling _spellChecker;

        public SpellRichTextBox()
        {
            Application.Idle +=new EventHandler(OnIdle);
        }
	
		
        protected override void WndProc(ref Message m)
        {
            base.WndProc (ref m);
        }
    
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged (e);
        }
    
        protected override void OnSelectionChanged(EventArgs e)
        {
            base.OnSelectionChanged (e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
        }

		protected override void Dispose(bool disposing)
		{
			if( disposing )
			{
				// free pen resources
				if(_errorPen != null)
					_errorPen.Dispose();
			}
			base.Dispose( disposing );
		}

		protected void OnScroll(EventArgs e)
		{

		}

        protected void OnIdle(object sender, EventArgs e)
        {

        }

		/// <summary>
		///     The WordDictionary object to use when spell checking
		/// </summary>
		[Browsable(true)]
		[CategoryAttribute("SpellCheck")]
		[Description("The Spelling object to use when spell checking")]
		public Spelling SpellChecker
		{
			get {return _spellChecker;}
			set {_spellChecker = value;}
		}


		/// <summary>
		///     The color of the wavy underline that marks misspelled words
		/// </summary>
		[Browsable(true)]
		[CategoryAttribute("SpellCheck")]
		[Description("The color of the wavy underline that marks misspelled words")]
		public Color ErrorColor
		{
			get {return _errorPen.Color;}
			set {_errorPen.Color = value;}
		}


		/// <summary>
		///     Enable As You Type spell checking
		/// </summary>
		[Browsable(true)]
		[DefaultValue(true)]
		[CategoryAttribute("SpellCheck")]
		[Description("Enable As You Type spell checking")]
		public bool EnableSpellCheck
		{
			get {return _EnableSpellCheck;}
			set {_EnableSpellCheck = value;}
		}

	}
}
