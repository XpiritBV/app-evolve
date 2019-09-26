using System.Net.Http;

namespace XamarinEvolve.Utils
{
    public interface IMessageHandlerProvider
    {
        HttpMessageHandler GetHandler();
    }
}