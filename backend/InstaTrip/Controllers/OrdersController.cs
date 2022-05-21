using AutoMapper;
using InstaTrip.API.Dtos;
using InstaTrip.API.ResourceParameters;
using InstaTrip.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InstaTrip.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITouristRouteRepository _touristRouteRepository;
        private readonly IMapper _mapper;
        private readonly IHttpClientFactory _httpClientFactory;

        public OrdersController(
            IHttpContextAccessor httpContextAccessor,
            ITouristRouteRepository touristRouteRepository,
            IMapper mapper,
            IHttpClientFactory httpClientFactory
        )
        {
            _httpContextAccessor = httpContextAccessor;
            _touristRouteRepository = touristRouteRepository;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet(Name = "GetOrders")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetOrders(
            [FromQuery] PaginationResourceParamaters paramaters)
        {
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var orders = await _touristRouteRepository.GetOrdersByUserId(
                userId, paramaters.PageSize, paramaters.PageNumber);

            return Ok(_mapper.Map<IEnumerable<OrderDto>>(orders));
        }

        [HttpGet("{orderId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GerOrderById([FromRoute] Guid orderId)
        {
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var order = await _touristRouteRepository.GetOrderById(orderId);

            return Ok(_mapper.Map<OrderDto>(order));
        }


        [HttpPost("{orderId}/placeOrder")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> placeOrder([FromRoute] Guid orderId)
        {
            var userId = _httpContextAccessor
                .HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var order = await _touristRouteRepository.GetOrderById(orderId);
            order.PaymentProcessing();
            await _touristRouteRepository.SaveAsync();

            var httpClient = _httpClientFactory.CreateClient();
            string url = @"https://localhost:3001/api/FakeVanderPaymentProcess?orderNumber={0}&returnFault={1}";
            var response = await httpClient.PostAsync(
                string.Format(url, order.Id, false)
                , null);

            bool isApproved = false;
            string transactionMetadata = "";
            if (response.IsSuccessStatusCode)
            {
                transactionMetadata = await response.Content.ReadAsStringAsync();
                var jsonObject = (JObject)JsonConvert.DeserializeObject(transactionMetadata);
                isApproved = jsonObject["approved"].Value<bool>();
            }

            if (isApproved)
            {
                order.PaymentApprove();
            } 
            else
            {
                order.PaymentReject();
            }
            order.TransactionMetadata = transactionMetadata;
            await _touristRouteRepository.SaveAsync();

            return Ok(_mapper.Map<OrderDto>(order));
        }
    }
}
