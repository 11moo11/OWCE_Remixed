namespace OWCE
{
    /// <summary>
    /// Settings the board reports/accepts on <see cref="OWBoard.RideTraitsUUID"/>.
    /// The numeric values are the trait ids used on the wire.
    /// </summary>
    public enum RideTrait : byte
    {
        AngleOffset = 0,
        TurnCompensation = 1,
        Aggressiveness = 2,
        SimpleStop = 3,
    }
}
