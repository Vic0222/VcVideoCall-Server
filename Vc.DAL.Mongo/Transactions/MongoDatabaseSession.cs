using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vc.Domain.DataHelper;

namespace Vc.DAL.Mongo
{
    public class MongoDatabaseSession : IDatabaseTransaction
    {
        private ClientManager _clientManager;
        private IClientSessionHandle _currentSession;
        public IClientSessionHandle CurrentSession { get => _currentSession;  }

        public MongoDatabaseSession(ClientManager clientManager)
        {
            _clientManager = clientManager;
            
        }

        public async Task StartSessionAsync()
        {
            _currentSession = await _clientManager.DefaultMongoClient.StartSessionAsync();
        }

        public async Task CommitSessionAsync()
        {
            await _currentSession.CommitTransactionAsync();
        }

        public async Task AbortSessionAsync()
        {
            _currentSession = await _clientManager.DefaultMongoClient.StartSessionAsync();
        }

        public void Dispose()
        {
            _currentSession.Dispose();
            _currentSession = null;
        }
    }
}
