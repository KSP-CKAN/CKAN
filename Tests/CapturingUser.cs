using System;
using System.Collections.Generic;

using CKAN;

namespace Tests
{
    public class CapturingUser : IUser
    {
        public CapturingUser(bool                        headless,
                             Func<string, bool>          yesNoAnswerer,
                             Func<string, object[], int> selectionDialogAnswerer)
        {
            Headless                     = headless;
            this.yesNoAnswerer           = yesNoAnswerer;
            this.selectionDialogAnswerer = selectionDialogAnswerer;
        }

        public bool Headless { get; private set; }

        public bool RaiseYesNoDialog(string question)
        {
            RaisedYesNoDialogQuestions.Add(question);
            return yesNoAnswerer(question);
        }

        public int RaiseSelectionDialog(string message, params object[] args)
        {
            RaisedSelectionDialogs.Add(new Tuple<string, object[]>(message, args));
            return selectionDialogAnswerer(message, args);
        }

        public void RaiseError(string message, params object[] args)
        {
            RaisedErrors.Add(string.Format(message, args));
        }

        public void RaiseProgress(string message, int percent)
        {
            RaisedProgresses.Add(new Tuple<string, int>(message, percent));
        }

        public void RaiseProgress(int percent, long bytesPerSecond, long bytesLeft)
        {
            RaisedProgresses.Add(new Tuple<string, int>($"{bytesPerSecond} {bytesLeft}",
                                                        percent));
        }

        public void RaiseMessage(string message, params object[] args)
        {
            RaisedMessages.Add(string.Format(message, args));
        }

        public readonly List<string>                  RaisedYesNoDialogQuestions = new List<string>();
        public readonly List<Tuple<string, object[]>> RaisedSelectionDialogs     = new List<Tuple<string, object[]>>();
        public readonly List<string>                  RaisedErrors               = new List<string>();
        public readonly List<Tuple<string, int>>      RaisedProgresses           = new List<Tuple<string, int>>();
        public readonly List<string>                  RaisedMessages             = new List<string>();


        private readonly Func<string, bool>          yesNoAnswerer;
        private readonly Func<string, object[], int> selectionDialogAnswerer;
    }
}
