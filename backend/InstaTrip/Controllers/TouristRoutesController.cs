﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaTrip.API.Dtos;
using InstaTrip.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Text.RegularExpressions;
using InstaTrip.API.ResourceParameters;
using InstaTrip.API.Models;
using Microsoft.AspNetCore.JsonPatch;
using InstaTrip.API.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using System.Dynamic;

namespace InstaTrip.API.Controllers
{
    [Route("api/[controller]")] // api/touristroute
    [ApiController]
    public class TouristRoutesController : ControllerBase
    {
        private ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;
        private readonly IPropertyMappingService _propertyMappingService;


        public TouristRoutesController(
            ITouristRouteRepository touristRouteRepository,
            IMapper mapper,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IPropertyMappingService propertyMappingService
        )
        {
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _propertyMappingService = propertyMappingService;
        }

        private string GenerateTouristRouteResourceURL(
            TouristRouteResourceParamaters paramaters,
            PaginationResourceParamaters paramaters2,
            ResourceUriType type
        )
        {
            return type switch
            {
                ResourceUriType.PreviousPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber - 1,
                        pageSize = paramaters2.PageSize
                    }),
                ResourceUriType.NextPage => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber + 1,
                        pageSize = paramaters2.PageSize
                    }),
                _ => _urlHelper.Link("GetTouristRoutes",
                    new
                    {
                        fields = paramaters.Fields,
                        orderBy = paramaters.OrderBy,
                        keyword = paramaters.Keyword,
                        rating = paramaters.Rating,
                        pageNumber = paramaters2.PageNumber,
                        pageSize = paramaters2.PageSize
                    })
            };
        }

        [Produces(
            "application/json",
            "application/vnd.aleks.hateoas+json",
            "application/vnd.aleks.touristRoute.simplify+json",
            "application/vnd.aleks.touristRoute.simplify.hateoas+json"
            )]
        [HttpGet(Name = "GetTouristRoutes")]
        [HttpHead]
        public async Task<IActionResult> GerTouristRoutes(
            [FromQuery] TouristRouteResourceParamaters paramaters,
            [FromQuery] PaginationResourceParamaters paramaters2,
            [FromHeader(Name = "Accept")] string mediaType
        //[FromQuery] string keyword,
        )// FromQuery vs FromBody
        {
            if(!MediaTypeHeaderValue
                .TryParse(mediaType, out MediaTypeHeaderValue parsedMediatype))
            {
                return BadRequest();
            }

            if (!_propertyMappingService
                .IsMappingExists<TouristRouteDto, TouristRoute>(
                paramaters.OrderBy))
            {
                return BadRequest("Please insert correct parameter");
            }

            if (!_propertyMappingService
                .IsPropertiesExists<TouristRouteDto>(paramaters.Fields))
            {
                return BadRequest("Please insert correct parameter");
            }

            var touristRoutesFromRepo = await _touristRouteRepository
                .GetTouristRoutesAsync(
                    paramaters.Keyword, 
                    paramaters.RatingOperator, 
                    paramaters.RatingValue,
                    paramaters2.PageSize,
                    paramaters2.PageNumber,
                    paramaters.OrderBy
                );
            if (touristRoutesFromRepo == null || touristRoutesFromRepo.Count() <= 0)
            {
                return NotFound("No travel route found");
            }

            var previousPageLink = touristRoutesFromRepo.HasPrevious
                ? GenerateTouristRouteResourceURL(
                    paramaters, paramaters2, ResourceUriType.PreviousPage)
                : null;

            var nextPageLink = touristRoutesFromRepo.HasNext
                ? GenerateTouristRouteResourceURL(
                    paramaters, paramaters2, ResourceUriType.NextPage)
                : null;

            // x-pagination
            var paginationMetadata = new
            {
                previousPageLink,
                nextPageLink,
                totalCount = touristRoutesFromRepo.TotalCount,
                pageSize = touristRoutesFromRepo.PageSize,
                currentPage = touristRoutesFromRepo.CurrentPage,
                totalPages = touristRoutesFromRepo.TotalPages
            };

            Response.Headers.Add("x-pagination",
                Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            bool isHateoas = parsedMediatype.SubTypeWithoutSuffix
                .EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);

            var primaryMediaType = isHateoas
                ? parsedMediatype.SubTypeWithoutSuffix
                    .Substring(0, parsedMediatype.SubTypeWithoutSuffix.Length - 8)
                : parsedMediatype.SubTypeWithoutSuffix;

            //var touristRoutesDto = _mapper.Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
            //var shapedDtoList = touristRoutesDto.ShapeData(paramaters.Fields);
            IEnumerable<object> touristRoutesDto;
            IEnumerable<ExpandoObject> shapedDtoList;

            if (primaryMediaType == "vnd.aleks.touristRoute.simplify")
            {
                touristRoutesDto = _mapper
                    .Map<IEnumerable<TouristRouteSimplifyDto>>(touristRoutesFromRepo);

                shapedDtoList = ((IEnumerable<TouristRouteSimplifyDto>)touristRoutesDto)
                    .ShapeData(paramaters.Fields);
            }
            else
            {
                touristRoutesDto = _mapper
                    .Map<IEnumerable<TouristRouteDto>>(touristRoutesFromRepo);
                shapedDtoList =
                    ((IEnumerable<TouristRouteDto>)touristRoutesDto)
                    .ShapeData(paramaters.Fields);
            }

            if (isHateoas) 
            {
                var linkDto = CreateLinksForTouristRouteList(paramaters, paramaters2);

                var shapedDtoWithLinklist = shapedDtoList.Select(t =>
                {
                    var touristRouteDictionary = t as IDictionary<string, object>;
                    var links = CreateLinkForTouristRoute(
                        (Guid)touristRouteDictionary["Id"], null);
                    touristRouteDictionary.Add("links", links);
                    return touristRouteDictionary;
                });

                var result = new
                {
                    value = shapedDtoWithLinklist,
                    links = linkDto
                };

                return Ok(result);
            }

            return Ok(shapedDtoList);
        }

        private IEnumerable<LinkDto> CreateLinksForTouristRouteList(
            TouristRouteResourceParamaters paramaters,
            PaginationResourceParamaters paramaters2)
        {
            var links = new List<LinkDto>();
            // 添加self，自我链接
            links.Add(new LinkDto(
                    GenerateTouristRouteResourceURL(
                        paramaters, paramaters2, ResourceUriType.CurrnetPage),
                    "self",
                    "GET"
                ));

            // "api/touristRoutes"
            links.Add(new LinkDto(
                    Url.Link("CreateTouristRoute", null),
                    "create_tourist_route",
                    "POST"
                ));

            return links;
        }

        // api/touristroutes/{touristRouteId}
        [HttpGet("{touristRouteId}", Name = "GetTouristRouteById")]
        public async Task<IActionResult> GetTouristRouteById(
            Guid touristRouteId,
            string fields)
        {
            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            if (touristRouteFromRepo == null)
            {
                return NotFound($"旅游路线{touristRouteId}找不到");
            }
            //var touristRouteDto = new TouristRouteDto()
            //{
            //    Id = touristRouteFromRepo.Id,
            //    Title = touristRouteFromRepo.Title,
            //    Description = touristRouteFromRepo.Description,
            //    Price = touristRouteFromRepo.OriginalPrice * (decimal)(touristRouteFromRepo.DiscountPresent ?? 1),
            //    CreateTime = touristRouteFromRepo.CreateTime,
            //    UpdateTime = touristRouteFromRepo.UpdateTime,
            //    Features = touristRouteFromRepo.Features,
            //    Fees = touristRouteFromRepo.Fees,
            //    Notes = touristRouteFromRepo.Notes,
            //    Rating = touristRouteFromRepo.Rating,
            //    TravelDays = touristRouteFromRepo.TravelDays.ToString(),
            //    TripType = touristRouteFromRepo.TripType.ToString(),
            //    DepartureCity = touristRouteFromRepo.DepartureCity.ToString()
            //};
            var touristRouteDto = _mapper.Map<TouristRouteDto>(touristRouteFromRepo);
            //return Ok(touristRouteDto.ShapeData(fields));

            var linkDtos = CreateLinkForTouristRoute(touristRouteId, fields);

            var result = touristRouteDto.ShapeData(fields)
                as IDictionary<string, object>;
            result.Add("links", linkDtos);

            return Ok(result);
        }

        private IEnumerable<LinkDto> CreateLinkForTouristRoute(
            Guid touristRouteId,
            string fields)
        {
            var links = new List<LinkDto>();

            links.Add(
                new LinkDto(
                    Url.Link("GetTouristRouteById", new { touristRouteId, fields }),
                    "self",
                    "GET"
                    )
                );

            links.Add(
                new LinkDto(
                    Url.Link("UpdateTouristRoute", new { touristRouteId }),
                    "update",
                    "PUT"
                    )
                );

            links.Add(
                new LinkDto(
                    Url.Link("PartiallyUpdateTouristRoute", new { touristRouteId }),
                    "partially_update",
                    "PATCH")
                );

            links.Add(
                new LinkDto(
                    Url.Link("DeleteTouristRoute", new { touristRouteId }),
                    "delete",
                    "DELETE")
                );

            links.Add(
                new LinkDto(
                    Url.Link("GetPictureListForTouristRoute", new { touristRouteId }),
                    "get_pictures",
                    "GET")
                );

            links.Add(
                new LinkDto(
                    Url.Link("CreateTouristRoutePicture", new { touristRouteId }),
                    "create_picture",
                    "POST")
                );

            return links;
        }

        [HttpPost(Name = "CreateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize]
        public async Task<IActionResult> CreateTouristRoute([FromBody] TouristRouteForCreationDto touristRouteForCreationDto)
        {
            var touristRouteModel = _mapper.Map<TouristRoute>(touristRouteForCreationDto);
            _touristRouteRepository.AddTouristRoute(touristRouteModel);
            await _touristRouteRepository.SaveAsync();
            var touristRouteToReture = _mapper.Map<TouristRouteDto>(touristRouteModel);

            var links = CreateLinkForTouristRoute(touristRouteModel.Id, null);

            var result = touristRouteToReture.ShapeData(null)
                as IDictionary<string, object>;

            result.Add("links", links);
            
            return CreatedAtRoute(
                "GetTouristRouteById",
                new { touristRouteId = result["Id"] },
                result
            );
        }

        [HttpPut("{touristRouteId}", Name = "UpdateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTouristRoute(
            [FromRoute]Guid touristRouteId,
            [FromBody] TouristRouteForUpdateDto touristRouteForUpdateDto
        )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Travel root cannot be found");
            }

            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            _mapper.Map(touristRouteForUpdateDto, touristRouteFromRepo);

            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{touristRouteId}", Name = "PartiallyUpdateTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PartiallyUpdateTouristRoute(
            [FromRoute]Guid touristRouteId,
            [FromBody] JsonPatchDocument<TouristRouteForUpdateDto> patchDocument
        )
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Travel root cannot be found");
            }

            var touristRouteFromRepo = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            var touristRouteToPatch = _mapper.Map<TouristRouteForUpdateDto>(touristRouteFromRepo);
            patchDocument.ApplyTo(touristRouteToPatch, ModelState);
            if (!TryValidateModel(touristRouteToPatch))
            {
                return ValidationProblem(ModelState);
            }
            _mapper.Map(touristRouteToPatch, touristRouteFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("{touristRouteId}", Name = "DeleteTouristRoute")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTouristRoute([FromRoute] Guid touristRouteId)
        {
            if (!(await _touristRouteRepository.TouristRouteExistsAsync(touristRouteId)))
            {
                return NotFound("Travel root cannot be found");
            }

            var touristRoute = await _touristRouteRepository.GetTouristRouteAsync(touristRouteId);
            _touristRouteRepository.DeleteTouristRoute(touristRoute);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("({touristIDs})")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteByIDs(
            [ModelBinder( BinderType = typeof(ArrayModelBinder))][FromRoute]IEnumerable<Guid> touristIDs)
        {
            if(touristIDs == null)
            {
                return BadRequest();
            }

            var touristRoutesFromRepo = await _touristRouteRepository.GetTouristRoutesByIDListAsync(touristIDs);
            _touristRouteRepository.DeleteTouristRoutes(touristRoutesFromRepo);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }
    }
}