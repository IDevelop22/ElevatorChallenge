using BusinessLogic.Models;
using BusinessLogic.Models.Interfaces;
using Moq;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BusinessLogic.Tests
{
    public class CartScheduleTests
    {
        [Fact]
        public void AddCart_AddsCartToCartsList()
        {
            
            var mockCart = new Mock<Cart>();
            var cartSchedule = new CartSchedule();

          
            var addedCart = cartSchedule.AddCart(mockCart.Object);

            
            Assert.Contains(addedCart, cartSchedule.Carts);
        }



        

        [Fact]
        public void GetPickUpFloorsById_ReturnsPickUpFloors()
        {
            
            var cartId = 1;
            var cartSchedule = new CartSchedule();
            cartSchedule.Schedule.Add(new CartFloorInstructions { CartId = cartId, FloorInstructionStatus = FloorInstructionStatus.Requested, LoadingFloorNum = 2 });
            cartSchedule.Schedule.Add(new CartFloorInstructions { CartId = cartId, FloorInstructionStatus = FloorInstructionStatus.Requested, LoadingFloorNum = 5 });

           
            var pickUpFloors = cartSchedule.GetPickUpFloorsById(cartId);

            
            Assert.Equal(new List<int> { 2, 5 }, pickUpFloors);
        }


   


    }
}
