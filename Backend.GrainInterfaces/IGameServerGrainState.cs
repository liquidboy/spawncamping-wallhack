namespace Backend.GrainInterfaces
{
    using System.Collections.Generic;

    public interface IGameServerGrainState : Orleans.IGrainState
    {
        List<string> Players { get; set; }
    }
}
