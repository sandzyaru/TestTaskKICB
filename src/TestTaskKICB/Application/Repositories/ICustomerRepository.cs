using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.Repositories;

public interface ICustomerRepository
{
    bool Exists(long id);
    IReadOnlyList<Customer> ListCustomers();
    Customer? FindCustomer(long id);
    Customer Add(Customer customer);
    void Update(Customer customer);
    void Remove(Customer customer);

    IReadOnlyList<Phone> ListPhones(long customerId);
    Phone? FindPhone(long id);
    Phone Add(Phone phone);
    void Update(Phone phone);
    void Remove(Phone phone);
}
