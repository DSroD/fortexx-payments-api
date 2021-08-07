using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Microsoft.EntityFrameworkCore;

using Fortexx.Data;
using Fortexx.Services;
using Fortexx.Models;
using Fortexx.Models.Api;

namespace Fortexx.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase {

        private readonly ILogger<PaymentController> _logger;
        private readonly IPaymentContext _context;
        private readonly IAuthorizationService _authSrv;
        private readonly IDtoService<Payment, PaymentDto> _paymentDtoSrv;
        private readonly IDtoService<GameServer, GameServerDto> _gameServerDtoSrv;
        private readonly IDtoService<Product, ProductDto> _productDtoSrv;

        public PaymentController(ILogger<PaymentController> logger,
                                IPaymentContext context, 
                                IAuthorizationService authSrv, 
                                IDtoService<Payment, PaymentDto> paymentDtoService,
                                IDtoService<GameServer, GameServerDto> gameServerDtoService,
                                IDtoService<Product, ProductDto> productDtoService) {
            _logger = logger;
            _context = context;
            _authSrv = authSrv;
            _paymentDtoSrv = paymentDtoService;
            _gameServerDtoSrv = gameServerDtoService;
            _productDtoSrv = productDtoService;
        }
        #region payments endpoints
        /// <summary>
        /// Get last 20 payment records
        /// </summary>
        /// <returns>Returns last 20 payments</returns>
        /// <param name = "key">Key for accessing/modifying payment records</param>
        /// <response code="200">Last 20 payments</response>
        /// <response code="403">Provided key is not valid</response>
        [HttpGet("{key}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsAsync(string key) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var paymentsList = await _context.GetLastPaymentsAsync(20);
            var rt = paymentsList.Select(p => _paymentDtoSrv.GetDto(p)).ToList<PaymentDto>();
            return rt;
        }

        /// <summary>
        /// Get page (page) with (show_num) payment records
        /// </summary>
        /// <returns>Page (page) with (show_num) payment records</returns>
        /// <param name = "key">Key for accessing/modifying payment records</param>
        /// <param name="show_num">Number of records per page</param>
        /// <param name = "page">Page number</param>
        /// <response code="200">(show_num) payments</response>
        /// <response code="403">Provided key is not valid</response>
        [HttpGet("{key}/show/{show_num}/page/{page}/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPageOfNPayments(string key, int show_num, int page) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var paymentsList = await _context.GetNumPaymentsPageAsync(show_num, page);
            var rt = paymentsList.Select(p => _paymentDtoSrv.GetDto(p)).ToList<PaymentDto>();
            return rt; 
            
        }

        /// <summary>
        /// Get payment record by id
        /// </summary>
        /// <returns>Payment record</returns>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="id">Id of record</param>
        /// <response code="200">Payment record</response>
        /// <response code="403">Provided key is not valid</response>
        /// <response code="404">Payment with provided id not found</response>
        [HttpGet("{key}/id/{id}", Name = "GetId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> GetPaymentByID(string key, int id) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var payment = await _context.GetPaymentByIdAsync(id);
            if(payment == null) {
                return NotFound();
            }
            return _paymentDtoSrv.GetDto(payment);
        }


        /// <summary>
        /// Get payments of a User by username
        /// </summary>
        /// <returns>Payment records</returns>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="name">Username</param>
        /// <response code="200">Payment record</response>
        /// <response code="403">Provided key is not valid</response>
        [HttpGet("{key}/name/{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByName(string key, string name) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var result = await _context.GetPaymentsByNameAsync(name);
            var rt = result.Select(p => _paymentDtoSrv.GetDto(p)).ToList<PaymentDto>();
            return rt;
        }

        /// <summary>
        /// Create payment record from SMS Payment
        /// </summary>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="smspayment">SMS Payment data</param>
        /// <returns>A newly created payment record</returns>
        /// <response code="201">Returns if succesfully created</response>
        /// <response code="403">Provided key is not valid</response>
        [HttpPost("{key}/sms")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaymentDto>> PostSMSPaymentAsync(string key, SMSPayment smspayment) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            Payment p = new Payment() {
                PaymentId = smspayment.Id,
                PaymentDate = DateTime.Now,
                PaymentType = "SMS",
                Value = 0,
                Currency = "CZK",
                User = "",
                Server = null,
                Product = null,
                MainInfo = smspayment.Sms,
                OtherInfo = "NONE but OTHER",
                Status = "WAITING",
                Activated = false
            };
            await _context.AddPaymentAsync(p);
            return CreatedAtRoute("GetId", new {Key = key, Id = p.Id}, _paymentDtoSrv.GetDto(p));
        }

        /// <summary>
        /// Create payment record from Informant
        /// </summary>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="informant">Informant data</param>
        /// <returns>A newly created payment record</returns>
        /// <response code="201">Returns if succesfully created</response>
        /// <response code="403">Provided key is not valid</response>
        [HttpPost("{key}/informant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PaymentDto>> PostInformantAsync(string key, Informant informant) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var product = (informant.ProductId != null) ? await _context.GetProductByIdAsync(informant.ProductId ?? 0) : null;
            Payment p = new Payment() {
                    PaymentId = informant.Id,
                    PaymentDate = DateTime.Now,
                    PaymentType = informant.Type,
                    Value = 0,
                    Currency = "---",
                    User = informant.Nickname,
                    ServerId = product?.Result?.GameServerId ?? null,
                    ProductId = product?.Result?.Id ?? null,
                    MainInfo = "",
                    OtherInfo = informant.Info,
                    Status = "REQUIRES CONFIRMATION",
                    Activated = false
                };
            await _context.AddPaymentAsync(p);
            return CreatedAtRoute("GetId", new {Key = key, Id = p.Id}, _paymentDtoSrv.GetDto(p));
        }

        /// <summary>
        /// Sets payment record as active
        /// </summary>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="id">Id of record</param>
        /// <returns>Updated payment record</returns>
        /// <response code="200">Payment succesfully activated</response>
        /// <response code="304">Payment was already active</response>
        /// <response code="403">Provided key is not valid</response>
        /// <response code="404">Payment with provided id not found</response>
        [HttpPut("{key}/activate/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentDto>> SetActivated(string key, int id) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }

            ActivatePaymentResult apr = await _context.ActivatePaymentAsync(id);

            if(apr.Response == ActivatePaymentResponse.NOT_FOUND) {
                return NotFound();
            }

            if (apr.Response == ActivatePaymentResponse.ALREADY_ACTIVE) {
                return StatusCode(304);
            }

            if (apr.Response == ActivatePaymentResponse.ACTIVATED) {
                return _paymentDtoSrv.GetDto(apr.Result);
            }

            else {
                return StatusCode(501);
            }
        }
        #endregion

        #region servers endpoints
        
        [HttpPost("/Servers/{key}/create")]
        public async Task<ActionResult<GameServerDto>> CreateGameServer(string key, GameServerDto serverdto) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var server = _gameServerDtoSrv.GetModel(serverdto);
            await _context.AddGameServerAsync(server);
            return CreatedAtRoute("GetServerId", new {Key = key, Id = server.Id}, _gameServerDtoSrv.GetDto(server));
        }

        [HttpPut("/Servers/{key}/update")]
        public async Task<ActionResult<GameServerDto>> UpdateServer(string key, GameServerDto updates) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var exists = await _context.GetGameServerByIdAsync(updates.Id);
            if (exists.Response == GetObjectResponse.NOT_FOUND) {
                return StatusCode(404);
            }
            else
            {
                await _context.UpdateGameServerAsync(updates);
                exists = await _context.GetGameServerByIdAsync(updates.Id);
                return _gameServerDtoSrv.GetDto(exists.Result);
            }

        }

        [HttpGet("/Servers/{key}/list")]
        public async Task<ActionResult<IEnumerable<GameServerDto>>> GetServerList(string key) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }
            var servers = await _context.GetGameServersAsync();
            var rt = servers.Select(s => _gameServerDtoSrv.GetDto(s)).ToList<GameServerDto>();
            return rt;
        }

        [HttpGet("/Servers/{key}/id/{id}", Name = "GetServerId")]
        public async Task<ActionResult<GameServerDto>> GetGameServerById(string key, int id) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }
            var server = await _context.GetGameServerByIdAsync(id);
            if (server.Response == GetObjectResponse.NOT_FOUND) {
                return StatusCode(404);
            }
            return _gameServerDtoSrv.GetDto(server.Result);
        }
        #endregion

        #region products endpoints

        [HttpPost("/Products/{key}/create")]
        public async Task<ActionResult<ProductDto>> CreateProduct(string key, ProductDto productdto) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var server = await _context.GetGameServerByIdAsync(productdto.GameServerId);
            if(server.Response == GetObjectResponse.NOT_FOUND) {
                return StatusCode(409);
            }
            var product = _productDtoSrv.GetModel(productdto);
            await _context.AddProductAsync(product);
            int id = product.Id;
            var product_res = await _context.GetProductByIdAsync(id);
            product = product_res.Result;
            return CreatedAtRoute("GetProductId", new {Key = key, Id = product.Id}, _productDtoSrv.GetDto(product));
        }

        [HttpGet("/Products/{key}/id/{id}", Name = "GetProductId")]
        public async Task<ActionResult<ProductDto>> GetProductById(string key, int id) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }
            var product = await _context.GetProductByIdAsync(id);
            if(product.Response == GetObjectResponse.NOT_FOUND) {
                return StatusCode(404);
            }
            return _productDtoSrv.GetDto(product.Result);
        }

        [HttpGet("/Products/{key}/server/{serverId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetServerProducts(string key, int serverId) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }
            var server = await _context.GetGameServerByIdAsync(serverId);
            if(server.Response == GetObjectResponse.NOT_FOUND) {
                return StatusCode(404);
            }
            var products = await _context.GetServerProductsAsync(serverId);
            var rt = products.Select(p => _productDtoSrv.GetDto(p)).ToList<ProductDto>();
            return rt;
        }
        #endregion
    }   

}