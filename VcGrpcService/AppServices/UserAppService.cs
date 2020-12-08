using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Vc.Domain.Entities;
using Vc.Domain.RepositoryInterfaces;

namespace VcGrpcService.AppServices
{
    /// <summary>
    /// Mainly user related operations
    /// </summary>
    public class UserAppService : AbstractAppService
    {
        private readonly ILogger<UserAppService> _logger;
        private readonly IUserRepository _userRepository;

        public UserAppService(ILogger<UserAppService> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Sync user from provider to database
        /// </summary>
        /// <param name="userClaims">The user claims from provider</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SyncUserAsync(ClaimsPrincipal userClaims, CancellationToken cancellationToken)
        {
            User user = await _userRepository.GetUserAsync(userClaims.FindFirstValue(ClaimTypes.NameIdentifier), cancellationToken);
            if (!CompareUser(userClaims, user))
            {
                user.Username = userClaims.FindFirstValue("name");
                user.Email = userClaims.FindFirstValue(ClaimTypes.Email);
                user.PhotoUrl = userClaims.FindFirstValue("picture");
                await _userRepository.UpdateUserAsync(user.Id, user, cancellationToken);
            }


        }

        public bool CompareUser(ClaimsPrincipal userClaims, User user)
        {
            return userClaims.FindFirstValue("name") == user.Username
                && userClaims.FindFirstValue(ClaimTypes.Email) == user.Email
                && userClaims.FindFirstValue("picture") == user.PhotoUrl;

        }
    }
}
