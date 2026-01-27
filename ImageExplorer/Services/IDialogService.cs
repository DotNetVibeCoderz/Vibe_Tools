using System.Threading.Tasks;

namespace ImageExplorer.Services
{
    public interface IDialogService
    {
        Task<string?> OpenFileDialogAsync();
        Task<string?> OpenFolderDialogAsync();
        Task<string?> SaveFileDialogAsync(string defaultName);
        Task<(int Width, int Height)?> ShowResizeDialogAsync(int currentWidth, int currentHeight);
        Task<float?> ShowRotateDialogAsync();
        Task ShowBatchConvertDialogAsync();
        Task ShowMessageAsync(string title, string message);
    }
}