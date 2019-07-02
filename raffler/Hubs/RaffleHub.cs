﻿using Microsoft.AspNetCore.SignalR;
using shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace raffler.Hubs
{
    public class RaffleHub : Hub
    {
        public async Task AddNewRaffleEntry(RaffleEntry entry)
        {
            await Clients.All.SendAsync("addNewRaffleEntry", entry);
        }
    }
}
