using System;

namespace OpenAC.Net.DFe.Core.Service
{
    internal class DFeMessageEventArgs : EventArgs
    {
        #region Constructors

        public DFeMessageEventArgs(string message)
        {
            this.Message = message;
        }

        #endregion Constructors

        #region Properties

        public string Message { get; }

        #endregion Properties
    }
}