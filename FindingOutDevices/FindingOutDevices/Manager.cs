using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;

namespace FindingOutDevices
{
    /// <summary>
    /// A module that leads all the work
    /// </summary>
    public class Manager
    {
        /// <summary>
        /// a method that runs an instance of Manager class
        /// </summary>
        /// <param name="updateUI"></param>
        public void Run(Action<List<string>> updateUI)
        {
            // creating an instance of Authorizator class
            Authorizator authorizator = new Authorizator();
            // reading response data from config file
            string responseData = ConfigurationSettings.AppSettings["Response Data"];
            // reading requst data from config file
            string requestData = ConfigurationSettings.AppSettings["Request Data"];
            // reading port number from config file
            int port = Convert.ToInt32(ConfigurationSettings.AppSettings["Port"]);
            // reading time for update from config file
            int time = Convert.ToInt32(ConfigurationSettings.AppSettings["Time"]);
            // A list that keeps all IP addresses in the current network
            List<IPAddress> IPs = authorizator.GetIPAddresses();
            // Calling Initialize method of authorizator to transfer data
            authorizator.Initialize(responseData, requestData, port, time);
            // calling run method of authorizator
            authorizator.Run(IPs, updateUI);
        }
    }
}
