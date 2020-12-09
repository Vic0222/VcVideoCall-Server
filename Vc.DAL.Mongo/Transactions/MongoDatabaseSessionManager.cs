using System;
using System.Collections.Generic;
using System.Text;
using Vc.Domain.DataHelper;

namespace Vc.DAL.Mongo.Transactions
{
    public class MongoDatabaseSessionManager : ITransactionManager
    {
        private readonly ClientManager _clientManager;
        private MongoDatabaseSession _mongoDatabaseSession;
        public MongoDatabaseSession MongoDatabaseSession { get => _mongoDatabaseSession; }

        public MongoDatabaseSessionManager(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }
        public IDatabaseTransaction CreateDatabaseTransaction()
        {
            _mongoDatabaseSession = new MongoDatabaseSession(_clientManager);
            return _mongoDatabaseSession;
        }
    }
}
