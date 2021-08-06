using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using System.Diagnostics;
using System.Threading;

using Xunit;
using Moq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Microsoft.AspNetCore.Mvc;

using Fortexx.Models;
using Fortexx.Models.Api;
using Fortexx.Data;
using Fortexx.Controllers;
using Fortexx.Services;

namespace FortexxTests
{
    public class ControllerTest
    {


        private Mock<ILogger<PaymentController>> _loggerMock;
        private Mock<IAuthorizationService> _authMock;

        private IDtoService<Payment, PaymentDto> _paymentMock;
        private IDtoService<Product, ProductDto> _productMock;
        private IDtoService<GameServer, GameServerDto> _serverMock;

        public ControllerTest() {
            _loggerMock = new Mock<ILogger<PaymentController>>();
            _authMock = new Mock<IAuthorizationService>();

            _paymentMock = new PaymentDtoService(); // There should be mocks, but whatever

            _productMock = new ProductDtoService();

            _serverMock = new ServerDtoService();
        }

        [Fact]
        public async Task GetPaymentsAsyncReturnTest1()
        {
            _authMock.Setup(m => m.HasFullView(It.IsAny<string>()))
                    .Returns(true);

            using (var db = new PaymentContext(Utilities.TestDbContextOptions(), Utilities.BuildTestConfiguration())) {

                var payment1 = Utilities.GenerateRandomPayment(1);
                var payment2 = Utilities.GenerateRandomPayment(2);
                IEnumerable<PaymentDto> expectedResult = new List<PaymentDto> {_paymentMock.GetDto(payment1), _paymentMock.GetDto(payment2)}.OrderByDescending(p => p.Id);
                await db.AddPaymentAsync(payment1);
                await db.AddPaymentAsync(payment2);
                await db.SaveChangesAsync();

                var controller = new PaymentController(_loggerMock.Object, db, _authMock.Object, _paymentMock, _serverMock, _productMock);

                var result = await controller.GetPaymentsAsync("key");

                Assert.Equal(
                    expectedResult, result.Value
                );
            }
        }


        [Fact]
        public async Task GetPaymentsAsyncReturnTest2()
        {
            _authMock.Setup(m => m.HasFullView(It.IsAny<string>()))
                    .Returns(true);

            using (var db = new PaymentContext(Utilities.TestDbContextOptions(), Utilities.BuildTestConfiguration())) {
                var controller = new PaymentController(_loggerMock.Object, db, _authMock.Object, _paymentMock, _serverMock, _productMock);

                var result = await controller.GetPageOfNPayments("key", 2, 3);

                AssemblyLoadEventArgs.Equals(
                    0, result.Value.Count()
                );                
            }
        }

        [Fact]
        public async Task GetPageOfNPaymentsReturnTest() {

            _authMock.Setup(m => m.HasFullView(It.IsAny<string>()))
                    .Returns(true);

            using (var db = new PaymentContext(Utilities.TestDbContextOptions(), Utilities.BuildTestConfiguration())) {

                // Add 5 randomly generated payments
                var payment1 = Utilities.GenerateRandomPayment(1);
                var payment2 = Utilities.GenerateRandomPayment(2);
                var payment3 = Utilities.GenerateRandomPayment(3);
                var payment4 = Utilities.GenerateRandomPayment(4);
                IEnumerable<PaymentDto> expectedResult = new List<PaymentDto> {_paymentMock.GetDto(payment1), _paymentMock.GetDto(payment2)}.OrderByDescending(p => p.Id);
                await db.AddPaymentAsync(payment1);
                await db.AddPaymentAsync(payment2);
                await db.AddPaymentAsync(payment3);
                await db.AddPaymentAsync(payment4);
                await db.SaveChangesAsync();

                // Create controller
                var controller = new PaymentController(_loggerMock.Object, db, _authMock.Object, _paymentMock, _serverMock, _productMock);

                //Return page 2 with 2 records
                var result = await controller.GetPageOfNPayments("key", 2, 2);

                Assert.Equal(
                    expectedResult, result.Value
                );

                result = await controller.GetPageOfNPayments("key", 2, 3);

                AssemblyLoadEventArgs.Equals(
                    0, result.Value.Count()
                );
            }
        }

        [Fact]
        public async Task GetPaymentByIDReturnTest() {
            _authMock.Setup(m => m.HasFullView(It.IsAny<string>()))
                    .Returns(true);

            using (var db = new PaymentContext(Utilities.TestDbContextOptions(), Utilities.BuildTestConfiguration())) {

                var payment1 = Utilities.GenerateRandomPayment(51);
                await db.AddPaymentAsync(payment1);
                await db.SaveChangesAsync();

                var controller = new PaymentController(_loggerMock.Object, db, _authMock.Object, _paymentMock, _serverMock, _productMock);

                var result = await controller.GetPaymentByID("key", 51);

                Assert.Equal(_paymentMock.GetDto(payment1), result.Value);

                result = await controller.GetPaymentByID("key", 50);

                Assert.IsType<NotFoundResult>(result.Result);

            }
        }


        [Fact]
        public async Task GetPaymentByNameReturnTest() {
            _authMock.Setup(m => m.HasFullView(It.IsAny<string>()))
                    .Returns(true);

            using (var db = new PaymentContext(Utilities.TestDbContextOptions(), Utilities.BuildTestConfiguration())) {

                var payment1 = Utilities.GenerateRandomPayment(51, "test");
                var payment2 = Utilities.GenerateRandomPayment(52, "test");
                IEnumerable<PaymentDto> expectedResult = new List<PaymentDto> {_paymentMock.GetDto(payment1), _paymentMock.GetDto(payment2)}.OrderByDescending(p => p.Id);
                IEnumerable<PaymentDto> expectedResult2 = new List<PaymentDto>();

                await db.AddPaymentAsync(payment1);
                await db.AddPaymentAsync(payment2);
                await db.SaveChangesAsync();

                var controller = new PaymentController(_loggerMock.Object, db, _authMock.Object, _paymentMock, _serverMock, _productMock);

                var result = await controller.GetPaymentsByName("key", "test");

                Assert.Equal(expectedResult, result.Value);

                result = await controller.GetPaymentsByName("key", "test2");

                Assert.Equal(expectedResult2, result.Value);

            }
        }

        [Fact]
        public async Task SetActivatedTest() {
            _authMock.Setup(m => m.HasLimitedView(It.IsAny<string>()))
                    .Returns(true);

            using (var db = new PaymentContext(Utilities.TestDbContextOptions(), Utilities.BuildTestConfiguration())) {
                var payment1 = Utilities.GenerateRandomPayment(1);
                await db.AddPaymentAsync(payment1);
                await db.SaveChangesAsync();

                var controller = new PaymentController(_loggerMock.Object, db, _authMock.Object, _paymentMock, _serverMock, _productMock);

                var result = await controller.SetActivated("key", 1);
                Assert.True(result.Value.Activated);

                result = await controller.SetActivated("key", 1);
                var rs = (StatusCodeResult) result.Result;
                Assert.Equal(304, rs.StatusCode);

                result = await controller.SetActivated("key", 2);
                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

    }
}
