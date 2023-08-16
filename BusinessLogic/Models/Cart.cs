using BusinessLogic.Models.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public CartState CurrentState { get; set; }
        public int CurrentFloor { get;  set; }
        public int CurrentCapacity { get; private set; }
        public CartDirection CurrentDirection { get; private set; }
        private ICartSchedule _schedule;
        private Serilog.ILogger _logger = Serilog.Log.ForContext<Cart>();


        public Cart() { }
        public Cart(int Id, int floor, ICartSchedule schedule)
        {
            CartID = Id;
            CurrentFloor = floor;
            CurrentCapacity = 0;
            CurrentState = CartState.AwaitingDispatch;
            _schedule = schedule;



        }
        public async Task WaitForDispatch()
        {
            int count = 1;
            while (CurrentState == CartState.AwaitingDispatch)
            {
                await Task.Delay(1000 * count);
                //TODO: Add Backoff after counter reaches certain number
                count++;
                if (_schedule.GetScheduleByCartId(CartID).Count() > 0)
                {
                    CurrentState = CartState.Moving;
                    await ProcessSchedule();
                }
            }
        }

        private async Task ProcessSchedule()
        {
            var myDropOffSchedule = _schedule.GetDropOffSchedule(CartID);
            var myRequestSchedule = _schedule.GetPickUpSchedule(CartID);


            NextElevatorAction nextFloor = _schedule.GetClosestNextFloor(CartID, CurrentFloor);
            while (myDropOffSchedule.Count() > 0 || myRequestSchedule.Count() > 0)
            {

                _logger.Information("Elevator : {cartId} going to Floor {nextFloor} for {nextAction}", CartID, nextFloor.NextFloor, nextFloor.Action);
                await Move(nextFloor.NextFloor);
                await Task.Delay(10000);
                nextFloor = _schedule.GetClosestNextFloor(CartID, CurrentFloor);
                myDropOffSchedule = _schedule.GetDropOffSchedule(CartID);
                myRequestSchedule = _schedule.GetPickUpSchedule(CartID);
            }
            _logger.Information("Elevator : {cartId} finished and waiting on floor {floor}", CartID, CurrentFloor);
            SetState(CartState.AwaitingDispatch);
            await WaitForDispatch();

        }
        public async Task Move(int nextFloor)
        {

            if (nextFloor == CurrentFloor)
            {
                var offLoadInstructions = _schedule.GetScheduleByCartId(this.CartID).Where(s => s.DestinationFloorNum == CurrentFloor && s.FloorInstructionStatus == FloorInstructionStatus.Loaded);
                var onLoadInstructions = _schedule.GetScheduleByCartId(this.CartID).Where(s => s.LoadingFloorNum == CurrentFloor && s.FloorInstructionStatus == FloorInstructionStatus.Requested);

                if (offLoadInstructions != null)
                {

                    foreach (var offload in offLoadInstructions)
                    {
                        await OffLoadPassengers(offload.Passengers);
                        _schedule.Schedule.Remove(offload);
                    }
                }


                if (onLoadInstructions != null)
                {
                    foreach (var onload in onLoadInstructions)
                    {
                        await OnLoadPassengers(onload.Passengers);
                        onload.FloorInstructionStatus = FloorInstructionStatus.Loaded;
                    }
                }
            }
            if (nextFloor > CurrentFloor)
            {
                await MoveUp();
            }
            else if (nextFloor < CurrentFloor)
            {
                await MoveDown();
            }





        }


        public async Task OnLoadPassengers(int passengers)
        {
            Console.WriteLine($"Cart {CartID} Onloading {passengers} currently on floor : {CurrentFloor}");
            CurrentCapacity += passengers;
        }
        public async Task OffLoadPassengers(int passengers)
        {
            Console.WriteLine($"Cart {CartID} offloading {passengers} currently on floor : {CurrentFloor}");
            CurrentCapacity -= passengers;
        }

        public async Task MoveUp()
        {
            Console.WriteLine($"Cart {CartID} Moving {CartDirection.up} currently on floor : {CurrentFloor}");
            this.CurrentDirection = CartDirection.up;
            CurrentFloor++;
        }

        public async Task MoveDown()
        {
            Console.WriteLine($"Cart {CartID} Moving {CartDirection.down} currently on floor : {CurrentFloor}");
            this.CurrentDirection = CartDirection.down;
            CurrentFloor--;
        }

        public void SetState(CartState state)
        {
            CurrentState = state;
        }



    }

    public enum CartState
    {
        Idle,
        AwaitingDispatch,
        Moving,
        Stopped

    }

    public enum CartDirection
    {
        up,
        down,
        none
    }

    public enum ElevatorAction
    {
        DropOff,
        PickUp,
        PickUp_DropOff
    }
}
