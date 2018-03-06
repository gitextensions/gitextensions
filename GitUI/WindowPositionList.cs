using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GitUI
{
    /// <summary>
    ///   Stores the state and position of a single window
    /// </summary>
    [DebuggerDisplay("Name={Name} Rect={Rect} DeviceDpi={DeviceDpi} State={State}")]
    [Serializable]
    public class WindowPosition
    {
        protected WindowPosition()
        {
            DeviceDpi = 96;
        }

        public WindowPosition(Rectangle rect, int deviceDpi, FormWindowState state, string name)
        {
            Rect = rect;
            DeviceDpi = deviceDpi;
            State = state;
            Name = name;
        }

        public Rectangle Rect { get; set; }
        [DefaultValue(96)]
        public int DeviceDpi { get; set; }
        public FormWindowState State { get; set; }
        public string Name { get; set; }
    }

    [Serializable]
    public class WindowPositionList
    {
        public List<WindowPosition> WindowPositions { get; set; }

        private static readonly string AppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitExtensions");

        private static readonly string ConfigFilePath = Path.Combine(AppDataDir, "WindowPositions.xml");

        static WindowPositionList()
        {
            if (!Directory.Exists(AppDataDir))
            {
                Directory.CreateDirectory(AppDataDir);
            }
        }

        protected WindowPositionList()
        {
            WindowPositions = new List<WindowPosition>();
        }

        public WindowPosition Get(string name)
        {
            return WindowPositions.FirstOrDefault(r => r.Name == name);
        }

        public void AddOrUpdate(WindowPosition pos)
        {
            WindowPositions.RemoveAll(r => r.Name == pos.Name);
            WindowPositions.Add(pos);
        }

        public static WindowPositionList Load()
        {
            if (!File.Exists(ConfigFilePath))
            {
                return new WindowPositionList();
            }

            try
            {
                using (
                    var stream = File.Open(ConfigFilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    return new XmlSerializer(typeof(WindowPositionList)).Deserialize(stream) as WindowPositionList;
                }
            }
            catch
            {
                return new WindowPositionList();
            }
        }

        public void Save()
        {
            using (var stream = File.Open(ConfigFilePath, FileMode.Create, FileAccess.Write))
            {
                new XmlSerializer(typeof(WindowPositionList)).Serialize(stream, this);
            }
        }
    }
}