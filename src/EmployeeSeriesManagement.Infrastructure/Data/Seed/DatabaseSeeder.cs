using EmployeeSeriesManagement.Domain.Entities;
using EmployeeSeriesManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeSeriesManagement.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(EmployeeSeriesDbContext context, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await context.Employees.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Database already seeded.");
            return;
        }

        logger.LogInformation("Seeding demo data for Employee Series Management.");

        var personalType = new AddressType { Id = 1, Description = "Personal" };
        var workType = new AddressType { Id = 2, Description = "Work" };
        context.AddressTypes.AddRange(personalType, workType);

        var brusselsOffice = new Address
        {
            Id = 1,
            AddressTypeId = 2,
            Country = "Belgium",
            City = "Brussels",
            ZipCode = "1000",
            Street = "Avenue de la Porte de Namur",
            Number = "1",
            Building = "SNCB HQ"
        };

        var antwerpOffice = new Address
        {
            Id = 2,
            AddressTypeId = 2,
            Country = "Belgium",
            City = "Antwerp",
            ZipCode = "2000",
            Street = "Koningin Astridplein",
            Number = "27"
        };

        var personal1 = new Address
        {
            Id = 3,
            AddressTypeId = 1,
            Country = "Belgium",
            City = "Ixelles",
            ZipCode = "1050",
            Street = "Rue du Bailli",
            Number = "12"
        };

        var personal2 = new Address
        {
            Id = 4,
            AddressTypeId = 1,
            Country = "Belgium",
            City = "Schaerbeek",
            ZipCode = "1030",
            Street = "Avenue Rogier",
            Number = "88"
        };

        var personal3 = new Address
        {
            Id = 5,
            AddressTypeId = 1,
            Country = "Belgium",
            City = "Uccle",
            ZipCode = "1180",
            Street = "Chaussée de Waterloo",
            Number = "340"
        };

        context.Addresses.AddRange(brusselsOffice, antwerpOffice, personal1, personal2, personal3);

        var employee1 = new Employee
        {
            ExternalId = 1001,
            UserId = "USR001",
            Number = 1,
            FirstName = "Jean",
            LastName = "Dupont",
            Language = EmployeeLanguage.FR,
            BirthDate = new DateOnly(1985, 3, 15),
            BirthPlace = "Liège",
            EmailAddress = "jean.dupont@sncb.be",
            OrganizationalUnit = 10,
            PhoneNumber = "+32 2 123 45 67"
        };

        var employee2 = new Employee
        {
            ExternalId = 1002,
            UserId = "USR002",
            Number = 2,
            FirstName = "Marie",
            LastName = "Janssens",
            Language = EmployeeLanguage.NL,
            BirthDate = new DateOnly(1990, 7, 22),
            BirthPlace = "Ghent",
            EmailAddress = "marie.janssens@sncb.be",
            OrganizationalUnit = 10,
            PhoneNumber = "+32 2 234 56 78"
        };

        var employee3 = new Employee
        {
            ExternalId = 1003,
            UserId = "USR003",
            Number = 3,
            FirstName = "Pieter",
            LastName = "Peeters",
            Language = EmployeeLanguage.NL,
            BirthDate = new DateOnly(1988, 11, 5),
            BirthPlace = "Antwerp",
            EmailAddress = "pieter.peeters@sncb.be",
            OrganizationalUnit = 20,
            PhoneNumber = "+32 3 345 67 89"
        };

        context.Employees.AddRange(employee1, employee2, employee3);

        context.EmployeesAddresses.AddRange(
            new EmployeeAddress { EmployeesExternalId = 1001, AddressesId = 1 },
            new EmployeeAddress { EmployeesExternalId = 1001, AddressesId = 3 },
            new EmployeeAddress { EmployeesExternalId = 1002, AddressesId = 1 },
            new EmployeeAddress { EmployeesExternalId = 1002, AddressesId = 4 },
            new EmployeeAddress { EmployeesExternalId = 1003, AddressesId = 2 },
            new EmployeeAddress { EmployeesExternalId = 1003, AddressesId = 5 });

        var series1 = new Series
        {
            Code = 501,
            ExternalId = 9001,
            Name = "Brussels - Ghent Summer",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 8, 31)
        };

        var series2 = new Series
        {
            Code = 502,
            ExternalId = 9002,
            Name = "Brussels - Antwerp Peak",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

        context.Series.AddRange(series1, series2);

        context.EmployeesSeries.AddRange(
            new EmployeeSeries
            {
                EmployeesExternalId = 1001,
                SeriesCode = 501,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 8, 31)
            },
            new EmployeeSeries
            {
                EmployeesExternalId = 1001,
                SeriesCode = 502,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31)
            });

        context.EmployeesIdCards.Add(
            new EmployeeIdCard
            {
                Id = 1,
                EmployeesExternalId = 1001,
                Number = "SNCB-IDC-1001",
                Validity = new DateOnly(2028, 12, 31)
            });

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Demo data seeded successfully.");
    }
}
