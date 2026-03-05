using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Attributes;
using ResturantReserve.Models;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

namespace ResturantReserve.ModelsLogic
{
    public class Game : GameModel
    {
        private CardsSet myCards = new CardsSet(false);
        private bool isTakingFromPackage = false;
        [Ignored]
        public List<Card> MyCards => myCards.GetAllCards();

        public Game() : base()
        {
            // Firestore only
            package = new CardsSet(full: false);

            myCards = new CardsSet(full: false)
            {
                SingleSelect = true
            };
        }

        public Game(bool isHostUser) : base()
        {
            IsHostUser = isHostUser;
            PackageCardCount = 52;
            package = IsHostUser
        ? new CardsSet(full: true)
        : new CardsSet(full: false);
            myCards = new CardsSet(full: false)
            {
                SingleSelect = true
            };

            HostName = new User().Name;
            Created = DateTime.Now;
        }
        


        public void Restart()
        {
            pickedCardsCount = 0;
            package.Reset(true);
            openedCard = package.TakeCard();
            PackageCardCount = package.Count; 
            myCards.Reset(false);  

            Move = [Keys.NoMove, Keys.NoMove];
            UpdateFbMove();
        }

        public override Card? TakePackageCard()
        {
            if (!IsMyTurn || package == null || isTakingFromPackage)
                return null;

            // אם יש כבר קלף פתוח שלא טופל — לא שולפים חדש
            if (OpenedCardPending)
                return null;

            isTakingFromPackage = true;

            var newCard = package.TakeCard();
            if (newCard != null)
            {
                openedCard = newCard;
                OpenedCardPending = true;
                pickedCardsCount++;
                PackageCardCount = package.Count;

                UpdateFbMove();
                OnGameChanged?.Invoke(this, EventArgs.Empty);
            }

            isTakingFromPackage = false;

            return newCard;
        }


        public Card? TakeCard()
        {
            if (!IsMyTurn || package == null)
                return null;

            // 1) מסירים קלף אקראי מהיד
            myCards.TakeCard();

            // 2) מוסיפים את הקלף הפתוח הנוכחי ליד (אם קיים)
            if (openedCard != null)
                myCards.Add(openedCard);

            // 3) שולפים קלף חדש מהחבילה
            var newCard = package.TakeCard();
            if (newCard != null)
            {
                openedCard = newCard; // עדכון הקלף הפתוח
                pickedCardsCount++;
                PackageCardCount = package.Count;
                IsHostTurn = !IsHostTurn;
                Move = new List<int> { Keys.TakeFromPackage, 0 };
                UpdateFbMove(); // מסנכרן מיידי
            }
            else
            {
                openedCard = null; // אם אין קלפים בחבילה, אין קלף פתוח
            }

            return newCard; // מחזיר את הקלף החדש או null
        }



        internal void SelectCard(Card card)
        {
            if (!IsMyTurn || !OpenedCardPending)
                return;

            if (myCards.GetAllCards().Contains(card)) // החלפה עם קלף מהיד
            {
                ReplaceCard(card.Index);
            }
            else if (card == openedCard) // השארת הקלף מהחבילה
            {
                SkipReplace();
            }

            OpenedCardPending = false;
            OnGameChanged?.Invoke(this, EventArgs.Empty);
        }


        public override string OpponentName => IsHostUser ? GuestName : HostName;

        public override void SetDocument(Action<System.Threading.Tasks.Task> OnComplete)
        {
            Id = fbd.SetDocument(this, Keys.GamesCollection, Id, OnComplete);
        }

        public void UpdateGuestUser(Action<Task> OnComplete)
        {
            IsFull = true;
            GuestName = MyName;
            UpdateFbJoinGame(OnComplete);
        }

        private void UpdateFbJoinGame(Action<Task> OnComplete)
        {
            Dictionary<string, object> dict = new()
            {
                { nameof(IsFull), IsFull },
                { nameof(GuestName), GuestName }
            };
            fbd.UpdateFields(Keys.GamesCollection, Id, dict, OnComplete);
        }

        public override void AddSnapshotListener()
        {
            System.Diagnostics.Debug.WriteLine($"GAME ID BEFORE LISTENER: {Id}");
            if (string.IsNullOrEmpty(Id))
                return;
            ilr = fbd.AddSnapshotListener(Keys.GamesCollection, Id, OnChange);
        }

        public override void RemoveSnapshotListener()
        {
            ilr?.Remove();
            DeleteDocument(OnComplete);
        }

        private void OnComplete(Task task)
        {
            if (task.IsCompletedSuccessfully)
                OnGameDeleted?.Invoke(this, EventArgs.Empty);
        }
        protected override void UpdateStatus()
        {
            _status.CurrentStatus = IsHostUser && IsHostTurn || !IsHostUser && !IsHostTurn ?
                GameStatus.Statuses.Play : GameStatus.Statuses.Wait;
        }
        protected override void UpdateFbMove()
        {
            PackageCards = package.GetAllCards().Select(card => new CardData
            {
                Type = card.Type.ToString(),
                Value = card.Value,
                Index = card.Index
            }).ToList();

            Dictionary<string, object> dict = new()
            {
                { nameof(Move), Move },
                { nameof(IsHostTurn), IsHostTurn },
                { nameof(PackageCardCount), package.Count },
                { nameof(PickedCardsCount), pickedCardsCount },
                { nameof(PackageIndex), PackageIndex },
                { nameof(PackageCards), PackageCards },     

            };

            var myHand = myCards.GetAllCards().Select(card => new CardData
            {
                Type = card.Type.ToString(),
                Value = card.Value,
                Index = card.Index
            }).ToList();

            if (IsHostUser)
            {
                dict.Add(nameof(HostHand), myHand);
            }
            else
            {
                dict.Add(nameof(GuestHand), myHand);
            }

            if (openedCard != null)
            {
                dict.Add(nameof(OpenedCardData), new CardData
                {
                    Type = openedCard.Type.ToString(),
                    Value = openedCard.Value,
                    Index = openedCard.Index
                });
            }

            fbd.UpdateFields(Keys.GamesCollection, Id, dict, OnComplete);
        }

        public override void Play(bool MyMove)
        {
            if (_status.CurrentStatus == GameStatus.Statuses.Play)
            {
                DisplayMoveArgs args = new(MyMove);
                DisplayChanged?.Invoke(this, args);

                if (MyMove)
                {
                    _status.ChangeStatus();
                    IsHostTurn = !IsHostTurn;
                    UpdateFbMove();       
                }
                else
                {
                    OnGameChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        private void OnChange(IDocumentSnapshot? snapshot, Exception? error)
        {
            Game? updatedGame = snapshot?.ToObject<Game>();
            if (updatedGame != null)
            {
                updatedGame.Id = snapshot!.Id;
                Id =updatedGame.Id;
                IsFull = updatedGame.IsFull;
                if (IsHostUser && updatedGame.IsFull && !updatedGame.HandsDealt)
                {
                    DealOpeningHands(4);
                    return;
                }
                GuestName = updatedGame.GuestName;
                Move = updatedGame.Move;
                IsHostTurn = updatedGame.IsHostTurn;
                PackageCardCount = updatedGame.PackageCardCount;
                pickedCardsCount = updatedGame.pickedCardsCount;
                PackageIndex = updatedGame.PackageIndex;
                if (updatedGame.OpenedCardData != null)
                {
                    var cd = updatedGame.OpenedCardData;
                    var type = (CardModel.CardType)Enum.Parse(
                        typeof(CardModel.CardType),
                        cd.Type
                    );

                    openedCard = new Card(type, cd.Value)
                    {
                        Index = cd.Index
                    };
                }
                else
                {
                    openedCard = null;
                }


                // ← שינוי 1: CardModel במקום Card + בניית Card חדש
                package.Reset(false);
                if (updatedGame.PackageCards != null)
                {
                    foreach (CardData cd in updatedGame.PackageCards)
                    {
                        var type = (CardModel.CardType)Enum.Parse(typeof(CardModel.CardType), cd.Type);
                        package.Add(new Card(type, cd.Value)
                        {
                            Index = cd.Index
                        });
                    }
                }
                 
                System.Diagnostics.Debug.WriteLine($"PACKAGE COUNT AFTER REBUILD: {package.Count}");

                // ← שינוי 2: CardModel במקום Card + בניית Card חדש
                myCards.Reset(false);
                var myHand = IsHostUser ? updatedGame.HostHand : updatedGame.GuestHand;
                foreach (CardData cd in myHand)
                {
                    var type = (CardModel.CardType)Enum.Parse(typeof(CardModel.CardType), cd.Type);
                    myCards.Add(new Card(type, cd.Value)
                    {
                        Index = cd.Index
                    });
                }

                UpdateStatus();

                if (_status.CurrentStatus == GameStatus.Statuses.Play)
                {
                    Play(false);
                }
                OpenedCardPending = IsMyTurn && openedCard != null;
                OnGameChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    OnGameDeleted?.Invoke(this, EventArgs.Empty);
                    Shell.Current.Navigation.PopAsync();
                });
            }
        }


        public override void DeleteDocument(Action<Task> OnComplete)
        {
            fbd.DeleteDocument(Keys.GamesCollection, Id, OnComplete);
        }
        public void ReplaceCard(int handIndex)
        {
            if (!IsMyTurn || !OpenedCardPending || package == null)
                return;

            // מחליפים עם הקלף הפתוח
            myCards.Replace(handIndex, openedCard);

            // שולפים קלף חדש בשביל התור הבא
            var newCard = package.TakeCard();
            if (newCard != null)
            {
                openedCard = newCard;
                PackageCardCount = package.Count;
            }

            OpenedCardPending = false;

            // מעבירים תור
            IsHostTurn = !IsHostTurn;

            Move = new List<int> { Keys.ReplaceCard, handIndex };

            UpdateFbMove();
        }

        public void SkipReplace()
        {
            if (!IsMyTurn || !OpenedCardPending || package == null)
                return;

            // שולפים קלף חדש במקום הישן
            var newCard = package.TakeCard();

            if (newCard != null)
            {
                openedCard = newCard;
                PackageCardCount = package.Count;
            }

            OpenedCardPending = false;

            // מעבירים תור
            IsHostTurn = !IsHostTurn;

            Move = new List<int> { Keys.SkipReplace, 0 };

            UpdateFbMove();
        }

        public void DealOpeningHands(int count)
        {
            if (!IsHostUser || package == null)
                return;

            List<CardData> hostCards = new();
            List<CardData> guestCards = new();

            for (int i = 0; i < count; i++)
            {
                var hostCard = package.TakeCard();
                if (hostCard != null)
                {
                    hostCards.Add(new CardData
                    {
                        Type = hostCard.Type.ToString(),
                        Value = hostCard.Value,
                        Index = hostCard.Index
                    });
                }

                var guestCard = package.TakeCard();
                if (guestCard != null)
                {
                    guestCards.Add(new CardData
                    {
                        Type = guestCard.Type.ToString(),
                        Value = guestCard.Value,
                        Index = guestCard.Index
                    });
                }
            }

            // פתיחת קלף אחרי חלוקת הידיים
            openedCard = package.TakeCard();
            PackageCardCount = package.Count;
            
            var dict = new Dictionary<string, object>
            {
                { nameof(HostHand), hostCards },
                { nameof(GuestHand), guestCards },
                { nameof(PackageCards), package.GetAllCards().Select(card => new CardData
                    {
                        Type = card.Type.ToString(),
                        Value = card.Value,
                        Index = card.Index
                    }).ToList()
                },
                { nameof(PackageCardCount), PackageCardCount }
            };

            if (openedCard != null)
            {
                dict.Add(nameof(OpenedCardData), new CardData
                {
                    Type = openedCard.Type.ToString(),
                    Value = openedCard.Value,
                    Index = openedCard.Index
                });
            }
            HandsDealt = true;
            dict.Add(nameof(HandsDealt), true);
            fbd.UpdateFields(Keys.GamesCollection, Id, dict, OnComplete);
        }


    }
}
