using CommunityToolkit.Maui.Alerts;
using Plugin.CloudFirestore;
using ResturantReserve.Models;

namespace ResturantReserve.ModelsLogic
{
    public class Games : GamesModel
    {
        public void AddGame()
        {
            IsBusy = true;
            currentGame = new(true);
            currentGame.OnGameDeleted += OnGameDeleted;
            currentGame.SetDocument(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    currentGame.InitializeNewGame();  

                    OnGameAdded?.Invoke(this, currentGame);
                }

                IsBusy = false;
            });
        }

        private void OnGameDeleted(object? sender, EventArgs e)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Toast.Make(Strings.GameDeleted, CommunityToolkit.Maui.Core.ToastDuration.Long, 14).Show();
            });
        }

        private void OnComplete(Task task)
        {
            IsBusy = false;

            if (currentGame?.Id != null)
            {
                currentGame.AddSnapshotListener();
            }

            OnGameAdded?.Invoke(this, currentGame!);
        }

        public Games()
        {

        }
        public override void AddSnapshotListener()
        {
            ilr = fbd.AddSnapshotListener(Keys.GamesCollection, OnChange!);
        }
        public override void RemoveSnapshotListener()
        {
            ilr?.Remove();
        }
        private void OnChange(IQuerySnapshot snapshot, Exception error)
        {
            fbd.GetDocumentsWhereEqualTo(Keys.GamesCollection, nameof(GameModel.IsFull), false, OnComplete);
        }

        private void OnComplete(IQuerySnapshot qs)
        {
            GamesList!.Clear();
            foreach (IDocumentSnapshot ds in qs.Documents)
            {
                Game? game = ds.ToObject<Game>();
                if (game != null)
                {
                    game.Id = ds.Id;
                    game.IsHostUser = false;
                    GamesList.Add(game);
                }
            }
            OnGamesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
