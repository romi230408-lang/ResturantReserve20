using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using ResturantReserve.Models;

namespace ResturantReserve.ModelsLogic
{
    public class User : UserModel
    {
        public override void Register()
        {
            IsBusy = true;
            CurrentAction = Actions.Register;
            fbd.CreateUserWithEmailAndPasswordAsync(Email, Password, Name, OnComplete);
        }
        public void Login()
        {
            IsBusy = true;
            fbd.SignInWithEmailAndPasswordAsync(Email, Password, OnComplete);
        }
        private void OnComplete(Task task)
        {
            IsBusy = false;
            if (task.IsCompletedSuccessfully)
            {
                if (CurrentAction == Actions.Register)
                    SaveToPreferences();
                OnAuthComplete?.Invoke(this, true);
            }
            else if (task.Exception != null)
            {
                string errMessage = task.Exception.Message;
                ShowAlert(errMessage);
                OnAuthComplete?.Invoke(this, false);
            }
            else
                ShowAlert(Strings.UnknownError);
        }

        private void ShowAlert(string errMessage)
        {
            errMessage = fbd.GetErrorMessage(errMessage);
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                Toast.Make(errMessage, ToastDuration.Long).Show();
            });
        }
        private void SaveToPreferences()
        {
            Preferences.Set(Keys.NameKey, Name);
            Preferences.Set(Keys.EmailKey, Email);
            Preferences.Set(Keys.PasswordKey, Password);
        }

        public override bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Password) && !string.IsNullOrWhiteSpace(Email);
        }


        

        public User()
        {
            Name = Preferences.Get(Keys.NameKey, string.Empty);
            Email = Preferences.Get(Keys.EmailKey, string.Empty);
            Password = Preferences.Get(Keys.PasswordKey, string.Empty);
            UserName = Preferences.Get(Keys.NameKey, string.Empty);
        }
    }
}

