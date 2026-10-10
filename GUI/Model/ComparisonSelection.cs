using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN.GUI
{
    /// <summary>
    /// Which entries of the installation history are being compared.
    /// The entry chosen with a plain click is the reference.
    /// One more entry can be added with Ctrl or Shift,
    /// and it can be either older or newer than the reference.
    /// </summary>
    public class ComparisonSelection
    {
        public int? Reference { get; private set; }
        public int? Other     { get; private set; }

        /// <summary>
        /// The entries that should be highlighted
        /// </summary>
        public IReadOnlyCollection<int> Indices
            => new int?[] { Reference, Other }.OfType<int>().ToArray();

        /// <summary>
        /// Which two entries to compare
        /// </summary>
        /// <param name="entryCount">
        /// How many entries the list has, the newest one is first
        /// </param>
        /// <returns>
        /// Null if nothing is selected.
        /// Otherwise the newer entry, and the older one to compare it with.
        /// With two entries chosen they are the pair,
        /// with one the older is the entry just below it (null for the last one).
        /// </returns>
        public (int Newer, int? Older)? Compared(int entryCount)
        {
            if (Reference is not int reference)
            {
                return null;
            }
            if (Other is int other)
            {
                return (Math.Min(reference, other), Math.Max(reference, other));
            }
            return (reference, reference + 1 < entryCount ? reference + 1 : null);
        }

        public void Reset()
        {
            Reference = null;
            Other     = null;
        }

        /// <summary>
        /// An entry was selected by the user
        /// </summary>
        /// <param name="index">Position of the entry</param>
        /// <param name="ctrl">True if Ctrl is held</param>
        /// <param name="shift">True if Shift is held</param>
        /// <param name="selected">Everything the list says is selected right now</param>
        /// <param name="focused">The entry that has the focus, if any</param>
        public void Select(int                      index,
                           bool                     ctrl,
                           bool                     shift,
                           IReadOnlyCollection<int> selected,
                           int?                     focused)
        {
            if (Reference is not int reference)
            {
                Reference = index;
                Other     = null;
            }
            else if (ctrl)
            {
                if (index != reference)
                {
                    Other = index;
                }
            }
            else if (shift)
            {
                // The list selects the whole range, the end farthest from the reference
                // (or the one with the focus) is the one that was clicked
                var others = selected.Where(i => i != reference).ToArray();
                if (others.Length > 0)
                {
                    Other = focused is int f && others.Contains(f)
                        ? f
                        : others.OrderByDescending(i => Math.Abs(i - reference)).First();
                }
            }
            else
            {
                Reference = index;
                Other     = null;
            }
        }

        /// <summary>
        /// An entry was deselected by the user with Ctrl held
        /// </summary>
        public void Deselect(int index)
        {
            if (index == Other)
            {
                Other = null;
            }
            else if (index == Reference)
            {
                Reference = Other;
                Other     = null;
            }
        }

        /// <summary>
        /// Accept what the list actually shows after the user did something that
        /// we don't track, like a plain click on a selected entry or on empty space
        /// </summary>
        /// <param name="actual">Everything the list says is selected right now</param>
        public void Reconcile(IReadOnlyCollection<int> actual)
        {
            var kept = Indices.Where(actual.Contains).ToArray();
            Reference = kept.Length > 0 ? kept[0]
                                        : actual.Cast<int?>().FirstOrDefault();
            Other     = kept.Length > 1 ? kept[1]
                                        : null;
        }
    }
}
