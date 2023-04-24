using Auction.Persistence.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Auction.Persistence
{
    /// <summary>
    /// Foglalás dátumát ellenőrző típus.
    /// </summary>
    public class LicitValidator
    {
        private readonly HomeService homeService;
		public LicitValidator(HomeService _homeService)
		{
			//context = _context;
            homeService = _homeService;

        }
        
        /// <summary>
        /// Foglalás dátumainak ellenőrzése.
        /// </summary>
        /// <param name="start">Foglalás kezdete.</param>
        /// <param name="end">Foglalás vége.</param>
        /// <param name="apartmentId">Apartman azonosítója.</param>
        public LicitError Validate(LicitVM lvm)
        {
            if (homeService.getItem(lvm.Item.Id) == null)
            {
                return LicitError.ItemIdInvalid; ;// "nem létezik ilyen azonosítójú tárgy(az elküldött objektum hibás)";
            }
            if (!homeService.IsItemActive(lvm.Item.Id))
            {
                return LicitError.LicitIsOver;// = "Már lezárult a tárgyra a licitálás";
            }
            Boolean isStartingLicit;
            Int32 previousLicit = homeService.GetActualLicitValueForItem(lvm.Item.Id, out isStartingLicit);
            if (isStartingLicit)
            {
                if (lvm.Value < previousLicit)
                {
                    return LicitError.LicitSmallThanPrevious;//"a megadott licit kisebb, mint a kezdőlicit!"
                }
                else
                {

                }
            }
            else
            {
                if (lvm.Value <= previousLicit)
                {
                    return LicitError.LicitSmallThanInitial;//failReason = "a licit nagyobb kell legyen az előző licitnél!";
                }
                else
                {

                }
            }
            return LicitError.None;
        }
        
    }
}