﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vc.Domain.Entities;

namespace Vc.Domain.RepositoryInterfaces
{
    public interface IMessageRepository : IRepository
    {
        Task AddMessageAsync(Message message);
        Task<List<Message>> GetMessagesByRoomIdOfUserAsync(string roomId, DateTime? lastMessageDatetime);
        Task<Message> GetRoomLastMessageAsync(string roomId);
    }
}
