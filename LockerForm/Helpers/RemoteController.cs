using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Locker.Helpers
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
        private BackgroundWorker worker;
        private bool _cancelling = false;
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
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + portNumber.ToString() + "/");

            try
            {
                listener.Start();
                
                TextReader tr = new StreamReader("RemoteControlPage.htm");
                string pageString = tr.ReadToEnd();
                while (!worker.CancellationPending)
                {
                   
                    HttpListenerContext context = listener.GetContext();
                    if (context.Request.HttpMethod == "POST")
                    {
                        var body = new StreamReader(context.Request.InputStream).ReadToEnd();
                        ParsePostParameters(body);
                        worker.ReportProgress(1);
                    }
                    
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
            _cancelling = false;
        }

        public void StartServer()
        {
            worker.RunWorkerAsync();

            string portString = "";
            if (portNumber != 80)
                portString = ":" + portNumber.ToString();
            
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            string ipString = null;
            foreach (IPAddress ip in localIPs)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ipString = ip.ToString();
        }

        public void StopServer()
        {
            worker.CancelAsync();
            _cancelling = true;
            listener.Close();
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
                    value = System.Uri.UnescapeDataString(value);
                    pageParams.Add(key, value);
                }
            }
        }
    }
}
