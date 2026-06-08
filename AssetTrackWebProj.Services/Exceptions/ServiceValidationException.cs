namespace AssetTrack.Services.Exceptions
{
    /// <summary>
    /// Thrown by the service layer when a business/validation rule is violated.
    /// Controllers translate this into ModelState errors so the user gets feedback.
    /// </summary>
    public class ServiceValidationException : Exception
    {
        public ServiceValidationException(string message) : base(message)
        {
        }
    }
}
