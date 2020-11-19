using System;
using System.Collections.Generic;
using System.Text;
using Vc.Domain.Entities;

namespace Vc.Domain.RepositoryInterfaces
{
    public interface IUserRepository : IRepository
    {
        List<User> GetUsersWithRoomId(string roomId);
    }
}
