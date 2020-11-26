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

namespace Vc.DAL.Mongo.Repositories
{
    public class MessageRepository : AbstractRepository<Dal.Message>, IMessageRepository
    {

        public MessageRepository(ClientManager clientManager, IMapper mapper) : base(clientManager, mapper, "messages")
        {
        }

        public async Task AddMessageAsync(Dom.Message domMessage)
        {
            var dalMessage = _mapper.Map<Dal.Message>(domMessage);
            await _collection.InsertOneAsync(dalMessage);
        }

        public async Task<List<Message>> GetMessagesByRoomIdOfUserAsync(string roomId, DateTime? lastMessageDatetime)
        {

            var builder = Builders<Dal.Message>.Filter;
            var filter = builder.Eq(m => m.RoomId, roomId);
            if (lastMessageDatetime != null)
            {
                filter = filter & builder.Gt(m => m.DateSent, lastMessageDatetime);
            }

            var dalMessages = _collection.Find(filter).SortByDescending(m => m.DateSent);

            List<Dal.Message> domMessages = null;

            if (lastMessageDatetime == null)
            {
                domMessages = await dalMessages.Limit(25).ToListAsync();
            }
            else
            {
                domMessages = await dalMessages.ToListAsync();
            }

            return _mapper.Map<List<Message>>(domMessages);
        }
    }
}
