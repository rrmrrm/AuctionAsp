using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Auction.Desktop.Model;
using Auction.Persistence;

namespace Auction.Desktop.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly AuctionApiService _service;

        private ObservableCollection<ItemLicitDTO> _items;
        public ObservableCollection<ItemLicitDTO> Items
        {
            get { return _items; }
            set { _items = value; OnPropertyChanged(); }
        }

        private ItemAllLicitsDTO _itemDetails;
        /// <summary>
        /// Tartalmazza a kiválasztott trágy részleteit
        /// </summary>
        public ItemAllLicitsDTO ItemDetails
        {
            get { return _itemDetails; }
            set { _itemDetails = value; OnPropertyChanged(); }
        }
        private ItemViewModel _editableItem;
        public ItemViewModel EditableItem
        {
            get { return _editableItem; }
            set { _editableItem = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CategoryDTO> _categories;
        public ObservableCollection<CategoryDTO> Categories
        {
            get { return _categories; }
            set { _categories = value; OnPropertyChanged(); }
        }

        public DelegateCommand RefreshItemsCommand { get; private set; }
        public DelegateCommand RefreshCategoriesCommand { get; private set; }
        public DelegateCommand SelectCommand { get; private set; }
        public DelegateCommand CloseLicitForSelectedItem { get; private set; }

        public DelegateCommand LogoutCommand { get; private set; }
        

        public DelegateCommand EditItemCommand { get; private set; }
        public DelegateCommand SaveItemEditCommand { get; private set; }
        public DelegateCommand CancelItemEditCommand { get; private set; }
        public DelegateCommand ChangeImageCommand { get; private set; }

        public Boolean IsLoggingOut { get; set; }
        public event EventHandler LogoutSucceeded;
        public event EventHandler LogoutFailed;

        public event EventHandler StartingItemEdit;
        public event EventHandler FinishingItemEdit;
        public event EventHandler StartingImageChange;

        public MainViewModel(AuctionApiService service)
        {
            IsLoggingOut = false;
            _service = service;

            RefreshItemsCommand = new DelegateCommand(_ => LoadAuctionItemsAsync());
            RefreshCategoriesCommand = new DelegateCommand(async _ => await LoadAuctionCategoriesAsync());//tegyél ide .Wait()-et, ha infiniteLoopot, vagy deadlockot akarsz todo: megnézni, hogy hogyan okozott ez deadlock szerűséget, tal'n ey> magyarázat :https://stackoverflow.com/questions/14526377/why-does-this-async-action-hang
            SelectCommand = new DelegateCommand(param => LoadAuctionItemDetailsAsync(param as ItemLicitDTO));
            CloseLicitForSelectedItem = new DelegateCommand(o => ( (ItemDetails?.item is null) ? false : ItemDTO.IsActiveFunc(ItemDetails.item) ),_ => CloseLicitAsync());
            LogoutCommand = new DelegateCommand(_ => !IsLoggingOut, _=> LogoutAsync());


            EditItemCommand = new DelegateCommand(_ => StartEditItem());
            SaveItemEditCommand = new DelegateCommand(_ => SaveItemEdit());
            CancelItemEditCommand = new DelegateCommand(_ => CancelItemEdit());
            ChangeImageCommand = new DelegateCommand(_ => StartingImageChange?.Invoke(this, EventArgs.Empty));
        }


        private async void LogoutAsync()
        {
            try
            {
                IsLoggingOut = true;
                await _service.LogoutAsync();
                LogoutSucceeded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                LogoutFailed?.Invoke(this, EventArgs.Empty);// ay App.xaml.cs-ben erre a loginFailed-ra kapcsolt eljaras pillanatnzilag nem csinal semmi hasznosat
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");
            }
            IsLoggingOut = false;
        }

        private async void LoadAuctionItemsAsync()
        {
            try
            {
                Items = new ObservableCollection<ItemLicitDTO>(await _service.LoadHirdetettAsync());
            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                OnMessageApplication($"Unexpected error occured while loading Items! ({ex.Message})");
            }
        }
        private async Task LoadAuctionCategoriesAsync()/////////MODIFIED
        {
            try
            {
                Categories = new ObservableCollection<CategoryDTO>(await _service.LoadCategoriesAsync());
            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");
            }
        }
        private async void LoadAuctionItemDetailsAsync(ItemLicitDTO ilDto)
        {
            if (ilDto is null)
            {
                OnMessageApplication($"Unexpected error occured! MainViewModel.LoadAuctionItemDetailsAsync(ItemLicitDTO): ItemLicitDTO cannot be null");
                return;
            }

            try
            {
                ItemAllLicitsDTO itemToDetail = await _service.LoadItemDetailsAsync(ilDto.ItemId);
                //ItemDetails = new ObservableCollection<ItemAllLicitsDTO>();
                //ItemDetails.Add(itemToDetail);
                ItemDetails = itemToDetail;
            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");
            }
        }
        private async void CloseLicitAsync()
        {
            ItemAllLicitsDTO ialDTO = ItemDetails;
            if (ialDTO is null)
            {
                OnMessageApplication($"No Items Selected");
                return;
            }

            try
            {
                ItemAllLicitsDTO closedItem = await _service.CloseLicit(ialDTO);
                //ItemDetails = new ObservableCollection<ItemAllLicitsDTO>();
                //ItemDetails.Add(itemToDetail);
                
                //a kiválasztott tárgyat lecseréljük a lezárt tárgyra DTO-jára, ami tartalmazza a nyertes felhasználónevét:
                ItemDetails = closedItem;
                if (closedItem?.winner?.UserName is null)
                {
                    OnMessageApplication("Licit lezárva, de a nyertes felhasználó lekérése sikertelen)");
                }
                else
                {
                    OnMessageApplication($"Licit sikeresen lezárva. Nyertes felhasználó: ({closedItem.winner.UserName})");
                }
            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");
                return;
            }
        }
        /*
        private async void CreateItemAsync(ItemDTO itemDTO)
        {
            try
            {
                Items = new ObservableCollection<ItemDTO>(await _service.LoadItemsAsync(listDto.Id));
            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");
            }
        }
        */

        private async void StartEditItem()
        {
            //LoadAuctionItemsAsync();/////////MODIFIED
            await LoadAuctionCategoriesAsync();
            if (Categories is null || Categories.Count <1)/////////MODIFIED
            {
                OnMessageApplication($"error occured in StartEditItem: no Categories could be loaded.");
                return;
            }
            //EditableItem = SelectedItem.ShallowClone();
            EditableItem = new ItemViewModel
            {
                CategoryId = Categories.First(c => true).Id,
                DateOfClosing = DateTime.Now.AddMonths(1),
                Description = "",
                Name = "",
                Picture = null,
                StartingLicit = 1
            };
            StartingItemEdit?.Invoke(this, EventArgs.Empty);
        }
        private void CancelItemEdit()
        {
            EditableItem = null;
            FinishingItemEdit?.Invoke(this, EventArgs.Empty);
        }
        private async void SaveItemEdit()
        {
            try
            {
                //EditableItem.CopyFrom(EditableItem);
                if (EditableItem == null)
                {
                    OnMessageApplication("no item to create");
                    return;
                }
                ItemDTO itemDTO = null;
                // ha az elküldendő tárgy egyik szükséges property-je null volt(amit a konverzió során eredményez), 
                // akkor nem próbáljuk meg menteni a tárgyat, hanem már itt jelezzük a hibát a felhasználónak:
                try {
                    itemDTO = (ItemDTO)EditableItem;

                    var validationContext = new ValidationContext(itemDTO, null, null);
                    var validationResults = new List<ValidationResult>();
                    if(!Validator.TryValidateObject(itemDTO, validationContext, validationResults, true))
                    {
                        for (int i = 0; i < validationResults.Count; ++i)
                        {
                            var err = validationResults.ElementAt(i);
                            OnMessageApplication($"item state is is invalid:{err.MemberNames.ElementAt(i)}: {err.ErrorMessage}");
                            i += 1;
                        }
                        OnMessageApplication("item state is is invalid. fill all requred properties for the item to be saved , then try again");
                        throw new Exception();
                    }
                    
                }
                catch
                {
/*
                    var validationContext = new ValidationContext(itemDTO, null, null);
                    var validationResults = new List<ValidationResult>();
                    Validator.TryValidateObject(itemDTO, validationContext, validationResults, true);
                    for (int i = 0; i < validationResults.Count; ++i)
                    {
                        var err = validationResults.ElementAt(i);
                        OnMessageApplication($"item state is is invalid:{err.MemberNames.ElementAt(i)}: {err.ErrorMessage}");
                        i += 1;
                    }*/
                    OnMessageApplication("item state is is invalid. fill all requred properties for the item to be saved , then try again");
                    return;
                }
                await _service.CreateItemAsync(itemDTO);
                /*if (SelectedItem.ListId != SelectedList.Id)
                {
                    Items.Remove(SelectedItem);
                    SelectedItem = null;
                }
                */
                FinishingItemEdit?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex) when (ex is NetworkException || ex is HttpRequestException)
            {
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");
            }
            catch (Exception ex) when (ex is ValidationException)
            {
                OnMessageApplication($"Unexpected error occured! ({ex.Message})");
            }
        }




    }
}
