namespace Backend.GrainInterfaces
{
    public interface ILobbyGrainState : Orleans.IGrainState
    {
        string SomeCrap { get; set; }
    }
}
