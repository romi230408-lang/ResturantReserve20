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


        public GamePageVM(Game game)
        {
            Console.WriteLine("VM ctor - start");
            this.game = game;
            Console.WriteLine("VM ctor - game assigned");
            game.OnGameChanged += OnGameChanged;
            game.DisplayChanged += OnDisplayChanged;
            game.OnWin += OnWin;
            Console.WriteLine("VM ctor - events registered");
            SyncMyCardsFromGame();
            Console.WriteLine("VM ctor - after SyncMyCardsFromGame");
            
            if (!game.IsHostUser)
                game.UpdateGuestUser(OnComplete);
            SelectCardCommand = new Command<Card>(SelectCard);
            ResetGameCommand = new Command(ResetGame);
            TakeCardCommand = new Command(TakeCard);
            SkipCardCommand = new Command(SkipCard);
            HatHatulCommand = new Command(() => game.HatHatul());
            if (game.IsHostUser)
            {
                StartNewGame(false);
            }

            WeakReferenceMessenger.Default.Register<AppMessage<long>>(this, (r, m) =>
            {
                OnMessageReceived(m.Value);
            });

            WeakReferenceMessenger.Default.Send(new AppMessage<long>(10000));
            Console.WriteLine("VM ctor - end");

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
            bool iWon = game.WinnerName == game.MyName;

            string title = iWon ? "🎉 ניצחת!" : "😢 הפסדת";
            string message = iWon ? "יש לך את סכום הקלפים הנמוך ביותר!" : "ליריב יש סכום נמוך יותר";

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
        

    }
}
