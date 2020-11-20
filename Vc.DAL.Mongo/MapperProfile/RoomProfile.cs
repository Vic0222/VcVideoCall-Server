using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using Dom = Vc.Domain.Entities;
using Dal = Vc.DAL.Mongo.Collections;

namespace Vc.DAL.Mongo.MapperProfile
{
    public class RoomProfile: Profile
    {
        public RoomProfile()
        {
            CreateMap<Dal.Room, Dom.Room>().ReverseMap();
            //CreateMap<Dom.Room, Dal.Room>();

        }
    }
}
