using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vc.Domain.Entities;
using Vc.Domain.RepositoryInterfaces;

namespace VcGrpcService.AppServices
{
    public class ChatAppService
    {
        private ConcurrentDictionary<string, IServerStreamWriter<Notification>> onlineUsers = new ConcurrentDictionary<string, IServerStreamWriter<Notification>>();
        private readonly IRoomRepository _roomRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public ChatAppService(IRoomRepository roomRepository, IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _roomRepository = roomRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }
        
        public void AddOnlineUser(string userId, IServerStreamWriter<Notification> responseStream)
        {
            onlineUsers.TryAdd(userId, responseStream);
        }

        public void RemoveOnlineUser(string userId)
        {
            onlineUsers.TryRemove(userId, out IServerStreamWriter<Notification> stream);
        }

        public async Task BroadcastMessage(MessageRequest messageRequest)
        {
            //test broadcast to all users
            //Notification notification = new Notification() { RoomId = messageRequest.RoomId, Sender = messageRequest.Sender, MessageBody = messageRequest.MessageBody };
            Room room = null;
            // 1 = PM, 2 = Group
            if (messageRequest.Type == 1)
            {
                //target is receiver UserId if type = 1
                room = await _roomRepository.GetIndividualRoomAsync(messageRequest.Sender, messageRequest.Target);

                //create room if not exist
                if (room == null)
                {
                    room = createRoom(messageRequest, RoomType.Private);
                    string roomId = await _roomRepository.AddRoomAsync(room);
                    room.Id = roomId;
                }
            }
            else if(messageRequest.Type == 2)
            {
                //target is roomId if type = 2
                room = await _roomRepository.GetRoomAsync(messageRequest.Target);
            }

            if (room != null)
            {
                await _messageRepository.AddMessageAsync(createMessage(messageRequest));

                List<User> users = _userRepository.GetUsersWithRoomId(room?.Id);
                foreach (var user in users)
                {
                    if (onlineUsers.TryGetValue(user.Id, out IServerStreamWriter<Notification> stream))
                    {
                        await stream.WriteAsync(createNotification(messageRequest));
                    }
                }
            }
        }
        public Room createRoom(MessageRequest messageRequest, RoomType roomType)
        {
            string name = string.Format("{0}-{1}", messageRequest.Sender, messageRequest.Target);
            return new Room() { Name = name, Type = roomType };
        }

        private  Notification createNotification(MessageRequest messageRequest)
        {
            return new Notification() { RoomId = messageRequest.Target, Sender = messageRequest.Sender, MessageBody = messageRequest.MessageBody };
        }

        private Message createMessage(MessageRequest messageRequest)
        {
            return new Message() { DateSent = DateTime.Now, MessageBody = messageRequest.MessageBody, RoomId = messageRequest.Target, UserId = messageRequest.Sender};
        }


    }
}
