using ResturantReserve.ViewModels;

namespace ResturantReserve.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageVM mpVM = new();
        public MainPage()
        {
            InitializeComponent();
            BindingContext = mpVM;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            mpVM.AddSnapshotListener();
        }

        protected override void OnDisappearing()
        {
            mpVM.RemoveSnapshotListener();
            base.OnDisappearing();
        }
    }

}
