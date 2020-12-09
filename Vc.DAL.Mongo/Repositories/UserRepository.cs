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
using System.Threading;
using Vc.DAL.Mongo.Transactions;

namespace Vc.DAL.Mongo.Repositories
{
    public class UserRepository : AbstractRepository<Dal.User>, IUserRepository
    {
        public UserRepository(ClientManager clientManager, IMapper mapper, MongoDatabaseSessionManager mongoDatabaseSessionManager) : base(clientManager, mapper, collectionName: "users", mongoDatabaseSessionManager)
        {
        }

        public async Task<Dom.User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var users = await _collection.FindAsync(u => u.Id == userId, cancellationToken: cancellationToken);
            var dalUser = users.FirstOrDefault();
            Dom.User domUser = dalUser.IsNotNull() ? _mapper.Map<Dom.User>(dalUser) : null;
            return domUser;
        }

        public async Task<List<Dom.User>> GetUsersWithRoomIdAsync(string roomId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateUserAsync(string id, Dom.User user, CancellationToken cancellationToken = default)
        {
            var builder = Builders<Dal.User>.Filter;
            var filter = builder.Eq(m => m.Id, id);

            var dalUser = _mapper.Map<Dal.User>(user);

            await _collection.ReplaceOneAsync(filter, dalUser, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Updates instances of photoUrl (User.PhotoUrl, RoomUsers.PhotoUrl)
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <param name="photoUrl">The new photoUrl</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task UpdateUserPhotoUrlAsync(string userId, string photoUrl, CancellationToken cancellationToken = default)
        {
            //update photo url on users first
            var builder = Builders<Dal.User>.Filter;
            var filter = builder.Eq(m => m.Id, userId);

            var update = Builders<Dal.User>.Update.Set(nameof(Dal.User.PhotoUrl), photoUrl);
            if (_mongoDatabaseSessionManager?.MongoDatabaseSession?.CurrentSession != null)
            {
                await _collection.UpdateOneAsync(_mongoDatabaseSessionManager?.MongoDatabaseSession?.CurrentSession, filter, update, cancellationToken: cancellationToken);
            }
            else
            {
                await _collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
            }


        }
    }
}
