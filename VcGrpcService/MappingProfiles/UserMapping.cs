using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vc.Domain.Entities;

namespace VcGrpcService.Mappings
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            //map domain user and proto user
            CreateMap<User, Proto.User>()
                .ForMember(pu => pu.UserId, cfg => cfg.MapFrom(u => u.Id))
                .ReverseMap();
        }
    }
}
