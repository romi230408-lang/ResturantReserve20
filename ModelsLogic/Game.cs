using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Plugin.CloudFirestore;
using ResturantReserve.Models;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

namespace ResturantReserve.ModelsLogic
{
    public class Game : GameModel
    {
        private CardsSet myCards = new CardsSet(false);
        public List<Card> MyCardsList => myCards.GetAllCards();

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

            if (IsHostUser)
            {
                openedCard = package.TakeCard();
                PackageCardCount = package.Count;
            }
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
            if (!IsMyTurn || package == null)
                return null;

            var card = package.TakeCard();
            if (card != null)
            {
                openedCard = card;
                OpenedCardPending = true;
                pickedCardsCount++;
                PackageCardCount = package.Count;
            }

            return openedCard;
        }


        public Card? TakeCard()
        {
            if (!IsMyTurn)
                return null;
            // 1) We remove once random card from player cards.
            //    We thow this card to the garbage.
            myCards.TakeCard();

            // 2) We add the opened card to player cards.
            if (openedCard != null)
            {
                myCards.Add(openedCard);
            }

            var card = package.TakeCard();
            if (card != null)
            {
                openedCard = card;
                pickedCardsCount++;
                PackageCardCount = package.Count; 
                IsHostTurn = !IsHostTurn;
                Move = [Keys.TakeFromPackage, 0];  
                UpdateFbMove(); 
            }
            return openedCard;

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

            HostHand = IsHostUser ? myCards.GetAllCards().Select(card => new CardData
            {
                Type = card.Type.ToString(),
                Value = card.Value,
                Index = card.Index
            }).ToList() : new List<CardData>();
            GuestHand = !IsHostUser ? myCards.GetAllCards().Select(card => new CardData
            {
                Type = card.Type.ToString(),
                Value = card.Value,
                Index = card.Index
            }).ToList() : new List<CardData>();

            Dictionary<string, object> dict = new()
            {
                { nameof(Move), Move },
                { nameof(IsHostTurn), IsHostTurn },
                { nameof(PackageCardCount), package.Count },
                { nameof(PickedCardsCount), pickedCardsCount },
                { nameof(PackageIndex), PackageIndex },
                { nameof(PackageCards), PackageCards },     
                { nameof(HostHand), HostHand },         
                { nameof(GuestHand), GuestHand },

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

                // טיפול במהלכי החלפה / השארה
                if (Move != null && Move.Count > 0)
                {
                    if (Move[0] == Keys.ReplaceCard)
                    {
                        // אין צורך לעשות כלום
                        // היד כבר עודכנה מה-Firestore
                        openedCard = null;
                    }
                    else if (Move[0] == Keys.SkipReplace)
                    {
                        openedCard = null;
                    }
                }

                UpdateStatus();

                if (_status.CurrentStatus == GameStatus.Statuses.Play)
                {
                    Play(false);
                }
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
        // החלפת קלף מהיד עם הקלף המוצג
        public void ReplaceCard(int handIndex)
        {
            if (!IsMyTurn || openedCard == null)
                return;

            var oldCard = myCards.Replace(handIndex, openedCard);

            openedCard = null; // הקלף מהחבילה "נזרק"

            IsHostTurn = !IsHostTurn; // מעביר את התור

            Move = new List<int> { Keys.ReplaceCard, handIndex };
            UpdateFbMove();
        }
        public void SkipReplace()
        {
            if (!IsMyTurn || openedCard == null)
                return;

            openedCard = null;

            IsHostTurn = !IsHostTurn; // מעביר את התור

            Move = new List<int> { Keys.SkipReplace, 0 };
            UpdateFbMove();
        }


    }
}
