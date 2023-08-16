using BusinessLogic.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Models
{
    public class Scheduler : IScheduler
    {
        public int MaxFloors { get; set; }
        public int MaxCartCapacity { get; set; } = 7;
        public int AvailableCarts { get; set; } = 10;

        private ICartSchedule _schedule { get; set; }
        private Serilog.ILogger _logger = Serilog.Log.ForContext<Scheduler>();
        public Scheduler(ICartSchedule schedule)
        {
            _schedule = schedule;
        }

        public async Task Run(IEnumerable<Cart> carts)
        {
            foreach (var cart in carts)
            {
                _schedule.AddCart(cart);

            }
            await _schedule.ResetCarts();
        }

        public ICartSchedule GetSchedule()
        {
            return _schedule;
        }
        public async Task<ElevatorRequestResult> RequestCart(int pickupFloor, int dropOffFloor, int passengers)
        {
            _logger.Information("Looking for Elevator");
            await Task.Delay(2000);
            Cart candidateCart = null;
            var tripDirection = pickupFloor < dropOffFloor ? CartDirection.up : CartDirection.down;

            if (passengers > MaxCartCapacity)
            {
                _logger.Error("Unable to Service Request to many Passengers,floor {floor},passengers {passengers},MaxCapacity {cap} ", pickupFloor, passengers, MaxCartCapacity);
                return new ElevatorRequestResult(ElevatorRequestOutcome.Rejected, $"Exceeds passenger limit of {MaxCartCapacity}");

            }

            //Get Closest Elevator based on multiple "Algorithms"


            if (GetActiveCartsOnFloor(pickupFloor).Count() > 0)
            {
                candidateCart = GetActiveCartsOnFloor(pickupFloor).FirstOrDefault();
                _schedule.AddInstruction(new CartFloorInstructions()
                {
                    CartId = candidateCart.CartID,
                    LoadingFloorNum = pickupFloor,
                    DestinationFloorNum = dropOffFloor,
                    Passengers = passengers
                });

                _logger.Information("PickUp : {pickup} placed on Cart : {cartId}, method : {placementMethod}", pickupFloor, candidateCart.CartID, nameof(GetActiveCartsOnFloor));
                return new ElevatorRequestResult(ElevatorRequestOutcome.Placed, $"Placed on Cart {candidateCart.CartID}");
            }

            if (PassingByFloor(pickupFloor).Count() > 0)
            {

                candidateCart = PassingByFloor(pickupFloor).FirstOrDefault();
                _schedule.AddInstruction(new CartFloorInstructions()
                {
                    CartId = candidateCart.CartID,
                    LoadingFloorNum = pickupFloor,
                    DestinationFloorNum = dropOffFloor,
                    Passengers = passengers
                });

                _logger.Information("PickUp : {pickup} placed on Cart : {cartId}, method : {placementMethod}", pickupFloor, candidateCart.CartID, nameof(PassingByFloor));
                return new ElevatorRequestResult(ElevatorRequestOutcome.Placed, $"Placed on Cart {candidateCart.CartID}");
            }
            if (GetClosestEmpty(pickupFloor, passengers).Count() > 0)
            {


                candidateCart = GetClosestEmpty(pickupFloor, passengers).FirstOrDefault();
                _schedule.AddInstruction(new CartFloorInstructions()
                {
                    CartId = candidateCart.CartID,
                    LoadingFloorNum = pickupFloor,
                    DestinationFloorNum = dropOffFloor,
                    Passengers = passengers
                });

                _logger.Information("PickUp : {pickup} placed on Cart : {cartId}, method : {placementMethod}", pickupFloor, candidateCart.CartID, nameof(GetClosestEmpty));
                return new ElevatorRequestResult(ElevatorRequestOutcome.Placed, $"Placed on Cart {candidateCart.CartID}");

            }


            if (GetClosest(pickupFloor, passengers).Count() > 0)
            {


                candidateCart = GetClosest(pickupFloor, passengers).FirstOrDefault();
                _schedule.AddInstruction(new CartFloorInstructions()
                {
                    CartId = candidateCart.CartID,
                    LoadingFloorNum = pickupFloor,
                    DestinationFloorNum = dropOffFloor,
                    Passengers = passengers
                });

                _logger.Information("PickUp : {pickup} placed on Cart : {cartId}, method : {placementMethod}", pickupFloor, candidateCart.CartID, nameof(GetClosest));
                return new ElevatorRequestResult(ElevatorRequestOutcome.Placed, $"Placed on Cart {candidateCart.CartID}");

            }



            if (_schedule.Carts.Count() < AvailableCarts && passengers == MaxCartCapacity)
            {
                candidateCart = _schedule.AddCart(new Cart(_schedule.Carts.Max(c => c.CartID) + 1, 0, _schedule));
                _schedule.ResetCart(candidateCart.CartID);

                _schedule.AddInstruction(new CartFloorInstructions()
                {
                    CartId = candidateCart.CartID,
                    LoadingFloorNum = pickupFloor,
                    DestinationFloorNum = dropOffFloor,
                    Passengers = passengers
                });

                _logger.Information("PickUp : {pickup} placed on Cart : {cartId}, method : {placementMethod}", pickupFloor, candidateCart.CartID, "Provision New Cart");
                return new ElevatorRequestResult(ElevatorRequestOutcome.Placed, $"Placed on Cart {candidateCart.CartID}");



            }

            return new ElevatorRequestResult(ElevatorRequestOutcome.Rejected, "No Carts Available");


        }


        public IEnumerable<Cart> GetClosestEmpty(int floor, int passengers)
        {
            return _schedule.Carts.Where(c => c.CurrentState == CartState.AwaitingDispatch &&
                                            c.CurrentCapacity + passengers < MaxCartCapacity).
                                            OrderBy(c => Math.Abs(c.CurrentFloor - floor));
        }



        public IEnumerable<Cart> GetActiveCartsOnFloor(int floor)
        {
            //TODO: Check capacity
            return _schedule.Carts.Where(c => c.CurrentFloor == floor && c.CurrentState == CartState.AwaitingDispatch);
        }

        public IEnumerable<Cart> PassingByFloor(int floor)
        {
            //TODO: Check capacity
            return _schedule.Carts.Where(c => _schedule.GetDropOffFloorsById(c.CartID).Contains(floor) || _schedule.GetPickUpFloorsById(c.CartID).Contains(floor));
        }

        public IEnumerable<Cart> GetClosest(int floor, int passengers)
        {
            //TODO: Check capacity
            return _schedule.Carts.Where(c => c.CurrentState != CartState.Stopped &&
                                            c.CurrentCapacity + passengers < MaxCartCapacity).
                                            OrderBy(c => Math.Abs(c.CurrentFloor - floor));
        }



    }
    public record ElevatorRequestResult(ElevatorRequestOutcome RequestOutcome, string message);


    public enum ElevatorRequestOutcome
    {
        Placed,
        Rejected
    }
}
