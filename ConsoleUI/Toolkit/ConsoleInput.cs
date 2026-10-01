using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CKAN.ConsoleUI.Toolkit {

    /// <summary>
    /// The console UI's input source.
    ///
    /// A background thread blocks in Console.ReadKey and queues each keystroke.
    /// The UI thread blocks on the queue, waiting for either a keystroke or an action to come in from other threads.
    ///
    /// Originally added so that we can respond to the URL handler pipe while waiting on input.
    /// </summary>
    public static class ConsoleInput {

        /// <summary>
        /// Start the background reader thread.
        /// </summary>
        public static void Start()
        {
            if (readerThread != null) {
                return;
            }

            readerThread = new Thread(ReadLoop) {
                IsBackground = true,
                Name = "ConsoleInputReader",
            };

            readerThread.Start();
        }

        /// <summary>
        /// Run an action on the UI thread. Safe to call from any thread.
        /// </summary>
        /// <param name="action">Work to run on the UI thread</param>
        /// <param name="owner">The screen the action belongs to</param>
        public static void Post(Action action, object owner)
        {
            queue.Add((null, action, owner));
        }

        /// <summary>
        /// Block until the next event and pump it. Runs a posted action or returns a keystroke.
        /// Actions belonging to another owner are discarded.
        /// </summary>
        /// <param name="owner">The screen that's pumping, or null to discard every posted action</param>
        /// <returns>The keystroke, or null if a posted action came up instead</returns>
        public static ConsoleKeyInfo? PumpEvent(object? owner = null)
        {
            var ev = queue.Take();
            if (ev.Key is ConsoleKeyInfo k) {
                return k;
            }

            if (ev.Owner == owner) {
                ev.Action?.Invoke();
            }

            return null;
        }

        /// <summary>
        /// Convenience method that blocks until the next keystroke, silently discarding any posted actions in the meantime.
        /// </summary>
        /// <returns>The next keystroke</returns>
        public static ConsoleKeyInfo NextKey()
        {
            while (true) {
                if (PumpEvent() is ConsoleKeyInfo k) {
                    return k;
                }
            }
        }

        private static void ReadLoop()
        {
            while (true) {
                // There's no way to interrupt this thread while it's in ReadKey.
                // That breaks going from ckan prompt to consoleui and back to ckan prompt.
                // Another option might be polling every X ms, but it's
                // probably not worth it for that rare use case.
                queue.Add((Console.ReadKey(true), null, null));
            }
        }

        // BlockingCollection defaults to ConcurrentQueue, which is FIFO.
        private static readonly BlockingCollection<(ConsoleKeyInfo? Key, Action? Action, object? Owner)> queue
            = new BlockingCollection<(ConsoleKeyInfo? Key, Action? Action, object? Owner)>();

        private static Thread? readerThread;
    }
}
