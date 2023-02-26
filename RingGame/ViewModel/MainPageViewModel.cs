using System.Net.WebSockets;
using CommunityToolkit.Mvvm;

namespace RingGame.ViewModel;

public partial class MainPageViewModel : BaseViewModel
{

    [ObservableProperty]
    string message;

    ServerConnect server;
    public MainPageViewModel()
    {
        Title = "Excel Sift";
        server = new ServerConnect(this);
    }
}
