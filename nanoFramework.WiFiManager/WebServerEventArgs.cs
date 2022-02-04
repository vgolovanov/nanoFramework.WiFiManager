using System;
using System.Net;
using System.Text;

namespace nanoFramework.WiFiManager
{
    /// <summary>
    /// Web server event argument class
    /// </summary>
    public class WebServerEventArgs
    {
        /// <summary>
        /// Constructor for the event arguments
        /// </summary>
        public WebServerEventArgs(HttpListenerContext context)
        {
            Context = context;
        }

        /// <summary>
        /// The response class
        /// </summary>
        public HttpListenerContext Context { get; protected set; }
    }
}
