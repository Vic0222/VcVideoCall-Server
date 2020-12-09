using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vc.Domain.Entities;

namespace Vc.Domain.RepositoryInterfaces
{
    public interface IUserRepository : IRepository
    {
        Task<User> GetUserAsync(string sender, CancellationToken cancellationToken = default);
        Task UpdateUserAsync(string id, User user, CancellationToken cancellationToken = default);
        Task UpdateUserPhotoUrlAsync(string id, string photoUrl, CancellationToken cancellationToken = default);
        Task<List<User>> GetUsersUsingKeywordAsync(string keyword, CancellationToken cancellationToken = default);
    }
}
