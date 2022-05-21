using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InstaTrip.API.Dtos;
using InstaTrip.API.Models;

namespace InstaTrip.API.Profiles
{
    public class TouristRoutePictureProfile : Profile
    {
        public TouristRoutePictureProfile()
        {
            CreateMap<TouristRoutePicture, TouristRoutePictureDto>();
            CreateMap<TouristRoutePictureForCreationDto, TouristRoutePicture>();
            CreateMap<TouristRoutePicture, TouristRoutePictureForCreationDto>();
        }
    }
}
