using TestTaskKICB.Application.DTO;
using TestTaskKICB.Application.Repositories;
using TestTaskKICB.Application.Validation;
using TestTaskKICB.Domain.Models;

namespace TestTaskKICB.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository customers;
    private readonly IAccountRepository accounts;

    public CustomerService(ICustomerRepository customers, IAccountRepository accounts)
    {
        this.customers = customers;
        this.accounts = accounts;
    }

    public IReadOnlyList<CustomerView> ListCustomers()
    {
        return customers.ListCustomers().Select(View).ToArray();
    }

    public OperationResult<CustomerView> GetCustomer(long id)
    {
        var customer = customers.FindCustomer(id);
        if (customer == null)
            return OperationResult<CustomerView>.Failed(BusinessError.Missing("customer"));
        return new OperationResult<CustomerView>(View(customer));
    }

    public OperationResult<CustomerView> CreateCustomer(CustomerDetails input)
    {
        var values = Trim(input);
        var error = Validate(values);
        if (error != null)
            return OperationResult<CustomerView>.Failed(error);

        var customer = new Customer(
            values.LastName!,
            values.FirstName!,
            values.Patronymic,
            values.Email!,
            values.DateOfBirth.GetValueOrDefault()
        );
        var saved = customers.Add(customer);
        return new OperationResult<CustomerView>(View(saved));
    }

    public OperationResult UpdateCustomer(long id, CustomerDetails input)
    {
        var values = Trim(input);
        var error = Validate(values);
        if (error != null)
            return new OperationResult(error);

        var customer = customers.FindCustomer(id);
        if (customer == null)
            return new OperationResult(BusinessError.Missing("customer"));

        customer.Update(
            values.LastName!,
            values.FirstName!,
            values.Patronymic,
            values.Email!,
            values.DateOfBirth.GetValueOrDefault()
        );
        customers.Update(customer);
        return new OperationResult();
    }

    public OperationResult DeleteCustomer(long id)
    {
        var customer = customers.FindCustomer(id);
        if (customer == null)
            return new OperationResult(BusinessError.Missing("customer"));
        if (accounts.HasAccounts(id))
            return new OperationResult(BusinessError.CustomerHasAccounts());

        customers.Remove(customer);
        return new OperationResult();
    }

    public OperationResult<IReadOnlyList<PhoneView>> ListPhones(long customerId)
    {
        if (!customers.Exists(customerId))
            return OperationResult<IReadOnlyList<PhoneView>>.Failed(BusinessError.Missing("customer"));

        var phones = customers.ListPhones(customerId).Select(View).ToArray();
        return new OperationResult<IReadOnlyList<PhoneView>>(phones);
    }

    public OperationResult<PhoneView> GetPhone(long id)
    {
        var phone = customers.FindPhone(id);
        if (phone == null)
            return OperationResult<PhoneView>.Failed(BusinessError.Missing("phone"));
        return new OperationResult<PhoneView>(View(phone));
    }

    public OperationResult<PhoneView> CreatePhone(long customerId, PhoneInput input)
    {
        var number = input.Number?.Trim();
        var error = ValidatePhone(number, input.Type, out var type);
        if (error != null)
            return OperationResult<PhoneView>.Failed(error);
        if (!customers.Exists(customerId))
            return OperationResult<PhoneView>.Failed(BusinessError.Missing("customer"));

        var phone = new Phone(customerId, number!, type.GetValueOrDefault());
        var saved = customers.Add(phone);
        return new OperationResult<PhoneView>(View(saved));
    }

    public OperationResult UpdatePhone(long id, PhoneInput input)
    {
        var number = input.Number?.Trim();
        var error = ValidatePhone(number, input.Type, out var type);
        if (error != null)
            return new OperationResult(error);

        var phone = customers.FindPhone(id);
        if (phone == null)
            return new OperationResult(BusinessError.Missing("phone"));

        phone.Update(number!, type.GetValueOrDefault());
        customers.Update(phone);
        return new OperationResult();
    }

    public OperationResult DeletePhone(long id)
    {
        var phone = customers.FindPhone(id);
        if (phone == null)
            return new OperationResult(BusinessError.Missing("phone"));

        customers.Remove(phone);
        return new OperationResult();
    }

    private static CustomerDetails Trim(CustomerDetails input)
    {
        var patronymic = input.Patronymic?.Trim();
        if (string.IsNullOrEmpty(patronymic))
            patronymic = null;

        return new CustomerDetails(
            input.LastName?.Trim(),
            input.FirstName?.Trim(),
            patronymic,
            input.Email?.Trim(),
            input.DateOfBirth
        );
    }

    private static BusinessError? Validate(CustomerDetails input)
    {
        var birthDate = input.DateOfBirth;
        if (!birthDate.HasValue || birthDate.Value == DateOnly.MinValue || birthDate.Value == DateOnly.MaxValue)
            return BusinessError.Invalid("dateOfBirth", "Provide a date of birth.");

        var error = InputValidation.CheckText(input.LastName, "lastName", 100)
            ?? InputValidation.CheckText(input.FirstName, "firstName", 100)
            ?? InputValidation.CheckText(input.Patronymic, "patronymic", 100, required: false)
            ?? InputValidation.CheckText(input.Email, "email", 254);
        if (error != null)
            return error;
        if (Customer.NormalizeEmail(input.Email!).Length > 254)
            return BusinessError.Invalid("email", "Use at most 254 characters.");
        return null;
    }

    private static BusinessError? ValidatePhone(
        string? number,
        string? inputType,
        out PhoneType? type
        )
    {
        if (!EnumValue.TryRead(inputType, out type))
            return BusinessError.Invalid("type", "Choose mobile, home, or work.");

        var error = InputValidation.CheckText(number, "number", 16);
        if (error != null)
            return error;
        if (!type.HasValue)
            return BusinessError.Invalid("type", "Choose mobile, home, or work.");
        return null;
    }

    private static CustomerView View(Customer customer) =>
        new CustomerView(
            customer.Id,
            customer.LastName,
            customer.FirstName,
            customer.Patronymic,
            customer.Email,
            customer.DateOfBirth
        );

    private static PhoneView View(Phone phone) =>
        new PhoneView(
            phone.Id,
            phone.CustomerId,
            phone.Number,
            phone.Type
        );
}
