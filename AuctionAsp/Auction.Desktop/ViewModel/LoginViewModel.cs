using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows.Controls;
using Auction.Desktop.Model;

namespace Auction.Desktop.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuctionApiService _service;

        public DelegateCommand LoginCommand { get; set; }

        public Boolean IsLoading { get; set; }

        public String UserName { get; set; }

        public event EventHandler LoginSucceeded;
        public event EventHandler LoginFailed;

        public LoginViewModel(AuctionApiService service)
        {
            _service = service;
            IsLoading = false;

            LoginCommand = new DelegateCommand(_ => !IsLoading, param => LoginAsync(param as PasswordBox));
        }

        private async void LoginAsync(PasswordBox passwordBox)
        {
            try
            {
                IsLoading = true;
                Boolean result = await _service.LoginAsync(UserName, passwordBox.Password);

                if (result)
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
                else
                    LoginFailed?.Invoke(this, EventArgs.Empty);
                IsLoading = false;

            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");                
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
