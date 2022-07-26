namespace Auction.Persistence
{
    /// <summary>
    /// Foglalási dátum hiba felsorolási típusa.
    /// </summary>
    public enum LicitError
    {

        /// <summary>
        /// Nincs hiba.
        /// </summary>
        None,

        /// <summary>
        /// nem létezik ilyen azonosítójú tárgy(a kliens által elküldött objektum hibás id-t tartalmaz)
        /// </summary>
        ItemIdInvalid,

        /// <summary>
        /// Már lezárult a tárgyra a licitálás
        /// </summary>
        LicitIsOver,

        /// <summary>
        /// a megadott licit kisebb, mint a kezdőlicit
        /// </summary>
        LicitSmallThanPrevious,

        /// <summary>
        /// a megadott licit kisebb, mint az előző vezetőlicit
        /// </summary>
        LicitSmallThanInitial
    }
}