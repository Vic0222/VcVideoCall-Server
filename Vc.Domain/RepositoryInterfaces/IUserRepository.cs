﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vc.Domain.Entities;

namespace Vc.Domain.RepositoryInterfaces
{
    public interface IUserRepository : IRepository
    {
        Task<List<User>> GetUsersWithRoomIdAsync(string roomId);
        Task<User> GetUserAsync(string sender);
    }
}
