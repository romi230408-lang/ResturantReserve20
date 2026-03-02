using ResturantReserve.Models;

namespace ResturantReserve.ModelsLogic
{
    public class Card : CardModel
    {
        private const int OFFSET = 50;
        public event EventHandler? OnClick;

        public Card() : base(CardModel.CardType.Number, 0)  
        {
        }

        public Card(CardType type, int value) : base(type, value)
        {
            Clicked += OnCardClick;
        }

        private void OnCardClick(object? sender, EventArgs e)
        {
            OnClick?.Invoke(this, EventArgs.Empty);
        }

        public void ToggleSelected()
        {
            IsSelected = !IsSelected;
            Thickness t = Margin;
            t.Bottom = IsSelected ? OFFSET : 0;
            Margin = t;
        }

        public static Card Copy(Card card)
        {
            Card newCard = new(CardType.Number, 0);

            newCard = new Card(card.Type, card.Value)
            {
                Index = card.Index
            };

            return newCard;
        }
    }
}

