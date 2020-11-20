using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Vc.DAL.Mongo.Collections;

namespace Vc.DAL.Mongo.Repositories
{
    public abstract class AbstractRepository<T> where T : AbstractCollection
    {
        protected readonly ClientManager _clientManager;
        protected readonly IMapper _mapper;
        protected readonly string _collectionName;
        protected readonly IMongoCollection<T> _collection;

        public AbstractRepository(ClientManager clientManager, IMapper mapper, string collectionName)
        {
            _clientManager = clientManager;
            _mapper = mapper;
            _collectionName = collectionName;
            _collection = _clientManager.GetDatabase().GetCollection<T>(collectionName);
        }
    }
}
