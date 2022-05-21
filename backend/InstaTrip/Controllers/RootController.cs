using InstaTrip.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaTrip.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RootController: ControllerBase
    {
        [HttpGet(Name = "GetRoot")]
        public IActionResult GetRoot()
        {
            var links = new List<LinkDto>();

            links.Add(
                new LinkDto(
                    Url.Link("GetRoot", null),
                    "self",
                    "GET"
                ));

            links.Add(
                new LinkDto(
                    Url.Link("GetTouristRoutes", null),
                    "get_tourist_routes",
                    "GET"
                ));

            links.Add(
                new LinkDto(
                    Url.Link("CreateTouristRoute", null),
                    "create_tourist_route",
                    "POST"
                ));

            links.Add(
                new LinkDto(
                    Url.Link("GetShoppingCart", null),
                    "get_shopping_cart",
                    "GET"
                ));

            links.Add(
                new LinkDto(
                    Url.Link("GetOrders", null),
                    "get_orders",
                    "GET"
                ));

            return Ok(links);
        }
    }
}
