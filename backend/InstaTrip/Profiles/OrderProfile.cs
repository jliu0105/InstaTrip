using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InstaTrip.API.Dtos;
using InstaTrip.API.Models;

namespace InstaTrip.API.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(
                    dest => dest.State,
                    opt =>
                    {
                        opt.MapFrom(src => src.State.ToString());
                    }
                );
        }
    }
}
