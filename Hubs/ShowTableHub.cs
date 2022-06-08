using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Connect4.Hubs
{
    [HubName("ShowTableHub")]
    public class ShowTableHub : Hub
    {
        [HubMethodName("UpdateTable")]
        public static void UpdateTable()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ShowTableHub>();
            if (context.Clients != null)
            {
                context.Clients.All.GetUpdateTable();
            }
        }
    }
}