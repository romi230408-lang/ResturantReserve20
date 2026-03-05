using ResturantReserve.Models;
using System;

namespace ResturantReserve.ModelsLogic
{
    public class CardsSet : CardsSetModel
    {
        private readonly Random rnd;
        private Card? selectedCard;

        public CardsSet(bool full) : base()
        {
            selectedCard = null;
            rnd = new Random();

            if (full)
                FillPackage();
        }

        private void FillPackage()
        {
            // Todo: remove this for of temp
            for (int temp = 0; temp < 20; temp++)
            {
                for (int value = 0; value <= 9; value++)
                {
                    cards.Add(new Card(CardModel.CardType.Number, value));
                }

                cards.Add(new Card(CardModel.CardType.Look, 0));
                cards.Add(new Card(CardModel.CardType.Swap, 0));
                cards.Add(new Card(CardModel.CardType.DrawTwo, 0));
            }
        }

        public void Reset(bool full)
        {
            cards.Clear();
            selectedCard = null;
            if (full)
                FillPackage();
        }

        public Card Add(Card card)
        {
            card.Index = cards.Count;
            card.Margin = new Thickness(50 + 30 * cards.Count, 0, 0, 0);
            cards.Add(card);
            return card;
        }

        public Card? TakeCard()
        {
            Card? card = null;

            if (cards.Count > 0)
            {
                int index = rnd.Next(0, cards.Count);
                card = cards[index];
                cards.RemoveAt(index);
            }

            return card;
        }
        public Card? TakeCard(int index)
        {
            if (index >= 0 && index < cards.Count)
            {
                Card card = cards[index];
                cards.RemoveAt(index);
                return card;
            }
            return null;
        }

        public void SelectCard(Card card)
        {
            if (SingleSelect)
            {
                if (card.IsSelected)
                {
                    card.ToggleSelected();
                    selectedCard = null;
                }
                else
                {
                    selectedCard?.ToggleSelected();
                    card.ToggleSelected();
                    selectedCard = card;
                }
            }
            else
            {
                card?.ToggleSelected();
            }
        }

        public List<Card> GetAllCards()
        {
            return cards.ToList();  // בהנחה שיש שדה cards פנימי List<Card>
        }
        public List<Card> Cards => cards;
        public void LoadFromList(List<Card> cardsList)
        {
            Reset(false);
            foreach (var card in cardsList)
                this.cards.Add(card);  // הוסף לרשימה הפנימית
        }
        public Card? Replace(int index, Card newCard)
        {
            if (index < 0 || index >= cards.Count)
                return null;

            Card oldCard = cards[index];

            newCard.Index = index;
            cards[index] = newCard;

            return oldCard;
        }

    }
}

