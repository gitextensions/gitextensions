using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Xml;


public class PortableSettingsProvider : SettingsProvider
{

    //XML Root Node
    const string SETTINGSROOT = "Settings";

    public override string Name
    {
        get
        {
            
            return "PortableSettingsProvider";
        }
    }
    public override string ApplicationName
    {
        get
        {
           
            if (Application.ProductName.Trim().Length > 0)
            {
                return Application.ProductName;
            }
            else
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(Application.ExecutablePath);
                return fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
            }
        }
        //Do nothing
        set { }
    }

    public virtual string GetAppSettingsPath()
    {
        //Used to determine where to store the settings
        System.IO.FileInfo fi = new System.IO.FileInfo(Application.ExecutablePath);
        return fi.DirectoryName;
    }

    public virtual string GetAppSettingsFilename()
    {
        //Used to determine the filename to store the settings
        return ApplicationName + ".settings";
    }

    public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection propvals)
    {
        //Iterate through the settings to be stored
        //Only dirty settings are included in propvals, and only ones relevant to this provider
        foreach (SettingsPropertyValue propval in propvals)
        {
            SetValue(propval);
        }

        try
        {
            SettingsXML.Save(System.IO.Path.Combine(GetAppSettingsPath(), GetAppSettingsFilename()));
        }
        catch (Exception ex)
        {
            //Ignore if cant save, device been ejected
        }
    }

    public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection props)
    {
        //Create new collection of values
        SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

        //Iterate through the settings to be retrieved

        foreach (SettingsProperty setting in props)
        {
            SettingsPropertyValue value = new SettingsPropertyValue(setting);
            value.IsDirty = false;
            value.SerializedValue = GetValue(setting);
            values.Add(value);
        }
        return values;
    }


    private System.Xml.XmlDocument m_SettingsXML = null;
    private XmlDocument SettingsXML
    {
        get
        {
            //If we dont hold an xml document, try opening one.  
            //If it doesnt exist then create a new one ready.
            if (m_SettingsXML == null)
            {
                m_SettingsXML = new System.Xml.XmlDocument();

                try
                {
                    m_SettingsXML.Load(System.IO.Path.Combine(GetAppSettingsPath(), GetAppSettingsFilename()));
                }
                catch (Exception ex)
                {
                    //Create new document
                    XmlDeclaration dec = m_SettingsXML.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                    m_SettingsXML.AppendChild(dec);

                    XmlNode nodeRoot = null;

                    nodeRoot = m_SettingsXML.CreateNode(XmlNodeType.Element, SETTINGSROOT, "");
                    m_SettingsXML.AppendChild(nodeRoot);
                }
            }

            return m_SettingsXML;
        }
    }

    private string GetValue(SettingsProperty setting)
    {
        string ret = "";

        try
        {
            if (IsRoaming(setting))
            {
                ret = SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + setting.Name).InnerText;
            }
            else
            {
                ret = SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + System.Environment.MachineName + "/" + setting.Name).InnerText;
            }

        }
        catch (Exception ex)
        {
            if ((setting.DefaultValue != null))
            {
                ret = setting.DefaultValue.ToString();
            }
            else
            {
                ret = "";
            }
        }

        return ret;
    }


    private void SetValue(SettingsPropertyValue propVal)
    {
        System.Xml.XmlElement MachineNode = null;
        System.Xml.XmlElement SettingNode = null;

        //Determine if the setting is roaming.
        //If roaming then the value is stored as an element under the root
        //Otherwise it is stored under a machine name node 
        try
        {
            if (IsRoaming(propVal.Property))
            {
                SettingNode = (XmlElement)SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + propVal.Name);
            }
            else
            {
                SettingNode = (XmlElement)SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + System.Environment.MachineName + "/" + propVal.Name);
            }
        }
        catch (Exception ex)
        {
            SettingNode = null;
        }

        //Check to see if the node exists, if so then set its new value
        if ((SettingNode != null))
        {
            SettingNode.InnerText = (propVal.SerializedValue != null) ? propVal.SerializedValue.ToString() : string.Empty;
        }
        else
        {
            if (IsRoaming(propVal.Property))
            {
                //Store the value as an element of the Settings Root Node
                SettingNode = SettingsXML.CreateElement(propVal.Name);
                SettingNode.InnerText = (propVal.SerializedValue != null) ? propVal.SerializedValue.ToString() : string.Empty;
                SettingsXML.SelectSingleNode(SETTINGSROOT).AppendChild(SettingNode);
            }
            else
            {
                //Its machine specific, store as an element of the machine name node,
                //creating a new machine name node if one doesnt exist.
                try
                {
                    MachineNode = (XmlElement)SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + System.Environment.MachineName);
                }
                catch (Exception ex)
                {
                    MachineNode = SettingsXML.CreateElement(System.Environment.MachineName);
                    SettingsXML.SelectSingleNode(SETTINGSROOT).AppendChild(MachineNode);
                }

                if (MachineNode == null)
                {
                    MachineNode = SettingsXML.CreateElement(System.Environment.MachineName);
                    SettingsXML.SelectSingleNode(SETTINGSROOT).AppendChild(MachineNode);
                }

                SettingNode = SettingsXML.CreateElement(propVal.Name);
                SettingNode.InnerText = (propVal.SerializedValue != null) ? propVal.SerializedValue.ToString() : string.Empty;
                MachineNode.AppendChild(SettingNode);
            }
        }
    }

    private bool IsRoaming(SettingsProperty prop)
    {
        //Determine if the setting is marked as Roaming
        foreach (DictionaryEntry d in prop.Attributes)
        {
            Attribute a = (Attribute)d.Value;
            if (a is System.Configuration.SettingsManageabilityAttribute)
            {
                return true;
            }
        }
        return false;
    }

}
