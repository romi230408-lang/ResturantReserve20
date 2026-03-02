using ResturantReserve.ModelsLogic;

namespace ResturantReserve.Models
{
    public class CardsSetModel
    {
        public List<Card> cards;

        public CardsSetModel()
        {
            cards = new List<Card>();
        }

        public bool SingleSelect { protected get; set; }

        public int Count
        {
            get { return cards.Count; }
        }
    }
}
