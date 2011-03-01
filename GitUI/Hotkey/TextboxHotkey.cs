using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.Hotkey
{
  public class TextboxHotkey : TextBox
  {
    #region Key
    private Keys _KeyData;
    /// <summary>Gets or sets the KeyData</summary>
    public Keys KeyData
    {
      get { return _KeyData; }
      set 
      { 
        _KeyData = value;
        this.Text = value.ToText();
      }
    }
    #endregion

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
      // We don't want only a modifier key pressed
      // TODO Further restrict the allowed keys
      if(!keyData.GetKeyCode().IsModifierKey())
        this.KeyData = keyData;
  
      // Swallow all keys
      return true;
    }

  }
}
