using Steamworks;

namespace CWAPI.Extensions
{
    public static class PlayerExtensions
    {
        public static bool TryGetSteamID(this Player player, out CSteamID steamId)
        {
            steamId = CSteamID.Nil;
            if (player?.refs?.view?.Owner?.CustomProperties?.ContainsKey("SteamID") != true) return false;
            if (!ulong.TryParse(player.refs.view.Owner.CustomProperties["SteamID"] as string, out ulong ulSteamID)) return false;
            steamId = new(ulSteamID);
            return true;
        }
    }
}
