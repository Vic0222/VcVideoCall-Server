using System;
using System.Collections.Generic;
using System.Text;
using Vc.DAL.Mongo.Collections;
using Vc.Domain.RepositoryInterfaces;
using Dom = Vc.Domain.Entities;
using Dal = Vc.DAL.Mongo.Collections;
using AutoMapper;
using MongoDB.Driver;
using System.Threading.Tasks;
using Vc.Common;

namespace Vc.DAL.Mongo.Repositories
{
    public class UserRepository : AbstractRepository<Dal.User>, IUserRepository
    {
        public UserRepository(ClientManager clientManager, IMapper mapper) : base(clientManager, mapper, collectionName: "users")
        {
        }

        public async Task<Dom.User> GetUserAsync(string userId)
        {
            var users = await _collection.FindAsync(u => u.Id == userId);
            var dalUser = users.FirstOrDefault();
            Dom.User domUser = dalUser.IsNotNull() ? _mapper.Map<Dom.User>(dalUser) : null;
            return domUser;
        }


        public async Task<List<Dom.User>> GetUsersWithRoomIdAsync(string roomId)
        {
            throw new NotImplementedException();
        }
    }
}
