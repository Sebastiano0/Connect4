using Connect4.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Connect4
{
    [HubName("ShowMatchHub")]
    public class ShowMatchHub : Hub
    {
        [HubMethodName("BroadcastMatch")]
        public static void BroadcastMatch()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ShowMatchHub>();

            if (context.Clients != null)
            {
                context.Clients.All.GetUpdateData();
            }
        }

        [HubMethodName("UpdateTable")]
        public static void UpdateTable()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ShowMatchHub>();

            if (context.Clients != null)
            {
                context.Clients.All.GetUpdateTable();
            }
        }
    }
}