namespace BusinessLogic.Models.Interfaces
{
    public interface ICartSchedule
    {
        HashSet<CartFloorInstructions> Schedule { get; set; }
        HashSet<Cart> Carts { get; set; }
        
        int GetScheduleCapacityById(int cartId);
        public IEnumerable<CartFloorInstructions> GetDropOffSchedule(int cartId);
        public IEnumerable<CartFloorInstructions> GetPickUpSchedule(int cartId);
        IEnumerable<CartFloorInstructions> GetScheduleByCartId(int cartId);
        IEnumerable<int> GetDropOffFloorsById(int cartId);
        IEnumerable<int> GetPickUpFloorsById(int cartId);
        NextElevatorAction GetClosestNextFloor(int cartId, int currentFloor);

        void AddInstruction(CartFloorInstructions instruction);
        Cart AddCart(Cart cart);
        public Task ResetCart(int cartId);
        public Task ResetCarts(bool clearSchedule= false);
    }
}