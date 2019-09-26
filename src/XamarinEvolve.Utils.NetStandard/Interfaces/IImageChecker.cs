using System.Threading.Tasks;

namespace XamarinEvolve.Utils
{
    public interface IImageChecker
    {
        Task<bool> IsImageUsableForAppLink(string url);
    }
}
