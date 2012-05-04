// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicResources.cs" company="NBusy Project">
//   Copyright (c) 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util
{
	using NBug.Properties;

	public class PublicResources
	{
		private string ui_Console_Full_Message;
		public string UI_Console_Full_Message
		{
			get
			{
				return this.ui_Console_Full_Message ?? Localization.UI_Console_Full_Message;
			}

			set
			{
				this.ui_Console_Full_Message = value;
			}
		}

		private string ui_Console_Minimal_Message;
		public string UI_Console_Minimal_Message
		{
			get
			{
				return this.ui_Console_Minimal_Message ?? Localization.UI_Console_Minimal_Message;
			}

			set
			{
				this.ui_Console_Minimal_Message = value;
			}
		}

		private string ui_Console_Normal_Message;
		public string UI_Console_Normal_Message
		{
			get
			{
				return this.ui_Console_Normal_Message ?? Localization.UI_Console_Normal_Message;
			}

			set
			{
				this.ui_Console_Normal_Message = value;
			}
		}
		
		private string ui_Dialog_Minimal_Message;
		public string UI_Dialog_Minimal_Message
		{
			get
			{
				return this.ui_Dialog_Minimal_Message ?? Localization.UI_Dialog_Minimal_Message;
			}

			set
			{
				this.ui_Dialog_Minimal_Message = value;
			}
		}

		private string ui_Dialog_Normal_Message;
		public string UI_Dialog_Normal_Message
		{
			get
			{
				return this.ui_Dialog_Normal_Message ?? Localization.UI_Dialog_Normal_Message;
			}

			set
			{
				this.ui_Dialog_Normal_Message = value;
			}
		}

		private string ui_Dialog_Normal_Title;
		public string UI_Dialog_Normal_Title
		{
			get
			{
				return this.ui_Dialog_Normal_Title ?? Localization.UI_Dialog_Normal_Title;
			}

			set
			{
				this.ui_Dialog_Normal_Title = value;
			}
		}

		private string ui_Dialog_Normal_Continue_Button;
		public string UI_Dialog_Normal_Continue_Button
		{
			get
			{
				return this.ui_Dialog_Normal_Continue_Button ?? Localization.UI_Dialog_Normal_Continue_Button;
			}

			set
			{
				this.ui_Dialog_Normal_Continue_Button = value;
			}
		}

		private string ui_Dialog_Normal_Quit_Button;
		public string UI_Dialog_Normal_Quit_Button
		{
			get
			{
				return this.ui_Dialog_Normal_Quit_Button ?? Localization.UI_Dialog_Normal_Quit_Button;
			}

			set
			{
				this.ui_Dialog_Normal_Quit_Button = value;
			}
		}

		private string ui_Dialog_Full_Message;
		public string UI_Dialog_Full_Message
		{
			get
			{
				return this.ui_Dialog_Full_Message ?? Localization.UI_Dialog_Full_Message;
			}

			set
			{
				this.ui_Dialog_Full_Message = value;
			}
		}

		private string ui_Dialog_Full_Title;
		public string UI_Dialog_Full_Title
		{
			get
			{
				return this.ui_Dialog_Full_Title ?? Localization.UI_Dialog_Full_Title;
			}

			set
			{
				this.ui_Dialog_Full_Title = value;
			}
		}

		private string ui_Dialog_Full_General_Tab;
		public string UI_Dialog_Full_General_Tab
		{
			get
			{
				return this.ui_Dialog_Full_General_Tab ?? Localization.UI_Dialog_Full_General_Tab;
			}

			set
			{
				this.ui_Dialog_Full_General_Tab = value;
			}
		}

		private string ui_Dialog_Full_Exception_Tab;
		public string UI_Dialog_Full_Exception_Tab
		{
			get
			{
				return this.ui_Dialog_Full_Exception_Tab ?? Localization.UI_Dialog_Full_Exception_Tab;
			}

			set
			{
				this.ui_Dialog_Full_Exception_Tab = value;
			}
		}

		private string ui_Dialog_Full_Report_Contents_Tab;
		public string UI_Dialog_Full_Report_Contents_Tab
		{
			get
			{
				return this.ui_Dialog_Full_Report_Contents_Tab ?? Localization.UI_Dialog_Full_Report_Contents_Tab;
			}

			set
			{
				this.ui_Dialog_Full_Report_Contents_Tab = value;
			}
		}

		private string ui_Dialog_Full_How_to_Reproduce_the_Error_Notification;
		public string UI_Dialog_Full_How_to_Reproduce_the_Error_Notification
		{
			get
			{
				return this.ui_Dialog_Full_How_to_Reproduce_the_Error_Notification ?? Localization.UI_Dialog_Full_How_to_Reproduce_the_Error_Notification;
			}

			set
			{
				this.ui_Dialog_Full_How_to_Reproduce_the_Error_Notification = value;
			}
		}

		private string ui_Dialog_Full_Quit_Button;
		public string UI_Dialog_Full_Quit_Button
		{
			get
			{
				return this.ui_Dialog_Full_Quit_Button ?? Localization.UI_Dialog_Full_Quit_Button;
			}

			set
			{
				this.ui_Dialog_Full_Quit_Button = value;
			}
		}

		private string ui_Dialog_Full_Send_and_Quit_Button;
		public string UI_Dialog_Full_Send_and_Quit_Button
		{
			get
			{
				return this.ui_Dialog_Full_Send_and_Quit_Button ?? Localization.UI_Dialog_Full_Send_and_Quit_Button;
			}

			set
			{
				this.ui_Dialog_Full_Send_and_Quit_Button = value;
			}
		}
	}
}
