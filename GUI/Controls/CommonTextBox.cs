using System;
using System.Windows.Forms;

namespace CKAN
{
    public class CommonTextBox : TextBox
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Back | Keys.Control))
            {
                if (SelectionStart > 0)
                {
                    int i = SelectionStart - 2;
                    for (; i > 0 && char.IsWhiteSpace(Text, i); i--);
                    for (; i > 0; i--)
                        if (char.IsPunctuation(Text, i) || char.IsSeparator(Text, i) || char.IsWhiteSpace(Text, i))
                        {
                            i++;
                            break;
                        }
                    this.Text = Text.Remove(i, SelectionStart - i);
                    this.SelectionStart = i;
                }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
