using CommunityToolkit.Mvvm.Messaging.Messages;

namespace ResturantReserve.Models
{
    public class AppMessage<T>(T msg) : ValueChangedMessage<T>(msg)
    {

    }
}
