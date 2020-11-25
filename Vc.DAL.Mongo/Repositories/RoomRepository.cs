﻿using System;
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
            await _collection.InsertOneAsync(dalRoom);
            return dalRoom.Id;
        }

        public async Task<Room> GetPrivateRoomAsync(string userId1, string userId2)
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

        public async Task<List<Room>> GetUserRoomsAsync(string userId)
        {
            var matchRooms = await _collection.FindAsync(r => r.RoomUsers.Any(u => u.UserId == userId));
            var dalRooms = matchRooms.ToList();
            return matchRooms.IsNotNull() ? _mapper.Map<List<Room>>(dalRooms) : new List<Room>();

        }

        public async Task<bool> IsUserInRoomAsync(string userId, string roomId)
        {
            var room = await _collection.FindAsync(r => r.Id == roomId && r.RoomUsers.Any(ru => ru.UserId == userId));
            return room.IsNotNull();
        }
    }
}
