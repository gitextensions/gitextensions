// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedConfiguration.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Core.Util
{
	internal class ProtectedConfiguration
	{
		/* Below is used to load custom app.config files:
		 * System.Configuration.ConfigurationManager.OpenExeConfiguration(string exePath)
		 */

		/*internal static void ProtectConfiguration()
		{
			// Get the application configuration file.
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			// Get the section to protect.
			ConfigurationSection connStrings = config.ConnectionStrings;

			if (connStrings != null)
			{
				if (!connStrings.SectionInformation.IsProtected)
				{
					if (!connStrings.ElementInformation.IsLocked)
					{
						// Protect the section.
						connStrings.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");

						connStrings.SectionInformation.ForceSave = true;
						config.Save(ConfigurationSaveMode.Full);

						Logger.Info(String.Format("Section {0} is now protected by {1}", connStrings.SectionInformation.Name, connStrings.SectionInformation.ProtectionProvider.Name));
					}
					else
					{
						Logger.Warning(String.Format("Can't protect, section {0} is locked", connStrings.SectionInformation.Name));
					}
				}
				else
				{
					Logger.Info(String.Format("Section {0} is already protected by {1}", connStrings.SectionInformation.Name, connStrings.SectionInformation.ProtectionProvider.Name));
				}
			}
			else
			{
				Logger.Error(String.Format("Can't get the section {0}", connStrings.SectionInformation.Name));
			}
		}

		/// <summary>
		/// Restores the unprotected state of the configuration file, connectionStrings section.
		/// </summary>
		/// <remarks>There is no need to manually decrypt </remarks>
		internal static void UnProtectConfiguration()
		{
			// Get the application configuration file.
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			// Get the section to unprotect.
			ConfigurationSection connStrings = config.ConnectionStrings;

			if (connStrings != null)
			{
				if (connStrings.SectionInformation.IsProtected)
				{
					if (!connStrings.ElementInformation.IsLocked)
					{
						// Unprotect the section.
						connStrings.SectionInformation.UnprotectSection();

						connStrings.SectionInformation.ForceSave = true;
						config.Save(ConfigurationSaveMode.Full);

						Logger.Info(String.Format("Section {0} is now unprotected.", connStrings.SectionInformation.Name));
					}
					else
					{
						Logger.Warning(String.Format("Can't unprotect, section {0} is locked", connStrings.SectionInformation.Name));
					}
				}
				else
				{
					Logger.Info(String.Format("Section {0} is already unprotected.", connStrings.SectionInformation.Name));
				}
			}
			else
			{
				Logger.Error(String.Format("Can't get the section {0}", connStrings.SectionInformation.Name));
			}
		}*/
	}
}