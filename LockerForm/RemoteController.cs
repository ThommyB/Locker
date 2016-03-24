using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Locker
{
    public class CommandReceivedEventArgs : EventArgs
    {
        public CommandReceivedEventArgs(bool enabled, string fingerprint)
        {
            Enabled = enabled;
            Fingerprint = fingerprint;
        }

        public bool Enabled { get; private set; }
        public string Fingerprint { get; private set; }
    }

    public class RemoteController
    {
        private bool _cancel = true;
        private BackgroundWorker worker;
        private bool cancelling = false;
        private HttpListener listener;
        private int portNumber = 8080;
        static Dictionary<string, string> pageParams = new Dictionary<string, string>();

        public event EventHandler<CommandReceivedEventArgs> CommandReceived;

        public RemoteController()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;

            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;

            StartServer();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Create a listener.
            listener = new HttpListener();
            // Add the prefixes.
            listener.Prefixes.Add("http://*:" + portNumber.ToString() + "/");
            try
            {
                listener.Start();
                //read in the html page to serve
                TextReader tr = new StreamReader("RemoteControlPage.htm");
                string pageString = tr.ReadToEnd();
                while (!worker.CancellationPending)
                {
                    // Note: The GetContext method blocks while waiting for a request. 
                    HttpListenerContext context = listener.GetContext();
                    if (context.Request.HttpMethod == "POST")
                    {
                        //this is a POST, handle the input parameters
                        var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                        ParsePostParameters(body);
                        worker.ReportProgress(1); //raise the event
                    }
                    // Set up the response object to serve the page
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(pageString);
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
                listener.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (pageParams.Count > 0)
            {
                if (pageParams.ContainsKey("enabled") && pageParams.ContainsKey("fingerprint"))
                {
                    if(CommandReceived != null)
                        CommandReceived(this, new CommandReceivedEventArgs(pageParams["enabled"] == "true", pageParams["fingerprint"]));
                }
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            cancelling = false;
        }

        public void StartServer()
        {
            worker.RunWorkerAsync();
            //Display some useful info

            string portString = "";
            if (portNumber != 80)
                portString = ":" + portNumber.ToString();
            //display the local machine name /  IP address for use with remote handler
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            string ipString = null;
            foreach (IPAddress ip in localIPs)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ipString = ip.ToString();
        }

        public void StopServer()
        {
            worker.CancelAsync();
            cancelling = true; //this prevents the error message display from the exception in the next line
            listener.Close(); //this causes the the listener to throw an exception
        }

        private static void ParsePostParameters(string body)
        {
            string[] stringParams = body.Split('&');
            pageParams.Clear();
            foreach (string s in stringParams)
            {
                int index = s.IndexOf('=');
                if (index > -1)
                {
                    string key = s.Substring(0, index);
                    string value = s.Substring(index + 1);
                    value = System.Uri.UnescapeDataString(value); //removes all the secret special-character encoding
                    pageParams.Add(key, value);
                }
            }
        }
    }
}
