using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Dom = Vc.Domain.Entities;
using Dal = Vc.DAL.Mongo.Collections;

namespace Vc.DAL.Mongo.MapperProfile
{
    public class MessageProfile: Profile
    {
        public MessageProfile()
        {
            CreateMap<Dal.Message, Dom.Message>().ReverseMap();
        }
    }
}
