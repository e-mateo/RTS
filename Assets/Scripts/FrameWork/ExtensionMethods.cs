public static class ExtensionMethods
{
    public static ETeam GetOpponent(this ETeam team)
    {
        return team == ETeam.Blue ? ETeam.Red : ETeam.Blue;
    }
}
