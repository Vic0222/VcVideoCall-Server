using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.DAL.Mongo.Repositories
{
    public abstract class AbstractRepository
    {
        protected readonly ClientManager _clientManager;

        public AbstractRepository(ClientManager clientManager)
        {
            _clientManager = clientManager;
        }
    }
}
