namespace FinalWar
{
    public interface IBattleInitDataSDS
    {
        int GetMapID();
        int GetMaxRoundNum();
        IPlayerInitDataSDS GetMPlayerInitData();
        IPlayerInitDataSDS GetOPlayerInitData();
    }

    public interface IPlayerInitDataSDS
    {
        int GetDeckCardsNum();
        int GetAddCardsNum();
        int GetAddMoney();
        int GetDefaultHandCardsNum();
        int GetDefaultMoney();
    }
}
