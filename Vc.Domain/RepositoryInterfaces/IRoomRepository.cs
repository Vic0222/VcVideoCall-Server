using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vc.Domain.Entities;

namespace Vc.Domain.RepositoryInterfaces
{
    public interface IRoomRepository : IRepository
    {
        Task<string> AddRoomAsync(Room room);
        Task<Room> GetRoomAsync(string roomId);

        Task<Room> GetPrivateRoomAsync(string userId1, string userId2);
        Task<List<Room>> GetUserRoomsAsync(string userId);
        Task<bool> IsUserInRoomAsync(string userId, string roomId);
    }
}
