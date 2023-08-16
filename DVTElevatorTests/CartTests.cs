using BusinessLogic.Models;
using BusinessLogic.Models.Interfaces;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BusinessLogic.Tests
{
    public class CartTests
    {
        [Fact]
        public async Task WaitForDispatch_ScheduleExists_ChangesStateAndProcessesSchedule()
        {
           
            var mockSchedule = new Mock<ICartSchedule>();
            var cartId = 1;
            var cartFloorInstructions = new CartFloorInstructions { CartId = cartId };
            mockSchedule.Setup(s => s.GetScheduleByCartId(cartId)).Returns(new List<CartFloorInstructions> { cartFloorInstructions });
            var cart = new Cart(cartId, 0, mockSchedule.Object);
            var logger = new Mock<ILogger>();
            Log.Logger = logger.Object;

           
            var dispatchTask = cart.WaitForDispatch();

           
            await Task.Delay(5000);
           // verify that cart checks for schedule updates is called once
            mockSchedule.Verify(s => s.GetScheduleByCartId(cartId), Times.AtLeastOnce);
        }

        [Fact]
        public async Task MoveUp_IncreasesCurrentFloorAndSetsDirection()
        {
            
            var mockSchedule = new Mock<ICartSchedule>();
            var cartId = 1;
            var cart = new Cart(cartId, 0, mockSchedule.Object);

            
            await cart.MoveUp();

            
            Assert.Equal(CartDirection.up, cart.CurrentDirection);
            Assert.Equal(1, cart.CurrentFloor);
        }

        


       
    }
}
