namespace BusinessLogic.Models.Interfaces
{
    public interface IScheduler
    {
        int AvailableCarts { get; set; }
        int MaxCartCapacity { get; set; }
        int MaxFloors { get; set; }

        Task<ElevatorRequestResult> RequestCart(int pickupFloor, int dropOffFloor, int passengers);
    }
}