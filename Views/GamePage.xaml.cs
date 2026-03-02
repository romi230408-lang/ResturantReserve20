using ResturantReserve.ModelsLogic;
using ResturantReserve.ViewModels;

namespace ResturantReserve.Views;


public partial class GamePage : ContentPage
{
    private readonly GamePageVM gpVM;
    public GamePage(Game game)
    {
        InitializeComponent();
        gpVM = new GamePageVM(game);
        BindingContext = gpVM;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        gpVM.AddSnapshotListener();
    }

    protected override void OnDisappearing()
    {
        gpVM.RemoveSnapshotListener();
        base.OnDisappearing();
    }
}