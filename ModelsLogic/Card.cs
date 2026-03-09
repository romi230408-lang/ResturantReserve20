using ResturantReserve.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ResturantReserve.ModelsLogic
{
    public class Card : CardModel, INotifyPropertyChanged
    {
        private const int OFFSET = 50;
        public event EventHandler? OnClick;
        public new event PropertyChangedEventHandler? PropertyChanged;

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

        private bool isRevealed;
        public new bool IsRevealed
        {
            get => isRevealed;
            set
            {
                if (isRevealed != value)
                {
                    isRevealed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplaySource));
                }
            }
        }

        public new ImageSource DisplaySource => IsRevealed
            ? Source
            : ImageSource.FromFile("startingcard.png");

        protected new void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

