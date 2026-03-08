using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using ResturantReserve.ModelsLogic;
using Microsoft.Maui.Controls; 

namespace ResturantReserve.Models
{
    public abstract class GameModel
    {
        protected FbData fbd = new();
        protected IListenerRegistration? ilr;

        [Ignored]
        public EventHandler? OnGameChanged;
        [Ignored]
        public EventHandler? OnGameDeleted;

        [Ignored]
        
        public string Id { get; set; } = string.Empty;

        public string HostName { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public bool IsFull { get; set; }

        [Ignored]
        public abstract string OpponentName { get; }

        [Ignored]
        public string MyName { get; set; } = new User().Name;

        [Ignored]
        public bool IsHostUser { get; set; }

        protected TimerSetting timerSetting = new(10000, 1000);

        public abstract void SetDocument(Action<System.Threading.Tasks.Task> OnComplete);
        public abstract void RemoveSnapshotListener();
        public abstract void AddSnapshotListener();
        public abstract void DeleteDocument(Action<System.Threading.Tasks.Task> OnComplete);


        protected int pickedCardsCount;
        protected Card? openedCard;
        protected CardsSet? package;
        public abstract Card? TakePackageCard();

        protected GameModel()
        {
            Created = DateTime.UtcNow;
        }

        [Ignored]
        public int PickedCardsCount
        {
            get { return pickedCardsCount; }
        }

        [Ignored]
        public ImageSource? OpenedCardImageSource
        {
            get { return openedCard?.Source; }
        }

        public bool IsHostTurn { get; set; } = false;
        public abstract void Play(bool MyMove);
        protected GameStatus _status = new();
        protected abstract void UpdateStatus();
        protected abstract void UpdateFbMove();
        public EventHandler<DisplayMoveArgs>? DisplayChanged;
        public List<int> Move { get; set; } = [Keys.NoMove, Keys.NoMove];
        public int PackageCardCount { get; set; } = 52;
        public CardData? OpenedCardData { get; set; }
        public int PackageIndex { get; set; } = 0;
        public List<int> DeckOrder { get; set; } = new();
        public List<CardData> PackageCards { get; set; } = new();  
        public List<CardData> HostHand { get; set; } = new();      
        public List<CardData> GuestHand { get; set; } = new();
        [Ignored]
        public bool IsMyTurn =>
            (IsHostUser && IsHostTurn) ||
            (!IsHostUser && !IsHostTurn);


        public void InitializeNewGame()
        {
            // שומר את מצב החבילה והיד ל-Firestore
            UpdateFbMove();
        }

        public bool OpenedCardPending { get; set; } = false;
        public bool HandsDealt { get; set; }
        public string WinnerName { get; set; } = "";
        public string HostId { get; set; }
        public string GuestId { get; set; }

    }
}
