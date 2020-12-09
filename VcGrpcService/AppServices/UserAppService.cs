using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Vc.Common;
using Vc.Domain.DataHelper;
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
        private readonly IRoomRepository _roomRepository;
        private readonly ITransactionManager _transactionManager;

        public UserAppService(ILogger<UserAppService> logger, IUserRepository userRepository, IRoomRepository roomRepository, ITransactionManager transactionManager)
        {
            _logger = logger;
            _userRepository = userRepository;
            _roomRepository = roomRepository;
            _transactionManager = transactionManager;
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

            using (var transaction = _transactionManager.CreateDatabaseTransaction())
            {
                try
                {
                    bool hasChanges = false;
                    await transaction.StartSessionAsync();
                    //check photo url
                    if (userClaims.FindFirstValue("picture").IsNotNull() && userClaims.FindFirstValue("picture") != user.PhotoUrl)
                    {
                        await _userRepository.UpdateUserPhotoUrlAsync(user.Id, userClaims.FindFirstValue("picture"), cancellationToken);
                        await _roomRepository.UpdateRoomUserPhotoUrl(user.Id, userClaims.FindFirstValue("picture"), cancellationToken);
                        hasChanges = true;
                    }

                    if (user != null && !CompareUser(userClaims, user))
                    {
                        user.Username = userClaims.FindFirstValue("name");
                        user.Email = userClaims.FindFirstValue(ClaimTypes.Email);
                        await _userRepository.UpdateUserAsync(user.Id, user, cancellationToken);
                        hasChanges = true;

                    }
                    if (hasChanges)
                    {
                        await transaction.CommitSessionAsync();
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "User sync error. Rolling back");
                    await transaction.AbortSessionAsync();
                    
                    throw;
                }
            }
            
           
        }

        public bool CompareUser(ClaimsPrincipal userClaims, User user)
        {
            return userClaims.FindFirstValue("name") == user.Username
                && userClaims.FindFirstValue(ClaimTypes.Email) == user.Email;

        }
    }
}
