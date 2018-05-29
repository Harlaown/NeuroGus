using System;

namespace NeuroGus.Core.Model
{
    public class NeuroNetworkEventArgs : EventArgs
    {
        public string Message { get; set; }

        public NeuroNetworkEventArgs(string message)
        {
            Message = message;
        }

        public NeuroNetworkEventArgs()
        {
            
        }
    }
}