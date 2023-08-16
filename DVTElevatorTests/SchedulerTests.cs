using BusinessLogic.Models;
using BusinessLogic.Models.Interfaces;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BusinessLogic.Tests
{
    public class SchedulerTests
    {
        [Fact]
        public async Task Run_AddsCartsAndResetsCarts()
        {
           
            var mockCart1 = new Mock<Cart>();
            var mockCart2 = new Mock<Cart>();
            var mockCartSchedule = new Mock<ICartSchedule>();
            var scheduler = new Scheduler(mockCartSchedule.Object);

           
            await scheduler.Run(new List<Cart> { mockCart1.Object, mockCart2.Object });

           
            mockCartSchedule.Verify(s => s.AddCart(mockCart1.Object), Times.Once);
            mockCartSchedule.Verify(s => s.AddCart(mockCart2.Object), Times.Once);
            mockCartSchedule.Verify(s => s.ResetCarts(false), Times.Once);
        }

        [Fact]
        public async Task RequestCart_PassengersExceedCapacity_RejectsRequest()
        {
            
            var pickupFloor = 1;
            var dropOffFloor = 5;
            var passengers = 8;
            var mockCartSchedule = new Mock<ICartSchedule>();
            var scheduler = new Scheduler(mockCartSchedule.Object);

           
            var result = await scheduler.RequestCart(pickupFloor, dropOffFloor, passengers);

            
            Assert.Equal(ElevatorRequestOutcome.Rejected, result.RequestOutcome);
        }





    }
}
