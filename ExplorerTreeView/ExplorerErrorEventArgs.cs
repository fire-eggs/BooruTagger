using System;

namespace ImageTag.ExplorerTreeView
{
    /// <summary>
    /// Provides data for explorer navigation exception handling.
    /// </summary>
    public class ExplorerErrorEventArgs: EventArgs
    {
        /// <summary>
        /// Gets thrown exception.
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        public ExplorerErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}