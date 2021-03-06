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
        /// Create payment record from SMS Payment (mobilniplatby.cz)
        /// </summary>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="payment">SMS Payment data (from query)</param>
        /// <returns>A newly created payment record</returns>
        /// <response code="200">Returns if succesfully created</response>
        /// <response code="204">Acknowledges payment delivery message</response>
        /// <response code="403">Provided key is not valid</response>
        /// <response code="404">Payment not found, delivery message has no meaning</payment>
        [HttpGet("{key}/sms")]
        [Consumes("text/plain")] 
        [Produces("text/plain")]
        public async Task<ActionResult<string>> PostSMSPaymentAsync(string key, [FromQuery] SMSPayment payment) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }

            if (payment.Shortcode != null || payment.Sms != null) {
                string[] smsParts = payment.Sms.Split(" ");
                string value = smsParts[2];
                string nickname = smsParts[3];
                string productCode = smsParts[4];
                string serverCode = smsParts[5];

                float valuef;

                string currency = (payment.Shortcode.ToString().Length == 4) ? "EUR" : "CZK";

                var product = await _context.GetProductByCodenames(productCode, serverCode);

                var valueString = (currency == "EUR") ? ((value.Length < 2) ? "0" + value + "00" : value + "00") : (value);

                if(product == null || !float.TryParse(value, out valuef)) {
                    _logger.LogInformation(string.Format("{0} send wrong SMS code {1}!", nickname, payment.Sms));
                    var strer = "Zaslany kod neni platny;FREE" + payment.Shortcode + ((currency == "EUR") ? "" : valueString);
                    Response.Headers.Add("Content-Length", System.Text.ASCIIEncoding.ASCII.GetByteCount(strer).ToString());
                    return strer;
                }

                DateTime dt;

                DateTime.TryParseExact(payment.Timestamp, "s", System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.AssumeUniversal, out dt);

                Payment p = new Payment {
                    PaymentId = payment.Id,
                    PaymentDate = dt,
                    PaymentType = "SMS",
                    Value = valuef,
                    Currency = currency,
                    User = nickname,
                    ServerId = product.GameServerId,
                    ProductId = product.Id,
                    MainInfo = string.Format("Sms: {0}; Country: {1}; Shortcode: {2}", payment.Sms, payment.Country, payment.Shortcode),
                    OtherInfo = string.Format("Operator: {0}; Phone: {1}", payment.Operator, payment.Phone),
                    Status = "PAYMENT REQUESTED",
                    Activated = false,
                };
                await _context.AddPaymentAsync(p);
                var str = "Dekujeme za SMS;" + payment.Shortcode + valueString;
                Response.Headers.Add("Content-Length", System.Text.ASCIIEncoding.ASCII.GetByteCount(str).ToString());
                return str;
            }
            else if(payment.Request != null) { //This kind of violates ORP, rewrite...
                var pmt = await _context.GetPaymentByPaymentIdAsync(payment.Request ?? 0);
                if (pmt == null || pmt?.PaymentType != "SMS" || pmt?.Status != "PAYMENT REQUIRED") {
                    return NotFound();
                }
                pmt.Status = payment.Status;
                Console.WriteLine(payment.Status);
                if(pmt.Status == "UNDELIVERED") {
                    pmt.OtherInfo += string.Format("; Reason: {0}", payment.Message);
                }
                pmt.OtherInfo += string.Format("; DeliveryId: {0}; DeliveryDate: {1}", payment.Id, payment.Timestamp);
                await _context.SaveDbChangesAsync();
                return NoContent();
            }
            else
            {
                return BadRequest();
            }

            
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
        
        /// <summary>
        /// Create GameServer record
        /// </summary>
        /// <returns>Newly created GameServer</returns>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="serverdto">ServerDto of GameServer to be created</param>
        /// <response code="200">Server created</response>
        /// <response code="403">Provided key is not valid</response>
        [HttpPost("/Servers/{key}/create")]
        public async Task<ActionResult<GameServerDto>> CreateGameServer(string key, GameServerDto serverdto) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            var server = _gameServerDtoSrv.GetModel(serverdto);
            await _context.AddGameServerAsync(server);
            return CreatedAtRoute("GetServerId", new {Key = key, Id = server.Id}, _gameServerDtoSrv.GetDto(server));
        }

        /// <summary>
        /// Updates server record based on id in passed Dto
        /// </summary>
        /// <returns>Updated server record</returns>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="updates">GameServerDto with updated record</param>
        /// <response code="200">Server record updated</response>
        /// <response code="403">Provided key is not valid</response>
        /// <response code="404">Server record not found, create it first</response>
        [HttpPut("/Servers/{key}/update")]
        public async Task<ActionResult<GameServerDto>> UpdateServer(string key, GameServerDto updates) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            if(await _context.UpdateGameServerAsync(updates)){
                var rs = await _context.GetGameServerByIdAsync(updates.Id);
                return _gameServerDtoSrv.GetDto(rs.Result);
            }
            else {
                return NotFound();
            }
        }

        /// <summary>
        /// Deletes server record
        /// </summary>
        /// <returns>Nothing</returns>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="id">Id of server that should be deleted</param>
        /// <response code="204">Server successfuly deleted</response>
        /// <response code="403">Provided key is not valid</response>
        /// <response code="404">Server record not found</response>
        [HttpDelete("/Servers/{key}/delete/{id}")]
        public async Task<ActionResult> DeleteServer(string key, int id) {
            if(!_authSrv.HasSuperUserView(key)) {
                return StatusCode(403);
            }
            if(await _context.DeleteGameServerAsync(id)) {
                return NoContent();
            }
            return NotFound();
        }

        /// <summary>
        /// Return all servers from database (up to 1024)
        /// </summary>
        /// <returns>All server records</returns>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <response code="200">Server records</response>
        /// <response code="403">Provided key is not valid</response>
        [HttpGet("/Servers/{key}/list")]
        public async Task<ActionResult<IEnumerable<GameServerDto>>> GetServerList(string key) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }
            var servers = await _context.GetGameServersAsync();
            var rt = servers.Select(s => _gameServerDtoSrv.GetDto(s)).ToList<GameServerDto>();
            return rt;
        }
        
        /// <summary>
        /// Returns game server by Id
        /// </summary>
        /// <returns>GameServer record</returns>
        /// <param name="key">Key for accessing/modifying payment records</param>
        /// <param name="id">GameServer Id</param>
        /// <response code="200">Server record</response>
        /// <response code="403">Provided key is not valid</response>
        /// <response code="404">Server record not found</response>
        [HttpGet("/Servers/{key}/id/{id}", Name = "GetServerId")]
        public async Task<ActionResult<GameServerDto>> GetGameServerById(string key, int id) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }
            var server = await _context.GetGameServerByIdAsync(id);
            if (server.Response == GetObjectResponse.NOT_FOUND) {
                return NotFound();
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

        [HttpPut("/Products/{key}/update")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(string key, ProductDto updates) {
            if(!_authSrv.HasFullView(key)) {
                return StatusCode(403);
            }
            if(await _context.UpdateProductAsync(updates)) {
                var rs = await _context.GetProductByIdAsync(updates.Id);
                return _productDtoSrv.GetDto(rs.Result);
            }
            else {
                return NotFound();
            }
        }

        [HttpDelete("/Products/{key}/delete/{id}")]
        public async Task<ActionResult> DeleteProduct(string key, int id) {
            if(!_authSrv.HasSuperUserView(key)) {
                return StatusCode(403);
            }
            if(await _context.DeleteProductAsync(id)) {
                return NoContent();
            }
            return NotFound();
        }

        [HttpGet("/Products/{key}/id/{id}", Name = "GetProductId")]
        public async Task<ActionResult<ProductDto>> GetProductById(string key, int id) {
            if(!_authSrv.HasLimitedView(key)) {
                return StatusCode(403);
            }
            var product = await _context.GetProductByIdAsync(id);
            if(product.Response == GetObjectResponse.NOT_FOUND) {
                return NotFound();
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
                return NotFound();
            }
            var products = await _context.GetServerProductsAsync(serverId);
            var rt = products.Select(p => _productDtoSrv.GetDto(p)).ToList<ProductDto>();
            return rt;
        }
        #endregion
    }   

}