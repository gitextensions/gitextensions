using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;

using NetSpell.SpellChecker;

namespace NetSpell.SpellChecker.Controls
{
	/// <summary>
	/// Summary description for SpellTextBox.
	/// </summary>
	public class SpellTextBox : System.Windows.Forms.RichTextBox
	{
		
		private Spelling _SpellChecker;
		private bool _AutoSpellCheck = true;
		private UnderlineColor _MisspelledColor = UnderlineColor.Red;
		private UnderlineStyle _MisspelledStyle = UnderlineStyle.UnderlineWave;
		
		private int PreviousTextLength = 0;
		private int UncheckedWordIndex = -1;
		private int CurrentWordIndex = 0;

			
		/// <summary>
		///		Underline Colors
		/// </summary>
		public enum UnderlineColor
		{
			Default = 0x00,
			Blue	= 0x10,
			Aqua	= 0x20,
			Lime 	= 0x30,
			Fuchsia = 0x40,
			Red		= 0x50,
			Yellow	= 0x60,
			White	= 0x70,
			Navy	= 0x80,
			Teal	= 0x90,
			Green	= 0xa0,
			Purple	= 0xb0,
			Maroon	= 0xc0,
			Olive	= 0xd0,
			Gray	= 0xe0,
			Silver	= 0xf0
		}

		/// <summary>
		///		Underline Styles
		/// </summary>
		public enum UnderlineStyle
		{
			Underline					=1,
			UnderlineDouble				=3,
			UnderlineDotted				=4,
			UnderlineDash				=5,
			UnderlineDashDot			=6,
			UnderlineDashDotDot			=7,
			UnderlineWave				=8,
			UnderlineThick				=9,
			UnderlineHairline			=10,
			UnderlineDoubleWave			=11,
			UnderlineHeavyWave			=12,
			UnderlineLongDash			=13,
			UnderlineThickDash			=14,
			UnderlineThickDashDot		=15,
			UnderlineThickDashDotDot	=16,
			UnderlineThickDotted		=17,
			UnderlineThickLongDash		=18
		}

		/// <summary>
		/// Summary description for SpellTextBox.
		/// </summary>
		public SpellTextBox()
		{
		}

		private void SpellChecker_MisspelledWord(object sender, NetSpell.SpellChecker.SpellingEventArgs args)
		{
			TraceWriter.TraceVerbose("Misspelled Word:{0}", args.Word);
			
			int selectionStart = base.SelectionStart;
			int selectionLength = base.SelectionLength;

			base.Select(args.TextIndex, args.Word.Length);

			NativeMethods.CHARFORMAT2 cf = new NativeMethods.CHARFORMAT2();
			cf.cbSize = Marshal.SizeOf(cf);
			cf.dwMask = NativeMethods.CFM_UNDERLINETYPE;
			cf.bUnderlineType = (byte)this.MisspelledStyle; 
			cf.bUnderlineType |= (byte)this.MisspelledColor; 
			
			int result = NativeMethods.SendMessage(base.Handle, 
				NativeMethods.EM_SETCHARFORMAT, 
				NativeMethods.SCF_SELECTION | NativeMethods.SCF_WORD, 
				ref cf);

			base.Select(selectionStart, selectionLength);
		}

		private NativeMethods.CHARFORMAT2 GetCharFormat()
		{
			NativeMethods.CHARFORMAT2 cf = new NativeMethods.CHARFORMAT2();
			cf.cbSize = Marshal.SizeOf(cf);
			int result = NativeMethods.SendMessage(base.Handle, 
				NativeMethods.EM_GETCHARFORMAT, 
				NativeMethods.SCF_SELECTION, 
				ref cf);

			return cf; 
		} 

		/// <summary>
		///     The Color to use to mark misspelled words
		/// </summary>
		[Browsable(true)]
		[CategoryAttribute("Spell")]
		[Description("The Color to use to mark misspelled words")]
		[DefaultValue(UnderlineColor.Red)]
		public UnderlineColor MisspelledColor
		{
			get {return _MisspelledColor;}
			set {_MisspelledColor = value;}
		}

		/// <summary>
		///     The Underline style used to mark misspelled words
		/// </summary>
		[Browsable(true)]
		[CategoryAttribute("Spell")]
		[Description("The underline style used to mark misspelled words")]
		[DefaultValue(UnderlineStyle.UnderlineWave)]
		public UnderlineStyle MisspelledStyle
		{
			get {return _MisspelledStyle;}
			set {_MisspelledStyle = value;}
		}

		/// <summary>
		///     Gets or sets the selections background color
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Color SelectionBackColor
		{
			get
			{
				NativeMethods.CHARFORMAT2 cf = this.GetCharFormat();
				return ColorTranslator.FromOle(cf.crBackColor);
			}
			set
			{
				NativeMethods.CHARFORMAT2 cf = new NativeMethods.CHARFORMAT2();
				cf.cbSize = Marshal.SizeOf(cf);
				cf.dwMask = NativeMethods.CFM_BACKCOLOR;
				cf.crBackColor = ColorTranslator.ToWin32(value);
			
				int result = NativeMethods.SendMessage(base.Handle, 
					NativeMethods.EM_SETCHARFORMAT, 
					NativeMethods.SCF_SELECTION, 
					ref cf);
			}
		}

		/// <summary>
		///     Automatically mark misspelled words
		/// </summary>
		[Browsable(true)]
		[CategoryAttribute("Spell")]
		[Description("Automatically mark misspelled words")]
		[DefaultValue(true)]
		public bool AutoSpellCheck
		{
			get {return _AutoSpellCheck;}
			set {_AutoSpellCheck = value;}
		}

		/// <summary>
		///     The Spelling Object to use when checking words
		/// </summary>
		[Browsable(true)]
		[CategoryAttribute("Spell")]
		[Description("The Spelling Object to use when checking words")]
		public Spelling SpellChecker
		{
			get {return _SpellChecker;}
			set 
			{
				if(value != null)
				{
					_SpellChecker = value;
					_SpellChecker.MisspelledWord += new Spelling.MisspelledWordEventHandler(this.SpellChecker_MisspelledWord);
				}
			}
		}

	
		protected override void OnTextChanged(EventArgs e)
		{
			// get change size
			int changeSize = this.Text.Length - PreviousTextLength;
			PreviousTextLength = this.Text.Length;

			// sync spell checker text with text box text
			_SpellChecker.Text = base.Text;

			int currentPosition = base.SelectionStart;

			// get indexs
			int previousWordIndex = _SpellChecker.GetWordIndexFromTextIndex(currentPosition - changeSize);
			CurrentWordIndex = _SpellChecker.GetWordIndexFromTextIndex(currentPosition);

			// set current word to spell check
			_SpellChecker.WordIndex = previousWordIndex;
			
			// get the end index of previous word with out white space
			int wordEndIndex = _SpellChecker.TextIndex + _SpellChecker.CurrentWord.Length;

			TraceWriter.TraceVerbose("ChangeSize:{0}; PreviousWord:{1}; CurrentWord:{2}; Position:{3}; WordEnd:{4};", 
				changeSize, previousWordIndex, CurrentWordIndex, currentPosition, wordEndIndex);
				
			if (previousWordIndex != CurrentWordIndex || wordEndIndex < currentPosition)
			{
				// if word indexs not equal, spell check all words from previousWordIndex to CurrentWordIndex
				// or if word indexs equal, spell check if caret in white space
				this.MarkMisspelledWords(previousWordIndex, CurrentWordIndex);
				UncheckedWordIndex = -1;
			}
			else 
			{
				UncheckedWordIndex = previousWordIndex;
			}
		
			base.OnTextChanged (e);
		}
	
		protected override void OnSelectionChanged(EventArgs e)
		{
			CurrentWordIndex = _SpellChecker.GetWordIndexFromTextIndex(base.SelectionStart);
			if (UncheckedWordIndex != -1 && CurrentWordIndex != UncheckedWordIndex)
			{
				// if uncheck word index not equal current word index, spell check uncheck word
				this.MarkMisspelledWords(UncheckedWordIndex, UncheckedWordIndex);
				UncheckedWordIndex = -1;
			}		
			base.OnSelectionChanged (e);
		}
		
		private void MarkMisspelledWords()
		{
			this.MarkMisspelledWords(0, _SpellChecker.WordCount-1);
		}

		private void MarkMisspelledWords(int startWordIndex, int endWordIndex)
		{
			TraceWriter.TraceVerbose("Mark Misspelled Words {0} to {1}", startWordIndex, endWordIndex);
			
			//optimize by disabling event messages
			int eventMask = NativeMethods.SendMessage(this.Handle, 
				NativeMethods.EM_SETEVENTMASK, 0, 0);
			//optimize by disabling redraw
			NativeMethods.SendMessage(base.Handle, 
				NativeMethods.WM_SETREDRAW, 0, 0);

			//save selection	
			int	selectStart = base.SelectionStart;
			int selectLength = base.SelectionLength;

			//save show dialog value
			bool dialog = this.SpellChecker.ShowDialog;
			
			//disable show dialog to prevent dialogs on spell check
			this.SpellChecker.ShowDialog = false;

			//spell check all words in range
			while (this.SpellChecker.SpellCheck(startWordIndex, endWordIndex)) 
			{
				startWordIndex++;
			}
			//restore show dialog value
			this.SpellChecker.ShowDialog = dialog;

			//restore selection
			base.Select(selectStart, selectLength);
			
			//restore event messages
			eventMask = NativeMethods.SendMessage(this.Handle,
				NativeMethods.EM_SETEVENTMASK, 0, eventMask);
			//restore redraw
			NativeMethods.SendMessage(base.Handle, 
				NativeMethods.WM_SETREDRAW, 1, 0);
		}
	}
}
