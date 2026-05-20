using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CKAN.GUI
{

    /// <summary>
    /// Generic class for keeping a browser-like navigation history.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NavigationHistory<T>
        where T : notnull
    {
        #region Events

        public delegate void HistoryChangeHandler();

        public event HistoryChangeHandler? OnHistoryChange;

        private void InvokeOnHistoryChange()
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
        /// Adds the given item to the head of the navigation history.
        /// </summary>
        /// <param name="item"></param>
        public void AddToHistory(T item)
        {
            lock (mutex)
            {
                if (m_currentIndex >= 0 && m_currentIndex < m_navigationHistory.Count
                    && item.Equals(m_navigationHistory[m_currentIndex]))
                {
                    // Don't add (or truncate) if same as current item
                    return;
                }

                if (CanNavigateForward)
                {
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
                    m_navigationHistory.RemoveRange(m_currentIndex + 1, m_navigationHistory.Count - (m_currentIndex + 1));
                }

                m_navigationHistory.Add(item);
                m_currentIndex++;

                InvokeOnHistoryChange();
            }
        }

        public bool TryGoBackward([NotNullWhen(true)] out T? newCurrentItem)
        {
            lock (mutex)
            {
                if (CanNavigateBackward)
                {
                    newCurrentItem = m_navigationHistory[--m_currentIndex];
                    InvokeOnHistoryChange();
                    return true;
                }
                newCurrentItem = default;
                return false;
            }
        }

        public bool TryGoForward([NotNullWhen(true)] out T? newCurrentItem)
        {
            lock (mutex)
            {
                if (CanNavigateForward)
                {
                    newCurrentItem = m_navigationHistory[++m_currentIndex];
                    InvokeOnHistoryChange();
                    return true;
                }
                newCurrentItem = default;
                return false;
            }
        }

        private readonly List<T> m_navigationHistory = new List<T>();
        private readonly object mutex = new object();
        private int m_currentIndex = -1;
    }
}
