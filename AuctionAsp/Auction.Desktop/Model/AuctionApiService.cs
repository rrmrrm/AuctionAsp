using Auction.Persistence;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
//using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Auction.Desktop.Model
{
    public class AuctionApiService
    {
        private readonly HttpClient _client;
        private const String ItPrefix = "api/Items";
        public AuctionApiService(string baseAddress)
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress)
            };
        }

        public async Task<IEnumerable<ItemLicitDTO>> LoadHirdetettAsync()
        {
            var resp = await _client.GetAsync(ItPrefix + "/hirdetett");
            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadAsAsync<IEnumerable<ItemLicitDTO>>();
            }
            throw new NetworkException("AuctionApiService: LoadHirdetettAsync: Service Retuned " + resp.StatusCode);
        }
        public async Task<ItemDTO> LoadItem(Int32 itemId)
        {
            var res = await _client.GetAsync(ItPrefix + "/" + itemId);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsAsync<ItemDTO>();
            }
            throw new NetworkException("AuctionApiService: GetAsync: Service Retuned " + res.StatusCode);
        }

        public async Task<ItemAllLicitsDTO> LoadItemDetailsAsync(Int32 itemId)
        {
            var res = await _client.GetAsync(ItPrefix + "/details/" + itemId);
            if (res.IsSuccessStatusCode)
            {
                return await res.Content.ReadAsAsync<ItemAllLicitsDTO>();
            }
            throw new NetworkException("AuctionApiService: LoadItemDetails: Service Retuned " + res.StatusCode);

        }
        public async Task<ItemAllLicitsDTO> CloseLicit(ItemAllLicitsDTO ialDTO)
        {
            var res = await _client.PutAsJsonAsync(ItPrefix + "/closeLicit", ialDTO);
            if (res.IsSuccessStatusCode)
            {
                ItemAllLicitsDTO closedItem = await res.Content.ReadAsAsync<ItemAllLicitsDTO>();
                return closedItem;
            }
            else
            {
                switch (res.StatusCode)
                {
                    case (HttpStatusCode)StatusCodes.Status404NotFound:
                        {
                            throw new NetworkException("AuctionApiService: LoadItemDetails: Service Retuned " + res.StatusCode + "Nem létezik ilyen tárgy");
                        }
                    case (HttpStatusCode)StatusCodes.Status405MethodNotAllowed:
                        {
                            throw new NetworkException("AuctionApiService: LoadItemDetails: Service Retuned " + res.StatusCode + "A licit még nem indult el, vagy már lezárult");
                        }

                    case (HttpStatusCode)StatusCodes.Status401Unauthorized:
                        {
                            throw new NetworkException("AuctionApiService: LoadItemDetails: Service Retuned " + res.StatusCode + " Csak az on altal hirdetett licitalast yzrhatja le");
                        }
                    case (HttpStatusCode)StatusCodes.Status418ImATeapot:
                        {
                            throw new NetworkException("AuctionApiService: LoadItemDetails: Service Retuned " + res.StatusCode + " nem licitaltak meg a targyra, ezert nem zarhato le");
                        }
                    case (HttpStatusCode)StatusCodes.Status403Forbidden:
                        {
                            throw new NetworkException("AuctionApiService: LoadItemDetails: Service Retuned " + res.StatusCode + " nem licitaltak meg a targyra, ezert nem zarhato le");
                        }
                    default:
                        throw new NetworkException("AuctionApiService: LoadItemDetails: Service Retuned " + res.StatusCode);
                }
            }
        }
        public async Task CreateItemAsync(ItemDTO item)
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync(ItPrefix, item);
            if (!response.IsSuccessStatusCode)
            {
                List<ValidationResult> errors = new List<ValidationResult>();
                var stuff =response.Content.ReadAsAsync<Microsoft.AspNetCore.Mvc.SerializableError>();
                try
                {
                    for (int i = 0; i < stuff.Result.Keys.Count; ++i)
                    {
                        errors.Add(new ValidationResult(stuff.Result.Values.ToList().ElementAt(i).ToString(), stuff.Result.Keys.ToList()));
                    }
                    throw new ValidationException();//itt todo folytatni
                }
                catch(Exception e)
                {

                }
                throw new NetworkException("Service returned response: " + response.StatusCode);
            }
            item.Id = (await response.Content.ReadAsAsync<ItemLicitDTO>()).ItemId;
        }

        public async Task<IEnumerable<CategoryDTO>> LoadCategoriesAsync()
        {
            HttpResponseMessage response = await _client.GetAsync(ItPrefix+ "/Categories");

            if (!response.IsSuccessStatusCode)
            {
                throw new NetworkException("Service returned response: " + response.StatusCode);
            }
            return await response.Content.ReadAsAsync< IEnumerable<CategoryDTO> >();
        }

        /*
public async Task<ItemDTO> SaveItemAsync(ItemLicitDTO item)
{
   //ide kell a "PostItem", vagz "Item" string az Uri-be????????????????????????????
   await _client.PostAsync(ItPrefix +"Item", item?.);//ide kell a "PostItem", vagz "Item" string az Uri-be????????????????????????????
}*/

        public async Task<bool> LoginAsync(string userName, string password)
        {
            LoginDTO user = new LoginDTO
            {
                UserName = userName,
                Password = password
            };

            var response = await _client.PostAsJsonAsync("api/Account/Login", user);

            if (response.IsSuccessStatusCode)
                return true;
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return false;

            throw new NetworkException("Service returned response: " + response.StatusCode);
        }

        public async Task LogoutAsync()
        {
            HttpResponseMessage response = await _client.GetAsync("api/Account/Logout");////MODIFIED

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            throw new NetworkException("Service returned response: " + response.StatusCode);
        }
    }
}
