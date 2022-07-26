using Auction.Desktop.Model;
using Auction.Desktop.View;
using Auction.Desktop.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Auction.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AuctionApiService _service;
        private MainViewModel _mainViewModel;
        private MainWindow _mainView;
        private LoginViewModel _loginViewModel;
        private LoginWindow _loginView;
        private ItemEditorWindow _editorView;

        public App()
        {
            Startup += AppStartup;
        }

        private void AppStartup(object sender, StartupEventArgs e)
        {
            _service = new AuctionApiService(ConfigurationManager.AppSettings["baseAddress"]);

            
            _loginViewModel = new LoginViewModel(_service);
            _loginViewModel.LoginSucceeded += _loginViewModel_LoginSucceded;
            _loginViewModel.LoginFailed += _loginViewModel_LoginFailed;
            _loginViewModel.MessageApplication += onMessageApplication;

            _loginView = new LoginWindow
            {
                DataContext = _loginViewModel
            };

            _mainViewModel = new MainViewModel(_service);
            _mainViewModel.MessageApplication += onMessageApplication;
            _mainViewModel.LogoutSucceeded += _mainViewModel_LogoutSucceded;
            _mainViewModel.LogoutFailed += _mainViewModel_LogoutFailed;
            _mainViewModel.StartingItemEdit += _mainViewModel_StartingItemEdit;
            _mainViewModel.FinishingItemEdit += _mainViewModel_FinishingItemEdit;
            _mainViewModel.StartingImageChange += _mainViewModel_StartingImageChange;
            _mainView = new MainWindow
            {
                DataContext = _mainViewModel
            };
            _loginView.Show();
        }

        private void _loginViewModel_LoginSucceded(object sender, EventArgs e)
        {
            _loginView.Hide();
            _mainView.Show();
            _loginViewModel.IsLoading = false;
        }
        private void _loginViewModel_LoginFailed(object sender, EventArgs e)
        {
            MessageBox.Show("Login Failed", "Auction", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void _mainViewModel_LogoutSucceded(object sender, EventArgs e)
        {
            _loginView.Show();
            _mainView.Hide();
            _loginViewModel.IsLoading = false;
        }
        private void _mainViewModel_LogoutFailed(object sender, EventArgs e)
        {
            _mainViewModel.IsLoggingOut = false;
        }
        private void onMessageApplication(object sender, MessageEventArgs e)
        {
            MessageBox.Show(e.Message, "Auction", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }


        private void _mainViewModel_StartingItemEdit(object sender, EventArgs e)
        {
            _editorView = new ItemEditorWindow
            {
                DataContext = _mainViewModel
            };
            _editorView.ShowDialog();
        }
        private void _mainViewModel_FinishingItemEdit(object sender, EventArgs e)
        {
            if (_editorView.IsActive)
            {
                _editorView.Close();
            }
        }
        private async void _mainViewModel_StartingImageChange(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Images|*.jpg;*.jpeg;*.bmp;*.tif;*.gif;*.png;",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };

            if (dialog.ShowDialog(_editorView).GetValueOrDefault(false))
            {
                /////MODIFIED
                //this:
                //-----------
                //_mainViewModel.EditableItem.Picture = await File.ReadAllBytesAsync(dialog.FileName);
                //----------
                //replaced with this:----------------------------------------
                var path = dialog.FileName;
                if (path == null)
                    throw new ArgumentNullException(nameof(path));
                try
                {
                    BitmapImage image = new BitmapImage(); // kép betöltése
                    image.BeginInit();
                    image.UriSource = new Uri(path);
                    image.DecodePixelHeight = 100; // megadott méretre
                    image.EndInit();

                    PngBitmapEncoder encoder = new PngBitmapEncoder(); // átalakítás PNG formátumra
                    encoder.Frames.Add(BitmapFrame.Create(image));

                    using (MemoryStream stream = new MemoryStream()) // átalakítás byte-tömbre
                    {
                        encoder.Save(stream);
                        _mainViewModel.EditableItem.Picture = stream.ToArray();
                    }
                }
                catch { 
                    onMessageApplication( this, new MessageEventArgs("Hiba lépett fela a kép betöltésekor") ); 
                }
                //------------------------------------------------------------
            }
        }
    }
}
