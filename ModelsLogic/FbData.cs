using Plugin.CloudFirestore;
using ResturantReserve.Models;
using System.Text.RegularExpressions;


namespace ResturantReserve.ModelsLogic
{
    public partial class FbData : FbDataModel
    {
        public override async void CreateUserWithEmailAndPasswordAsync(string email, string password, string name, Action<System.Threading.Tasks.Task> OnComplete)
        {
                await facl.CreateUserWithEmailAndPasswordAsync(email, password, name).ContinueWith(OnComplete);
        }
        public override async void SignInWithEmailAndPasswordAsync(string email, string password, Action<System.Threading.Tasks.Task> OnComplete)
        {
                await facl.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(OnComplete);
        }
        public override string SetDocument(object obj, string collectonName, string id, Action<System.Threading.Tasks.Task> OnComplete)
        {
            IDocumentReference dr = string.IsNullOrEmpty(id) ? fdb.Collection(collectonName).Document() : fdb.Collection(collectonName).Document(id);
            dr.SetAsync(obj).ContinueWith(OnComplete);
            return dr.Id;
        }


        public override string GetErrorMessage(string errMessage)
        {
            string retMessage = errMessage;
            int end, start = errMessage.IndexOf(Keys.MessageKey);
            if (start > 0)
            {
                end = errMessage.IndexOf(Keys.ErrorsKey, start);
                string title = errMessage[(start + Keys.MessageKey.Length)..end]
                    .Replace(Keys.Apostrophe, string.Empty)
                    .Replace(Keys.Colon, string.Empty)
                    .Replace(Keys.Comma, string.Empty)
                    .Trim();
                title = string.Join(Keys.WordsDelimiter, title.Split(Keys.TitleDelimiter));
                errMessage = errMessage[(errMessage.IndexOf(Keys.ReasonKey) +
                    Keys.ReasonKey.Length)..];
                errMessage = string.Join(Keys.WordsDelimiter,
                    Regex.Split(errMessage, Keys.UpperCaseDelimiter)).Trim();
                retMessage = title + Keys.NewLine + Keys.ReasonKey +
                   Keys.WordsDelimiter + errMessage[..^1];
            }

            return retMessage;
               
        }
        public override IListenerRegistration AddSnapshotListener(string collectonName, Plugin.CloudFirestore.QuerySnapshotHandler OnChange)
        {
            ICollectionReference cr = fdb.Collection(collectonName);
            return cr.AddSnapshotListener(OnChange);
        }
        public override IListenerRegistration AddSnapshotListener(string collectonName, string id, Plugin.CloudFirestore.DocumentSnapshotHandler OnChange)
        {
            IDocumentReference cr = fdb.Collection(collectonName).Document(id);
            return cr.AddSnapshotListener(OnChange);
        }
        public async void GetDocumentsWhereEqualTo(string collectonName, string fName, object fValue, Action<IQuerySnapshot> OnComplete)
        {
            ICollectionReference cr = fdb.Collection(collectonName);
            IQuerySnapshot qs = await cr.WhereEqualsTo(fName, fValue).GetAsync();
            OnComplete(qs);
        }
        public override async void UpdateFields(string collectonName, string id, Dictionary<string, object> dict, Action<Task> OnComplete)
        {
            IDocumentReference dr = fdb.Collection(collectonName).Document(id);
            await dr.UpdateAsync(dict).ContinueWith(OnComplete);
        }
        public override async void DeleteDocument(string collectonName, string id, Action<Task> OnComplete)
        {
            IDocumentReference dr = fdb.Collection(collectonName).Document(id);
            await dr.DeleteAsync().ContinueWith(OnComplete);
        }
    }
}


