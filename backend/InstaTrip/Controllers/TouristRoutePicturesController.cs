using AutoMapper;
using InstaTrip.API.Dtos;
using InstaTrip.API.Models;
using InstaTrip.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaTrip.API.Controllers
{
    [Route("api/touristRoutes/{touristRouteId}/pictures")]
    [ApiController]
    public class TouristRoutePicturesController : ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private IMapper _mapper;

        public TouristRoutePicturesController(
            ITouristRouteRepository touristRouteRepository,
            IMapper mapper
        )
        {
            _touristRouteRepository = touristRouteRepository ??
                throw new ArgumentNullException(nameof(touristRouteRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet(Name = "GetPictureListForTouristRoute")]
        public async Task<IActionResult> GetPictureListForTouristRoute(Guid touristRouteId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId))) 
            {
                return NotFound("travel route does not exist");    
            }

            var picturesFromRepo = await _touristRouteRepository.GetPicturesByTouristRouteIdAsync(touristRouteId);
            if(picturesFromRepo==null|| picturesFromRepo.Count() <= 0)
            {
                return NotFound("photo does not exist");
            }

            return Ok(_mapper.Map<IEnumerable<TouristRoutePictureDto>>(picturesFromRepo));

        }

        [HttpGet("{pictureId}", Name = "GetPicture")]
        public async Task<IActionResult> GetPicture(Guid touristRouteId, int pictureId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("travel route does not exist");
            }

            var pictureFromRepo = await _touristRouteRepository.GetPictureAsync(pictureId);
            if (pictureFromRepo == null)
            {
                return NotFound("photo does not exist");
            }
            return Ok(_mapper.Map<TouristRoutePictureDto>(pictureFromRepo));
        }

        [HttpPost(Name = "CreateTouristRoutePicture")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTouristRoutePicture(
            [FromRoute] Guid touristRouteId,
            [FromBody] TouristRoutePictureForCreationDto touristRoutePictureForCreationDto
        )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("travel route does not exist");
            }

            var pictureModel = _mapper.Map<TouristRoutePicture>(touristRoutePictureForCreationDto);
            _touristRouteRepository.AddTouristRoutePicture(touristRouteId, pictureModel);
            await _touristRouteRepository.SaveAsync();
            var pictureToReturn = _mapper.Map<TouristRoutePictureDto>(pictureModel);
            return CreatedAtRoute(
                "GetPicture",
                new
                {
                    touristRouteId = pictureModel.TouristRouteId,
                    pictureId = pictureModel.Id
                },
                pictureToReturn
            );
        }

        [HttpDelete("{pictureId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePicture(
            [FromRoute]Guid touristRouteId, 
            [FromRoute]int pictureId
        )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("travel route does not exist");
            }

            var picture = await _touristRouteRepository.GetPictureAsync(pictureId);
            _touristRouteRepository.DeleteTouristRoutePicture(picture);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }
    }
}
