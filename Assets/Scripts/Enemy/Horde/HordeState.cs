public sealed partial class HordeManager
{
    public enum HordeState
    {
        Disabled = 0,
        SpawningHorde = 1,
        WaitingForAllEnemiesDead = 2,
        WaitingForNextHorde = 3
    }
}