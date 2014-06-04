namespace Backend.GrainInterfaces
{
    using System.Threading.Tasks;

    public interface IPlayerRegistrationGrain : Orleans.IGrain
    {
        Task Register(IPlayerObserver playerObserver);
    }
}
