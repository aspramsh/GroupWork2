using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace FindingOutDevices
{
    /// <summary>
    /// A functional class that does authorization
    /// </summary>
    public class Authorizator
    {
        // A port number property
        public int Port { get; set; }
        // Waiting time property
        public int Time { get; set; }
        // Response Data property
        public string ResponseData { get; set; }
        // Request Data property
        public string RequestData { get; set; }
        /// <summary>
        /// A method that innitializes an authorizator instance
        /// </summary>
        /// <param name="responseData"></param>
        /// <param name="requestData"></param>
        /// <param name="port"></param>
        /// <param name="time"></param>
        internal void Initialize(string responseData, string requestData, int port, int time)
        {
            this.ResponseData = responseData;
            this.RequestData = requestData;
            this.Port = port;
            this.Time = time;
        }
        /// <summary>
        /// A method that runs client and server functions on different threads
        /// </summary>
        /// <param name="IPs"> A list of devices in current network </param>
        /// <param name="updateUI"> a delegate that points to a function that updates UI </param>
        public void Run(List<IPAddress> IPs, Action<List<string>> updateUI)
        {
            // A thread that starts the server
            Thread ServerThread = new Thread(() =>
            {
                this.StartServer();
            }
            );
            ServerThread.Start();
            // A thread that runs clients
            Thread ClientThread = new Thread(() =>
            {
                StartClient(IPs, updateUI);
            });
            ClientThread.Start();
        }
        /// <summary>
        /// A method that starts the server
        /// </summary>
        private void StartServer()
        {
            IPHostEntry ipHost = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, this.Port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Listen(10);
            while (true)
            {
                Socket handler = listener.Accept();
                BinaryFormatter formatter = new BinaryFormatter();
                while (true)
                {
                    byte[] bytes = new byte[1024];
                    handler.Receive(bytes);
                    Stream stream = new MemoryStream(bytes);
                    Request request = new Request();
                    request.Data = (string)formatter.Deserialize(stream);
                    if (request.Data.IndexOf("?") > -1)
                    {
                        break;
                    }
                }
                Thread.Sleep(1000);
                Response Resp = new Response();
                Resp.Data = this.ResponseData;
                MemoryStream memoryStream = new MemoryStream();
                formatter.Serialize(memoryStream, Resp.Data);
                memoryStream.Flush();
                memoryStream.Position = 0;
                handler.Send(ReadFully(memoryStream));
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
        /// <summary>
        /// A method that starts Clients
        /// </summary>
        /// <param name="ips"></param>
        /// <param name="updateUI"></param>
        private void StartClient(List<IPAddress> ips, Action<List<string>> updateUI)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // A list for storing active users data
            List<string> users = new List<string>();
            while (true)
            {
                foreach (IPAddress ip in ips)
                {
                    IPEndPoint endPoint = new IPEndPoint(ip, this.Port);
                    Socket sender = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);
                    // This try catch block checks which computers have the same server
                    try
                    {
                        sender.Connect(endPoint);
                        Request request = new Request();
                        request.Data = this.RequestData;
                        Stream memoryStream = new MemoryStream();
                        formatter.Serialize(memoryStream, request.Data);
                        memoryStream.Flush();
                        memoryStream.Position = 0;
                        sender.Send(ReadFully(memoryStream));
                        byte[] bytes = new byte[1024];
                        sender.Receive(bytes);
                        Stream stream = new MemoryStream(bytes);
                        Response response = new Response();
                        response.Data = (string)formatter.Deserialize(stream);
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                        users.Add(response.Data);
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                    }
                }
                // After getting data UI is updated
                updateUI(users);
                Thread.Sleep(this.Time);
            }
        }
        /// <summary>
        /// A method that returns a list of our IP addresses
        /// </summary>
        /// <returns></returns>
        public List<IPAddress> GetIPAddresses()
        {
            List<IPAddress> addresses = new List<IPAddress>();
            Process p = new Process();

            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = @"C:\Windows\System32\ARP.EXE";
            p.StartInfo.Arguments = @"-a";
            p.Start();

            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            p.WaitForExit();
            // Read the output stream first and then wait.
            List<string> lines = new List<string>();
            while (!p.StandardOutput.EndOfStream)
            {
                string line = p.StandardOutput.ReadLine();
                lines.Add(line);
            }
            foreach (string line in lines)
            {
                int start = line.IndexOf("192.168");
                string ipAddress = "";
                if (start > -1)
                {
                    while (start < line.Length && !line[start].Equals(' '))
                    {
                        ipAddress += line[start];
                        ++start;
                    }
                    addresses.Add(IPAddress.Parse(ipAddress));
                }
            }
            return addresses;
        }
        /// <summary>
        /// A method that converts stream to binary array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[1024];

            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
