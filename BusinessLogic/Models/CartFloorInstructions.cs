using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Models
{
    public class CartFloorInstructions
    {
        public int CartId { get; set; }
        public int DestinationFloorNum { get; set; }
        public int LoadingFloorNum { get; set; }
        public int Passengers { get; set; }
        public FloorInstructionStatus FloorInstructionStatus { get; set; } = FloorInstructionStatus.Requested;
    }

    public enum FloorInstructionStatus { 
        Requested,
        Loaded
    }
}
