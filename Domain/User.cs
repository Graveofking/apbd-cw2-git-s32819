namespace EquipmentRentalService.Domain;

public enum UserType
{
    Student,
    Employee
}

public abstract class User
{
    private static int _idCounter = 1;

    protected User(string firstName, string lastName, UserType userType)
    {
        Id = _idCounter++;
        FirstName = firstName;
        LastName = lastName;
        UserType = userType;
    }

    public int Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public UserType UserType { get; }
}

public sealed class Student : User
{
    public Student(string firstName, string lastName) : base(firstName, lastName, UserType.Student)
    {
    }
}

public sealed class Employee : User
{
    public Employee(string firstName, string lastName) : base(firstName, lastName, UserType.Employee)
    {
    }
}
