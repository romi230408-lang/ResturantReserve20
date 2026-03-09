using ResturantReserve.Models;
using ResturantReserve.Views;
using ResturantReserve.ModelsLogic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ResturantReserve.ViewModels
{
    public partial class MainPageVM : ObservableObject
    {
        private readonly User user = new();
        private readonly Games games = new();
        public ICommand AddGameCommand => new Command(AddGame);
        public bool IsBusy => games.IsBusy;
        public ObservableCollection<Game>? GamesList => games.GamesList;

        public Game? SelectedItem
        {
            get => games.CurrentGame;

            set
            {
                if (value != null)
                {
                    games.CurrentGame = value;
                    MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Shell.Current.Navigation.PushAsync(new GamePage(value), true);
                    });
                }
            }
        }

        private void AddGame()
        {
            games.AddGame();
            OnPropertyChanged(nameof(IsBusy));
        }

        public MainPageVM()
        {
            games.OnGameAdded += OnGameAdded;
            games.OnGamesChanged += OnGamesChanged;
        }

        private void OnGamesChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(GamesList));
        }

        private void OnGameAdded(object? sender, Game game)
        {
            OnPropertyChanged(nameof(IsBusy));
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Shell.Current.Navigation.PushAsync(new GamePage(game), true);
            });
        }
        public void AddSnapshotListener()
        {
            games.AddSnapshotListener();
        }

        public void RemoveSnapshotListener()
        {
            games.RemoveSnapshotListener();
        }
    }
}

