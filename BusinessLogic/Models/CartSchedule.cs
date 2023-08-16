using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Models;
using BusinessLogic.Models.Interfaces;

namespace BusinessLogic.Models
{
    public class CartSchedule : ICartSchedule
    {
        private Serilog.ILogger _logger = Serilog.Log.ForContext<CartSchedule>();

        public CartSchedule()
        {
        }
        public HashSet<CartFloorInstructions> Schedule { get; set; } = new HashSet<CartFloorInstructions>();
        public HashSet<Cart> Carts { get; set; } = new HashSet<Cart>();

        public Cart AddCart(Cart cart)
        {
            Carts.Add(cart);
            return cart;
        }

        public async Task ResetCarts(bool clearSchedule = false)
        {
            foreach (var cart in Carts)
            {
                _logger.Information("Reseting Cart {cartId}", cart.CartID);
                if (clearSchedule)
                {
                    Schedule.Clear();
                }

                cart.WaitForDispatch();
            }
        }



        public async Task ResetCart(int cartId)
        {
            var cart = Carts.Where(c => c.CartID == cartId).FirstOrDefault();
            Schedule.RemoveWhere(c => c.CartId == cartId);
            cart.WaitForDispatch();
        }
        public void AddInstruction(CartFloorInstructions instruction)
        {
            this.Schedule.Add(instruction);
        }



        public IEnumerable<CartFloorInstructions> GetScheduleByCartId(int cartId)
        {
            return this.Schedule.Where(c => c.CartId == cartId).ToList();
        }



        public int GetScheduleCapacityById(int cartId)
        {
            return this.Schedule.Where(c => c.CartId == cartId).Sum(c => c.Passengers);
        }

        public IEnumerable<int> GetPickUpFloorsById(int cartId)
        {
            return GetScheduleByCartId(cartId).Select(c => c.LoadingFloorNum);
        }

        public IEnumerable<int> GetDropOffFloorsById(int cartId)
        {
            return GetScheduleByCartId(cartId).Select(c => c.DestinationFloorNum);
        }

        public IEnumerable<CartFloorInstructions> GetPickUpSchedule(int cartId)
        {
            return this.GetScheduleByCartId(cartId).Where(s => s.FloorInstructionStatus == FloorInstructionStatus.Requested).OrderBy(s => s.LoadingFloorNum);
        }


        public IEnumerable<CartFloorInstructions> GetDropOffSchedule(int cartId)
        {
            return this.GetScheduleByCartId(cartId).Where(s => s.FloorInstructionStatus == FloorInstructionStatus.Loaded).OrderBy(s => s.DestinationFloorNum);
        }


        public NextElevatorAction GetClosestNextFloor(int cartId, int currentFloor)
        {
            var requestSchedule = GetPickUpSchedule(cartId);
            var dropOffSchedule = GetDropOffSchedule(cartId);
            int maxPickup = int.MaxValue, minPickup = int.MaxValue, maxDropOff = int.MaxValue, minDropOff = int.MaxValue;

            if (requestSchedule.Count() > 0)
            {
                maxPickup = requestSchedule.Max(s => s.LoadingFloorNum);
                minPickup = requestSchedule.Min(s => s.LoadingFloorNum);
            }

            if (dropOffSchedule.Count() > 0)
            {
                maxDropOff = dropOffSchedule.Max(s => s.DestinationFloorNum);
                minDropOff = dropOffSchedule.Min(s => s.DestinationFloorNum);
            }



            var closestPickUp = 0; ;
            var closestDropOff = 0;
            if (Math.Abs(currentFloor - maxPickup) > Math.Abs(currentFloor - minPickup))
            {

                closestPickUp = minPickup;

            }
            else
            {
                closestPickUp = maxPickup;

            }

            if (Math.Abs(currentFloor - maxDropOff) > Math.Abs(currentFloor - minDropOff))
            {
                closestDropOff = minDropOff;

            }
            else
            {
                closestDropOff = maxDropOff;

            }

            if (Math.Abs(currentFloor - closestDropOff) < Math.Abs(currentFloor - closestPickUp))
            {
                return new NextElevatorAction(ElevatorAction.DropOff, closestDropOff);
            }
            else if (Math.Abs(currentFloor - closestDropOff) > Math.Abs(currentFloor - closestPickUp))
            {
                return new NextElevatorAction(ElevatorAction.PickUp, closestPickUp);
            }
            else
            {
                return new NextElevatorAction(ElevatorAction.PickUp_DropOff, closestPickUp);
            }
        }


    }
}


public record NextElevatorAction(ElevatorAction Action, int NextFloor);

