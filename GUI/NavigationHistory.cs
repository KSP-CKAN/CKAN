using System;
using System.Collections.Generic;

namespace CKAN.GUI
{

    /// <summary>
    /// Generic class for keeping a browser-like navigation history.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NavigationHistory<T>
    {
        private readonly List<T> m_navigationHistory;
        private int m_currentIndex;

        public NavigationHistory()
        {
            m_navigationHistory = new List<T>();
            m_currentIndex = -1;
            IsReadOnly = false;
        }

        #region Events

        public delegate void HistoryChangeHandler();

        public event HistoryChangeHandler OnHistoryChange;

        public void InvokeOnHistoryChange()
        {
            OnHistoryChange?.Invoke();
        }

        #endregion

        /// <summary>
        /// Indicates whether it's possible to navigate backwards.
        /// </summary>
        public bool CanNavigateBackward => m_currentIndex > 0;

        /// <summary>
        /// Indicates whether it's possible to navigate forwards.
        /// </summary>
        public bool CanNavigateForward => m_currentIndex < (m_navigationHistory.Count - 1);

        /// <summary>
        /// Indicates whether the history is in read-only mode.
        /// During read-only mode, all calls that could modify the state of the
        /// history are silenly ignored.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Adds the given item to the head of the navigation history.
        /// </summary>
        /// <param name="item"></param>
        public void AddToHistory(T item)
        {
            if (IsReadOnly)
            {
                return;
            }

            /*
             * This operation removes all history AHEAD of the current index,
             * adds a new item to the head of the list, and advances the index.
             *
             * Let's say this is your current state:
             *
             * =============
             * |x|y|z|a|b|c|
             * =============
             *      ^
             *
             * When you add an item to the history ('d'),
             * the next state will be:
             *
             * =========
             * |x|y|z|d|
             * =========
             *        ^
             */

            if (CanNavigateForward)
            {
                m_navigationHistory.RemoveRange(m_currentIndex + 1, m_navigationHistory.Count - (m_currentIndex + 1));
            }

            m_navigationHistory.Add(item);
            m_currentIndex++;

            InvokeOnHistoryChange();
        }

        /// <summary>
        /// Moves backward by one item in the navigation history, and returns
        /// the item at that spot.
        /// </summary>
        public T NavigateBackward()
        {
            if (IsReadOnly)
            {
                return default;
            }

            if (!CanNavigateBackward)
            {
                throw new InvalidOperationException("Cannot navigate backward in history!");
            }

            m_currentIndex--;
            InvokeOnHistoryChange();

            return m_navigationHistory[m_currentIndex];
        }

        /// <summary>
        /// Moves forward by one item in the navigation history, and returns
        /// the item at that spot.
        /// </summary>
        public T NavigateForward()
        {
            if (IsReadOnly)
            {
                return default;
            }

            if (!CanNavigateForward)
            {
                throw new InvalidOperationException("Cannot navigate forward in history!");
            }

            m_currentIndex++;
            InvokeOnHistoryChange();

            return m_navigationHistory[m_currentIndex];
        }
    }
}
