using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using static FastKDS.Controllers.OrderController;

namespace FastKDS.Hubs
{
    [HubName("ChatHub")]
    public class ChatHub : Hub
    {
        //public void Send(string name, string message)
        //{
        //    // Call the addNewMessageToPage method to update clients.
        //    Clients.All.addNewMessageToPage(name, message);
        //}

        public void Send(string data)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(data);
        }

    }
}