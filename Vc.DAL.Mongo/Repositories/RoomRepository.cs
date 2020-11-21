using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vc.Domain.Entities;
using Vc.Domain.RepositoryInterfaces;
using Dom = Vc.Domain.Entities;
using Dal = Vc.DAL.Mongo.Collections;
using MongoDB.Driver;
using AutoMapper;
using Vc.Common;
using System.Linq;

namespace Vc.DAL.Mongo.Repositories
{
    public class RoomRepository : AbstractRepository<Dal.Room>, IRoomRepository
    {

        public RoomRepository(ClientManager clientManager, IMapper mapper) : base(clientManager, mapper, collectionName: "rooms")
        {
        }

        public async Task<string> AddRoomAsync(Dom.Room room)
        {
            Dal.Room dalRoom = _mapper.Map<Dal.Room>(room);

            foreach (var domUser in room.RoomUsers)
            {
                Dal.RoomUser dalRoomUser = _mapper.Map<Dal.RoomUser>(domUser);
                dalRoom.RoomUsers.Add(dalRoomUser);
            }

            await _collection.InsertOneAsync(dalRoom);
            return dalRoom.Id;
        }

        public async Task<Room> GetIndividualRoomAsync(string userId1, string userId2)
        {
            var privateRooms = await _collection.FindAsync(r => r.Type == (byte)RoomType.Private && r.RoomUsers.Any(u=>u.UserId== userId1) && r.RoomUsers.Any(u => u.UserId == userId2));
            Dal.Room source = privateRooms.FirstOrDefault();
            return source.IsNotNull() ? _mapper.Map<Dom.Room>(source) : null;
        }

        public async Task<Room> GetRoomAsync(string roomId)
        {
            var matchRooms = await _collection.FindAsync(r => r.Id == roomId);
            Dal.Room source = matchRooms.FirstOrDefault();
            return source.IsNotNull() ? _mapper.Map<Room>(source) : null; 
        }
    }
}
