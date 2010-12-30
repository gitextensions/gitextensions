using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

using NetSpell.SpellChecker;

namespace NetSpell.SpellChecker.Controls
{
	/// <summary>
	/// Summary description for CheckAsYouType.
	/// </summary>
	[ProvideProperty("EnableCheck", typeof(TextBoxBase))]
	[ProvideProperty("SpellChecker", typeof(TextBoxBase))]
	public class CheckAsYouType : Component, ISupportInitialize, IExtenderProvider, IMessageFilter
	{

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		
		private Hashtable textBoxes = new Hashtable();
		private Spelling _spellChecker;

		public CheckAsYouType(System.ComponentModel.IContainer container)
		{
			/// Required for Windows.Forms Class Composition Designer support
			container.Add(this);
			InitializeComponent();
			Initialize();
		}

		public CheckAsYouType()
		{
			/// Required for Windows.Forms Class Composition Designer support
			InitializeComponent();
			Initialize();
		}

		private void Initialize()
		{
			Application.AddMessageFilter(this);
			Application.Idle +=new EventHandler(OnIdle);
		}

		#region Extended Properties

		[
		Category("SpellChecker"),
		Description("Spell Check this Control As You Type"),
		]
		public bool GetEnableCheck(TextBoxBase extendee)
		{
			return this.textBoxes.Contains(extendee.Handle);
		}

		public void SetEnableCheck(TextBoxBase extendee, bool value)
		{
			if(value && !this.textBoxes.Contains(extendee.Handle))
			{
				this.textBoxes.Add(extendee.Handle, new CheckAsYouTypeState(extendee));
				extendee.TextChanged += new EventHandler(OnTextChanged);
				extendee.MouseDown += new MouseEventHandler(OnMouseDown);					
			}
			else if(!value && this.textBoxes.Contains(extendee.Handle))
			{
				this.textBoxes.Remove(extendee);
				extendee.TextChanged -= new EventHandler(OnTextChanged);
				extendee.MouseDown -= new MouseEventHandler(OnMouseDown);
			}
		}

		[
		Category("SpellChecker"),
		Description("Spell Check instance to use"),
		]
		public Spelling GetSpellChecker(TextBoxBase extendee)
		{
			return _spellChecker;
		}

		public void SetSpellChecker(TextBoxBase extendee, Spelling value)
		{
			_spellChecker = value;
		}

		#endregion //Extended Properties
		


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	
		#region IExtenderProvider Members

		public bool CanExtend(object extendee)
		{
			if (extendee is TextBoxBase)
				return true; 

			return false;
		}

		#endregion

		#region IMessageFilter Members

		 

		public bool PreFilterMessage(ref Message m)
		{
			// only listen to extended controls
			if(this.textBoxes.Contains(m.HWnd))
			{
				TextBoxBase textBox = ((CheckAsYouTypeState)this.textBoxes[m.HWnd]).TextBoxBaseInstance;
				
				if(m.Msg == NativeMethods.WM_PAINT)
					OnPaint(textBox, null);
				else if(m.Msg == NativeMethods.WM_ERASEBKGND)
					Console.WriteLine("WM_ERASEBKGND:{0}", textBox.Name);
				else if(m.Msg == NativeMethods.WM_HSCROLL)
					Console.WriteLine("WM_HSCROLL:{0}", textBox.Name);
				else if(m.Msg == NativeMethods.WM_VSCROLL)
					Console.WriteLine("WM_VSCROLL:{0}", textBox.Name);
				else if(m.Msg == NativeMethods.WM_CAPTURECHANGED)
					Console.WriteLine("WM_CAPTURECHANGED:{0}", textBox.Name);
				//else
					//Console.WriteLine("Msg: 0x{0}", m.Msg.ToString("x4"));
				
				
			}

			// never handle messages
			return false;
		}

		#endregion
	
		#region ISupportInitialize Members

		public void BeginInit()
		{
			// TODO:  Add TextBoxExtender.BeginInit implementation
		}

		public void EndInit()
		{
			// TODO:  Add TextBoxExtender.EndInit implementation
		}

		#endregion

		private void OnTextChanged(object sender, EventArgs e)
		{
			if(sender is TextBoxBase)
			{
				TextBoxBase textBox = (TextBoxBase)sender;
				Console.WriteLine("OnTextChanged:{0}", textBox.Name);
			}
		}

		private void OnPaint(object sender, PaintEventArgs e)
		{
			if(sender is TextBoxBase)
			{
				TextBoxBase textBox = (TextBoxBase)sender;
				Console.WriteLine("OnPaint:{0}", textBox.Name);
			}
		}

		private void OnMouseDown(object sender, MouseEventArgs e)
		{
			if(sender is TextBoxBase)
			{
				TextBoxBase textBox = (TextBoxBase)sender;
				Console.WriteLine("OnMouseDown:{0}", textBox.Name);
			}
		}

		private void OnIdle(object sender, EventArgs e)
		{

		}
	}
}
