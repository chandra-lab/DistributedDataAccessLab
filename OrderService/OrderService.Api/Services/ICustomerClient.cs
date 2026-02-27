public interface ICustomerClient
{
    Task<bool> CustomerExistsAsync(int customerId);
}