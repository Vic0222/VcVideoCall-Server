using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Vc.Domain.DataHelper
{
    public interface IDatabaseTransaction : IDisposable
    {
        Task AbortSessionAsync();
        Task CommitSessionAsync();
        Task StartSessionAsync();
    }
}
