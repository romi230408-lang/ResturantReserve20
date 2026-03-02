using ResturantReserve.ModelsLogic;
using ResturantReserve.Views;

namespace ResturantReserve
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AuthPage();
        }
    }
}
