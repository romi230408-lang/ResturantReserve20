using Firebase.Auth;
using Firebase.Auth.Providers;
using Plugin.CloudFirestore;
using ResturantReserve.Models;

namespace ResturantReserve.Models
{
    public abstract class FbDataModel
    {
        protected FirebaseAuthClient facl;
        protected IFirestore fdb;
        public string DisplayName => facl != null && facl.User != null ? facl.User.Info.DisplayName : string.Empty;
        public string UserId => facl != null ? facl.User.Uid : string.Empty;
        public abstract string GetErrorMessage(string errMessage);
        public abstract void CreateUserWithEmailAndPasswordAsync(string email, string password, string name, Action<System.Threading.Tasks.Task> OnComplete);
        public abstract void SignInWithEmailAndPasswordAsync(string email, string password, Action<System.Threading.Tasks.Task> OnComplete);
        public abstract string SetDocument(object obj, string collectonName, string id, Action<System.Threading.Tasks.Task> OnComplete);
        public abstract void UpdateFields(string collectonName, string id, Dictionary<string, object> dict, Action<Task> OnComplete);
        public abstract void DeleteDocument(string collectonName, string id, Action<Task> OnComplete);
        public abstract IListenerRegistration AddSnapshotListener(string collectonName, Plugin.CloudFirestore.QuerySnapshotHandler OnChange);
        public abstract IListenerRegistration AddSnapshotListener(string collectonName, string id, Plugin.CloudFirestore.DocumentSnapshotHandler OnChange);
        public FbDataModel()
        {
            FirebaseAuthConfig fac = new()
            {
                ApiKey = Keys.FbApiKey,
                AuthDomain = Keys.FbAppDomainKey,
                Providers = [new EmailProvider()]
            };
            facl = new FirebaseAuthClient(fac);
            fdb = CrossCloudFirestore.Current.Instance;
        }
    }
}

