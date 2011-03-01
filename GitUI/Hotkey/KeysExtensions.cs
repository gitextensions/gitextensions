using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.Hotkey
{
  public static class KeysExtensions
  {
    /// <summary>
    /// Strips the modifier from KeyData
    /// </summary>
    /// <param name="keyData"></param>
    /// <returns></returns>
    public static Keys GetKeyCode(this Keys keyData)
    {
      return keyData & Keys.KeyCode;
    }

    public static bool IsModifierKey(this Keys key)
    {
      return key == Keys.ShiftKey ||
             key == Keys.ControlKey ||
             key == Keys.Alt;
    }

    public static Keys[] GetModifiers(this Keys key)
    {
      // Retrieve the modifiers, mask away the rest
      Keys modifier = key & Keys.Modifiers;

      List<Keys> modifierList = new List<Keys>();
      Action<Keys> addIfContains = m => { if (m == (m & modifier))  modifierList.Add(m); };

      addIfContains(Keys.Control);
      addIfContains(Keys.Shift);
      addIfContains(Keys.Alt);

      return modifierList.ToArray();      
    }

    public static string ToText(this Keys key)
    {
      return string.Join("+", 
        new [] { key.GetKeyCode() }
        .Union(key.GetModifiers())
        .Select(k => k.ToString())
        .ToArray());
    }
  }
}
