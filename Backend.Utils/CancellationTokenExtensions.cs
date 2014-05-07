namespace Backend.Utils
{
    using System.Threading;

    public static class CancellationTokenExtensions
    {
        public static CancellationTokenSource DerivedToken(this CancellationToken existingToken, out CancellationToken newTokenSource)
        {
            var childCtsTwo = new CancellationTokenSource();

            newTokenSource = CancellationTokenSource.CreateLinkedTokenSource(existingToken, childCtsTwo.Token).Token;

            return childCtsTwo;
        }
    }
}