using System;

namespace CKAN
{

    public class GUIUser : NullUser
    {
        public delegate bool DisplayYesNo(string message);

        public Action<string, object[]> displayMessage;
        public Action<string, object[]> displayError;
        public DisplayYesNo displayYesNo;

        protected override bool DisplayYesNoDialog(string message)
        {
            if (displayYesNo == null)
                return true;

            return displayYesNo(message);
        }

        protected override void DisplayMessage(string message, params object[] args)
        {
            displayMessage(message, args);
        }

        protected override void DisplayError(string message, params object[] args)
        {
            displayError(message, args);
        }

        protected override void ReportProgress(string format, int percent)
        {
            Main.Instance.SetDescription($"{format} - {percent}%");
            Main.Instance.SetProgress(percent);
        }
    }

}
