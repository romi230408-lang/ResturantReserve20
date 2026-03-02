using System.Windows.Input;
using ResturantReserve.Models;
using ResturantReserve.ModelsLogic;
using ResturantReserve.Views;

namespace ResturantReserve.ViewModels
{
    public partial class AuthPageVM : ObservableObject
    {
        private readonly User user = new();
        public ICommand AuthCommand { get; }
        public ICommand ToggleIsPasswordCommand { get; }
        public bool IsBusy => user.IsBusy;
        public bool IsRegistered => user.IsRegistered;
        public string UserStateAction => user.IsRegistered ? Strings.Login : Strings.Register;
        public string Name
        {
            get => user.Name;
            set
            {
                if (user.Name != value)
                {
                    user.Name = value;
                    (AuthCommand as Command)?.ChangeCanExecute();
                }
            }
        }
        public string Email
        {
            get => user.Email;
            set
            {
                if (user.Email != value)
                {
                    user.Email = value;
                    (AuthCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public string Password
        {
            get => user.Password;
            set
            {
                if (user.Password != value)
                {
                    user.Password = value;
                    (AuthCommand as Command)?.ChangeCanExecute();
                }
            }
        }
        public bool IsPassword { get; set; } = true;

        public AuthPageVM()
        {
            AuthCommand = user.IsRegistered ? new Command(Login, CanAuth) : new Command(Register, CanAuth);
            ToggleIsPasswordCommand = new Command(ToggleIsPassword);
            user.OnAuthComplete += OnAuthComplete;
        }



        private void OnAuthComplete(object? sender, bool success)
        {
            OnPropertyChanged(nameof(IsBusy));
            if (success && Application.Current != null ||true)
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Application.Current.MainPage = new AppShell();
                });
            }
        }

        private bool CanAuth()
        {
            return user.IsValid();
        }
        private void Login()
        {
            user.Login();
            OnPropertyChanged(nameof(IsBusy));
        }
        private void Register()
        {
            user.Register();
            OnPropertyChanged(nameof(IsBusy));
        }

        private void ToggleIsPassword()
        {
            IsPassword = !IsPassword;
            OnPropertyChanged(nameof(IsPassword));
        }
    }
}
