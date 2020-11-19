using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vc.Domain.Entities;
using Vc.Domain.RepositoryInterfaces;
using Dom = Vc.Domain.Entities;
using Dal = Vc.Domain.Entities;
using MongoDB.Driver;

namespace Vc.DAL.Mongo.Repositories
{
    public class RoomRepository : AbstractRepository, IRoomRepository
    {
        private string collectionName = "Room";
        private IMongoCollection<Room> rooms;

        public RoomRepository(ClientManager clientManager) : base(clientManager)
        {
            rooms = _clientManager.GetDatabase().GetCollection<Dal.Room>(collectionName);
        }

        public async Task<string> AddRoomAsync(Dom.Room room)
        {
            var database = _clientManager.GetDatabase();
            Room roomDoc = new Dal.Room() { Name = room.Name, Type = room.Type };
            await rooms.InsertOneAsync(roomDoc);
            return roomDoc.Id;
        }

        public async Task<Room> GetIndividualRoomAsync(string userId1, string userId2)
        {
            var privateRooms = await rooms.FindAsync(r => r.Type == RoomType.Private);
            return privateRooms.FirstOrDefault();
        }

        public async Task<Room> GetRoomAsync(string roomId)
        {
            var matchRooms = await rooms.FindAsync(r => r.Id == roomId);
            return matchRooms.FirstOrDefault();
        }
    }
}
