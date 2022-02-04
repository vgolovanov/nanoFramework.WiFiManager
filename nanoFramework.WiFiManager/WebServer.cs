using System;
using System.Net;
using System.Text;
using System.Threading;

namespace nanoFramework.WiFiManager
{
    public class WebServer
    {
                /// <summary>
        /// Delegate for the CommandReceived event.
        /// </summary>
        public delegate void GetRequestHandler(object obj, WebServerEventArgs e);

        /// <summary>
        /// CommandReceived event is triggered when a valid command (plus parameters) is received.
        /// Valid commands are defined in the AllowedCommands property.
        /// </summary>
        public event GetRequestHandler CommandReceived;

        /// <summary>
        /// Gets or sets the port the server listens on.
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// The type of Http protocol used, http or https
        /// </summary>
        public HttpProtocol Protocol { get; protected set; }

        /// <summary>
        /// Instantiates a new web server.
        /// </summary>
        /// <param name="port">Port number to listen on.</param>
        /// <param name="protocol"><see cref="HttpProtocol"/> version to use with web server.</param>    
        /// 
        private HttpListener _listener;
        private Thread _serverThread;
        private bool _cancel = false;

        public WebServer(int port, HttpProtocol protocol=HttpProtocol.Http)
        {
            Protocol = protocol;
            Port = port;
            string prefix = Protocol == HttpProtocol.Http ? "http" : "https";
            _listener = new HttpListener(prefix, port);
            _serverThread = new Thread(RunServer);         
        }

            
        public bool Start()
        {
            bool bStarted = true;

            try
            {
                //if (_listener == null)
                //{
                //    _serverThread.Start();
                //}
                _serverThread.Start();
            }
            catch
            {                   
                bStarted = false;
            }

            return bStarted;
        }

        /// <summary>
        /// Restart the server.
        /// </summary>
        private bool Restart()
        {
            Stop();
            return Start();
        }

        public void Stop()
        {
            if (_listener != null)
            {
                _listener.Stop();
                Thread.Sleep(100);
                _serverThread.Abort();
            }                
        }

        private void RunServer()
        {
            _listener.Start();

            // while (_listener.IsListening)
            while (!_cancel)
            {
                HttpListenerContext context =null;

                try
                {
                    context = _listener.GetContext();
                }
                catch 
                {
                    continue;
                };

                new Thread(() =>
                {
                    if (context != null)
                    ProcessRequest(context);
                }).Start();
            }

            _listener.Close();
            _listener = null;
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if(request.HttpMethod== "GET")
            {
                string[] url = request.RawUrl.Split('?');
                if (url[0] == "/favicon.ico")
                {
                    response.ContentType = "image/png";                                
                    OutputByteStream (response, Resources.GetBytes(Resources.BinaryResources.favicon));
                    response.Close();
                    return;
                }
            }
                               
            if (CommandReceived != null)
            {                
                CommandReceived.Invoke(this, new WebServerEventArgs(context));       
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.ContentLength64 = 0;          
            }

            response.Close();
        }

        /// <summary>
        /// Output a stream
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="responseString">the stream to output</param>
        public void OutPutStream(HttpListenerResponse response, string responseString)
        {
            if (response == null)
            {
                return;
            }

            if (responseString != null)
            {
                byte[] messageBody = Encoding.UTF8.GetBytes(responseString);
                OutputByteStream(response, messageBody);
            }                     
        }

        /// <summary>
        /// Output a stream
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="strResponse">the stream to output</param>
        public void OutputByteStream(HttpListenerResponse response, byte[] responseBytes)
        {
            if (response == null)
            {
                return;
            }

            response.ContentLength64 = responseBytes.Length;
            response.ContentType = "text/html";
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }


        /// <summary>
        /// Output an HTTP Code and close the connection
        /// </summary>
        /// <param name="response">the socket stream</param>
        /// <param name="code">the http code</param>
        public void OutputHttpCode(HttpListenerResponse response, HttpStatusCode code)
        {
            if (response == null)
            {
                return;
            }

            // This is needed to force the 200 OK without body to go thru
            response.ContentLength64 = 0;
            response.KeepAlive = false;
            response.StatusCode = (int)code;
        }
    }
}
