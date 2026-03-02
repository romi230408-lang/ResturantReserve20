
namespace ResturantReserve.Models
{
        public class DisplayMoveArgs(bool isHostMove) : EventArgs
        {
              public bool IsHostMove { get; set; } = isHostMove;
        }
}
