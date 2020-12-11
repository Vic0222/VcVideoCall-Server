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
            if (_clientManager.DefaultMongoClient.Cluster.Description.Type == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet)
            {
                _currentSession = await _clientManager.DefaultMongoClient.StartSessionAsync();

                _currentSession.StartTransaction();
            }
            
        }

        public async Task CommitSessionAsync()
        {
            if (_currentSession != null)
            {
                await _currentSession.CommitTransactionAsync();
            }
            
        }

        public async Task AbortSessionAsync()
        {
            if (_currentSession != null)
            {
                await _currentSession.AbortTransactionAsync();
            }
        }

        public void Dispose()
        {
            if (_currentSession != null)
            {
                _currentSession.Dispose();
                _currentSession = null;
            }
        }
    }
}
