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

        public async Task<List<Message>> GetMessagesByRoomIdOfUserAsync(string roomId)
        {
            var filter = Builders<Dal.Message>.Filter.Eq(m => m.RoomId, roomId);

            var sort = Builders<Dal.Message>.Sort.Descending(nameof(Dal.Message.DateSent));

            var options = new FindOptions<Dal.Message, Dal.Message>()
            {
                Sort = sort
            };

            var dalMessages = await _collection.FindAsync<Dal.Message>(filter, options);
            return _mapper.Map<List<Message>>(dalMessages.ToList());
        }
    }
}
