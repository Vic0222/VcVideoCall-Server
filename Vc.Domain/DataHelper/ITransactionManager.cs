using System;
using System.Collections.Generic;
using System.Text;

namespace Vc.Domain.DataHelper
{
    public interface ITransactionManager
    {
        IDatabaseTransaction CreateDatabaseTransaction();
    }
}
