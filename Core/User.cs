// Communicate with the user (status messages, yes/no questions, etc)
// This class will proxy to either the GUI or cmdline functionality.

using System;

namespace CKAN
{
    public delegate int DisplaySelectionDialog(string message, params object[] args);

    public delegate void DisplayMessage(string message, params object[] args);

    public delegate bool DisplayYesNoDialog(string message);

    public delegate void DisplayError(string message, params object[] args);

    public delegate void ReportProgress(string format, int percent);

    public delegate void DownloadsComplete(Uri[] urls, string[] filenames, Exception[] errors);

    public interface IUser
    {
        event DisplayYesNoDialog AskUser;

        event DisplaySelectionDialog AskUserForSelection;

        event DisplayMessage Message;

        event DisplayError Error;

        event ReportProgress Progress;

        event DownloadsComplete DownloadsComplete;

        int WindowWidth { get; }
        bool Headless { get; }

        int RaiseSelectionDialog(string message, params object[] args);

        void RaiseMessage(string message, params object[] url);

        void RaiseProgress(string message, int percent);

        bool RaiseYesNoDialog(string question);

        void RaiseError(string message, params object[] args);

        void RaiseDownloadsCompleted(Uri[] file_urls, string[] file_paths, Exception[] errors);
    }

    //Can be used in tests to supress output or as a base class for other types of user.
    //It supplies no opp event handlers so that subclasses can avoid null checks.
    public class NullUser : IUser
    {
        public static readonly IUser User = new NullUser();

        public NullUser()
        {
            AskUser += DisplayYesNoDialog;
            AskUserForSelection += DisplaySelectionDialog;
            Message += DisplayMessage;
            Error += DisplayError;
            Progress += ReportProgress;
            DownloadsComplete += ReportDownloadsComplete;
        }

        public event DisplayYesNoDialog AskUser;

        public event DisplaySelectionDialog AskUserForSelection;

        public event DisplayMessage Message;

        public event DisplayError Error;

        public event ReportProgress Progress;

        public event DownloadsComplete DownloadsComplete;

        public virtual bool Headless
        {
            get { return false; }
        }

        protected virtual bool DisplayYesNoDialog(string message)
        {
            return true;
        }

        protected virtual void DisplayMessage(string message, params object[] args)
        {
        }

        protected virtual int DisplaySelectionDialog(string message, params object[] args)
        {
            return 0;
        }

        protected virtual void DisplayError(string message, params object[] args)
        {
        }

        protected virtual void ReportProgress(string format, int percent)
        {
        }

        protected virtual void ReportDownloadsComplete(Uri[] urls, string[] filenames, Exception[] errors)
        {
        }

        public virtual int WindowWidth
        {
            get { return -1; }
        }

        public void RaiseMessage(string message, params object[] url)
        {
            Message(message, url);
        }

        public void RaiseProgress(string message, int percent)
        {
            Progress(message, percent);
        }

        public bool RaiseYesNoDialog(string question)
        {
            //Return value will be from last handler added.
            return AskUser(question);
        }

        public int RaiseSelectionDialog(string message, params object[] args)
        {
            return AskUserForSelection(message, args);
        }

        public void RaiseError(string message, params object[] args)
        {
            Error(message, args);
        }

        public void RaiseDownloadsCompleted(Uri[] file_urls, string[] file_paths, Exception[] errors)
        {
            DownloadsComplete(file_urls, file_paths, errors);
        }
    }
}