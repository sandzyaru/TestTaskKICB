using Microsoft.EntityFrameworkCore;
using TestTaskKICB.Application.Repositories;
using TestTaskKICB.Domain.Models;
using TestTaskKICB.Infrastructure.DB;
using TestTaskKICB.Infrastructure.Models;

namespace TestTaskKICB.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BankDbContext db;

    public CustomerRepository(BankDbContext db)
    {
        this.db = db;
    }

    public bool Exists(long id)
    {
        return db.Customers.Any(customer => customer.Id == id);
    }

    public IReadOnlyList<Customer> ListCustomers()
    {
        return db.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .ThenBy(customer => customer.Id)
            .ToList()
            .Select(row => row.ToDomain())
            .ToArray();
    }

    public Customer? FindCustomer(long id)
    {
        return db.Customers.Find(id)?.ToDomain();
    }

    public Customer Add(Customer customer)
    {
        var row = new CustomerRow();
        SetCustomerFields(row, customer);
        db.Customers.Add(row);
        db.SaveChanges();
        return row.ToDomain();
    }

    public void Update(Customer customer)
    {
        var row = db.Customers.Find(customer.Id)!;
        SetCustomerFields(row, customer);
        db.Customers.Update(row);
        db.SaveChanges();
    }

    public void Remove(Customer customer)
    {
        var row = db.Customers.Find(customer.Id)!;
        db.Customers.Remove(row);
        db.SaveChanges();
    }

    public IReadOnlyList<Phone> ListPhones(long customerId)
    {
        return db.Phones
            .AsNoTracking()
            .Where(phone => phone.CustomerId == customerId)
            .OrderBy(phone => phone.Id)
            .ToList()
            .Select(row => row.ToDomain())
            .ToArray();
    }

    public Phone? FindPhone(long id)
    {
        return db.Phones.Find(id)?.ToDomain();
    }

    public Phone Add(Phone phone)
    {
        var row = new PhoneRow
        {
            CustomerId = phone.CustomerId,
            Number = phone.Number,
            Type = phone.Type
        };
        db.Phones.Add(row);
        db.SaveChanges();
        return row.ToDomain();
    }

    public void Update(Phone phone)
    {
        var row = db.Phones.Find(phone.Id)!;
        row.Number = phone.Number;
        row.Type = phone.Type;

        var entry = db.Entry(row);
        entry.Property(item => item.Number).IsModified = true;
        entry.Property(item => item.Type).IsModified = true;
        db.SaveChanges();
    }

    public void Remove(Phone phone)
    {
        var row = db.Phones.Find(phone.Id)!;
        db.Phones.Remove(row);
        db.SaveChanges();
    }

    private static void SetCustomerFields(CustomerRow row, Customer customer)
    {
        row.LastName = customer.LastName;
        row.FirstName = customer.FirstName;
        row.Patronymic = customer.Patronymic;
        row.Email = customer.Email;
        row.NormalizedEmail = customer.NormalizedEmail;
        row.DateOfBirth = customer.DateOfBirth;
    }
}
