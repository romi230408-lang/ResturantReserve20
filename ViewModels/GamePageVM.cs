using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using ResturantReserve.Models;
using ResturantReserve.ModelsLogic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;

namespace ResturantReserve.ViewModels
{
    public partial class GamePageVM : ObservableObject
    {
        private double timeLeft;
        private readonly Game game;
        public string MyName => game.MyName;
        public string OpponentName => game.OpponentName;
        public ICommand ResetGameCommand { get; }
        public ICommand SelectCardCommand { get; }
        public ICommand TakeCardCommand { get; }
        public ICommand SkipCardCommand { get; }
        public int PickedCardsCount => game.PickedCardsCount;
        public ImageSource? OpenedCardImageSource => game.OpenedCardImageSource;
        public bool IsHostTurn => game.IsHostTurn;
        public int PackageCardCount => game.PackageCardCount;
        public bool IsMyTurn => game.IsMyTurn;
        public bool OpenedCardPending => game.OpenedCardPending;
        public ObservableCollection<Card> MyCards { get; } = new();
        public ICommand HatHatulCommand { get; }
        private bool timerStarted = false;
        public bool ShowCards { get; set; } = false;

        public GamePageVM(Game game)
        {
            this.game = game;;
            game.OnGameChanged += OnGameChanged;
            game.DisplayChanged += OnDisplayChanged;
            game.OnWin += OnWin;
            game.OnRevealCards += RevealCards;
            SyncMyCardsFromGame();
            
            if (!game.IsHostUser)
                game.UpdateGuestUser(OnComplete);
            SelectCardCommand = new Command<Card>(SelectCard);
            ResetGameCommand = new Command(ResetGame);
            TakeCardCommand = new Command(TakeCard);
            SkipCardCommand = new Command(SkipCard);
            HatHatulCommand = new Command(() => game.HatHatul());

            WeakReferenceMessenger.Default.Register<AppMessage<long>>(this, (r, m) =>
            {
                OnMessageReceived(m.Value);
            });

            WeakReferenceMessenger.Default.Register<AppMessage<TimerSetting>>(this, (r, m) =>
            {
                StartTimer(m.Value);
            });
        }
        private void OnDisplayChanged(object? sender, DisplayMoveArgs e)
        {
            OnPropertyChanged(nameof(IsHostTurn));
            OnPropertyChanged(nameof(PickedCardsCount)); 
            OnPropertyChanged(nameof(OpenedCardImageSource));
            OnPropertyChanged(nameof(IsMyTurn));
        }
        private void OnMessageReceived(long timeLeft)
        {
            TimeLeft = timeLeft / 1000f;
        }
        private async void StartTimer(TimerSetting setting)
        {
            long remaining = setting.MillisInFuture;

            while (remaining > 0)
            {
                WeakReferenceMessenger.Default.Send(new AppMessage<long>(remaining));

                await Task.Delay((int)setting.CountDownInterval);
                remaining -= setting.CountDownInterval;
            }

            // שולח 0 בסיום
            WeakReferenceMessenger.Default.Send(new AppMessage<long>(0));
            // --- כאן מתחילים את המשחק:
            if (game.IsHostUser && game.IsFull)
            {
                game.DealOpeningHands(4);
            }
        }

        public double TimeLeft
        {
            get => Math.Round(timeLeft, 2, MidpointRounding.ToNegativeInfinity);
            set
            {
                if (timeLeft != value)
                {
                    timeLeft = value;
                    OnPropertyChanged();
                }
            }
        }
        private void OnGameChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(OpponentName));
            OnPropertyChanged(nameof(PickedCardsCount)); 
            OnPropertyChanged(nameof(IsHostTurn)); 
            OnPropertyChanged(nameof(OpenedCardImageSource));
            OnPropertyChanged(nameof(PackageCardCount));
            OnPropertyChanged(nameof(IsMyTurn));
            OnPropertyChanged(nameof(OpenedCardPending));
            SyncMyCardsFromGame(); // במקום OnPropertyChanged(nameof(MyCards))
            if (game.IsFull && !timerStarted)
            {
                timerStarted = true;

                TimerSetting timerSetting = new TimerSetting(10000, 1000);
                WeakReferenceMessenger.Default.Send(new AppMessage<TimerSetting>(timerSetting));
            }
        }

        private void OnComplete(Task task)
        {
            if (!task.IsCompletedSuccessfully)
                Toast.Make(Strings.JoinGameErr, CommunityToolkit.Maui.Core.ToastDuration.Long, 14);

        }

        public void AddSnapshotListener()
        {
            game.AddSnapshotListener();
        }

        public void RemoveSnapshotListener()
        {
            game.RemoveSnapshotListener();
        }

        private void TakeCard(object? _)
        {
            TakePackageCard();
        }

        private void SelectCard(Card selectedCard)
        {
            if (selectedCard == null || !IsMyTurn)
                return;

            if (game.OpenedCardPending)
            {
                game.SelectCard(selectedCard); // החלפה או דחייה
            }

            selectedCard.IsRevealed = true;

            OnPropertyChanged(nameof(OpenedCardImageSource));
            OnPropertyChanged(nameof(IsMyTurn));
            OnPropertyChanged(nameof(PickedCardsCount));
        }

        private void StartNewGame(bool restart)
        {
            if (restart)
                game.Restart();

            for (int i = 0; i < 4; i++)
                TakePackageCard();

            OnPropertyChanged(nameof(OpenedCardImageSource));
        }

        private async void OnWin(object? sender, EventArgs e)
        {
            bool iWon = game.WinnerName?.Trim() == game.MyName?.Trim();

            string title = iWon ? "😢 הפסדת" : "🎉 ניצחת!"; 
            string message = iWon ? "ליריב יש סכום נמוך יותר" : "יש לך את סכום הקלפים הנמוך ביותר!";

            bool newGame = await Application.Current!.MainPage!.DisplayAlert(
                title,
                message,
                "משחק חדש",
                "יציאה");

            if (newGame)
                ResetGame();
            else
                Application.Current?.Quit();
        }

        private void TakePackageCard()
        {
            Card? card = game.TakePackageCard();
            if (card != null)
            {
                OnPropertyChanged(nameof(OpenedCardImageSource));
                OnPropertyChanged(nameof(PickedCardsCount));
            }
            else
            {
                Toast.Make("No more cards", ToastDuration.Long, 20).Show();
            }
        }

        private void ResetGame()
        {
            StartNewGame(true);
            OnPropertyChanged(nameof(OpenedCardImageSource));
            OnPropertyChanged(nameof(PickedCardsCount));
        }


        // פעולה שמופעלת בלחיצה על כפתור Skip
        public void SkipCard()
        {
            if (!game.IsMyTurn || game.OpenedCardPending == false)
                return;

            // קוראים לפונקציה שכבר מעדכנת את התור ומסנכרנת ל-Firestore
            game.SkipReplace();

            // מעדכנים את ה-UI
            OnPropertyChanged(nameof(OpenedCardImageSource));
            OnPropertyChanged(nameof(PickedCardsCount));
            OnPropertyChanged(nameof(IsMyTurn));
        }

        private void SyncMyCardsFromGame()
        {
            MyCards.Clear();
            foreach (var card in game.MyCards) // כאן game.MyCards יכול להישאר List<Card>
                MyCards.Add(card);
        }

        private void RevealCards(object? sender, EventArgs e)
        {
            foreach (var card in MyCards)
                card.IsRevealed = true;
        }
    }
}
