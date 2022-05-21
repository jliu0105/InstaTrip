using AutoMapper;
using InstaTrip.API.Dtos;
using InstaTrip.API.Helper;
using InstaTrip.API.Models;
using InstaTrip.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaTrip.API.Controllers
{
    [ApiController]
    [Route("api/shoppingCart")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;

        public ShoppingCartController(
            IHttpContextAccessor httpContextAccessor,
            ITouristRouteRepository touristRouteRepository,
            IMapper mapper
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
        }

        [HttpGet(Name = "GetShoppingCart")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetShoppingCart()
        {
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCart = await _touristRouteRepository.GetShoppingCartByUserId(userId);

            return Ok(_mapper.Map<ShoppingCartDto>(shoppingCart));
        }

        [HttpPost("items")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> AddShoppingCartItem(
            [FromBody] AddShoppingCartItemDto addShoppingCartItemDto
        )
        {
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCart = await _touristRouteRepository
                .GetShoppingCartByUserId(userId);

            var touristRoute = await _touristRouteRepository
                .GetTouristRouteAsync(addShoppingCartItemDto.TouristRouteId);
            if (touristRoute == null)
            {
                return NotFound("travel route does not exist");
            }

            var lineItem = new LineItem()
            {
                TouristRouteId = addShoppingCartItemDto.TouristRouteId,
                ShoppingCartId = shoppingCart.Id,
                OriginalPrice = touristRoute.OriginalPrice,
                DiscountPresent = touristRoute.DiscountPresent
            };

            await _touristRouteRepository.AddShoppingCartItem(lineItem);
            await _touristRouteRepository.SaveAsync();

            return Ok(_mapper.Map<ShoppingCartDto>(shoppingCart));
        }

        [HttpDelete("items/{itemId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> DeleteShoppingCartItem([FromRoute] int itemId)
        {
            var lineItem = await _touristRouteRepository
                .GetShoppingCartItemByItemId(itemId);
            if (lineItem == null)
            {
                return NotFound("cannot find cart item");
            }

            _touristRouteRepository.DeleteShoppingCartItem(lineItem);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpDelete("items/({itemIDs})")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> RemoveShoppingCartItems(
            [ModelBinder(BinderType = typeof(ArrayModelBinder))]
            [FromRoute] IEnumerable<int> itemIDs
        )
        {
            var lineitems = await _touristRouteRepository
                .GeshoppingCartsByIdListAsync(itemIDs);

            _touristRouteRepository.DeleteShoppingCartItems(lineitems);
            await _touristRouteRepository.SaveAsync();

            return NoContent();
        }

        [HttpPost("checkout")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Checkout()
        {
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var shoppingCart = await _touristRouteRepository.GetShoppingCartByUserId(userId);

            var order = new Order()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                State = OrderStateEnum.Pending,
                OrderItems = shoppingCart.ShoppingCartItems,
                CreateDateUTC = DateTime.UtcNow,
            };

            shoppingCart.ShoppingCartItems = null;

            await _touristRouteRepository.AddOrderAsync(order);
            await _touristRouteRepository.SaveAsync();

            return Ok(_mapper.Map<OrderDto>(order));
        }
    }
}
