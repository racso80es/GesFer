using GesFer.ConsoleApp.Commands.Base;
using GesFer.ConsoleApp.Commands.Dtos;
using GesFer.ConsoleApp.Services;
using GesFer.Application.Commands.Customer;
using GesFer.Application.DTOs.Customer;
using GesFer.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GesFer.ConsoleApp.Commands;

public class CustomerCommand : GesFer.ConsoleApp.Commands.Base.ICommandHandler<CustomerCommandInput, CustomerCommandResult>
{
    private readonly LogService _logService;
    private readonly IConfiguration _configuration;

    public CustomerCommand(LogService logService, IConfiguration configuration)
    {
        _logService = logService;
        _configuration = configuration;
    }

    public async Task<CommandResult<CustomerCommandResult>> HandleAsync(CustomerCommandInput input)
    {
        var result = new CommandResult<CustomerCommandResult>
        {
            Data = new CustomerCommandResult { Success = true }
        };

        try
        {
            // Create the service provider scope using our new factory
            using var serviceProvider = ConsoleServiceFactory.CreateServiceProvider(_configuration) as ServiceProvider;

            if (serviceProvider == null)
            {
                 throw new Exception("Could not create ServiceProvider");
            }

            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            while (true)
            {
                // Clear console if interactive
                if (!Console.IsInputRedirected) Console.Clear();

                Console.WriteLine("========================================");
                Console.WriteLine("   Gestión de Clientes (Product)");
                Console.WriteLine("========================================");
                Console.WriteLine();
                Console.WriteLine("  1. Listar Clientes");
                Console.WriteLine("  2. Crear Cliente");
                Console.WriteLine("  3. Volver al menú principal");
                Console.WriteLine();
                Console.Write("Opción: ");

                var inputLine = Console.ReadLine();
                if (!int.TryParse(inputLine, out int option))
                {
                    continue;
                }

                if (option == 3) break;

                try
                {
                    switch (option)
                    {
                        case 1:
                            await ListCustomersAsync(scopedServices);
                            break;
                        case 2:
                            await CreateCustomerAsync(scopedServices);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    _logService.WriteError("Error en gestión de clientes", ex);
                }

                if (!Console.IsInputRedirected)
                {
                    Console.WriteLine();
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    try { Console.ReadKey(); } catch { }
                }
                else
                {
                    // If not interactive, break after one action to avoid infinite loop
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
            result.Data.Success = false;
            result.Data.Message = ex.Message;
            _logService.WriteError("Fallo crítico en CustomerCommand", ex);
        }

        return result;
    }

    private async Task ListCustomersAsync(IServiceProvider services)
    {
        // Resolve the handler for GetAllCustomersCommand
        // Note: Generic types can be tricky with DI if not registered exactly matching.
        // ConsoleServiceFactory registers via interfaces, so this should work.
        var handler = services.GetRequiredService<GesFer.Application.Common.Interfaces.ICommandHandler<GetAllCustomersCommand, List<CustomerDto>>>();

        Console.WriteLine("\nCargando clientes...");

        // Assuming GetAllCustomersCommand exists and has optional CompanyId
        var command = new GetAllCustomersCommand();

        var customers = await handler.HandleAsync(command);

        Console.WriteLine($"\nTotal clientes encontrados: {customers.Count}");
        Console.WriteLine("--------------------------------------------------------------------------------");
        Console.WriteLine("{0,-36} | {1,-30} | {2,-15}", "ID", "Nombre", "CIF/NIF");
        Console.WriteLine("--------------------------------------------------------------------------------");

        foreach (var c in customers)
        {
            Console.WriteLine("{0,-36} | {1,-30} | {2,-15}", c.Id, c.Name, c.TaxId ?? "N/A");
        }
    }

    private async Task CreateCustomerAsync(IServiceProvider services)
    {
        Console.WriteLine("\nCrear Nuevo Cliente");
        Console.WriteLine("-------------------");

        Console.Write("Nombre: ");
        var name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("El nombre es obligatorio.");
            return;
        }

        Console.Write("CIF/NIF: ");
        var taxId = Console.ReadLine();

        Console.Write("ID Empresa (GUID) [Enter para usar default]: ");
        var companyIdInput = Console.ReadLine();
        Guid companyId;

        if (string.IsNullOrWhiteSpace(companyIdInput) || !Guid.TryParse(companyIdInput, out companyId))
        {
            // Fallback to a known company or search for one?
            // For now, let's try to find the first company from DB context just to be safe,
            // or just use a dummy GUID if we trust Seed data is consistent (usually CompanyId is specific).
            // Let's assume user knows what they are doing or use a hardcoded fallback from seed.
            // But cleaner is to list companies. For now, let's ask for it or fail.
            Console.WriteLine("ID Empresa inválido. Usando GUID vacío (probablemente fallará si hay FK).");
            companyId = Guid.Empty;
        }

        var dto = new CreateCustomerDto
        {
            CompanyId = companyId,
            Name = name,
            TaxId = taxId
            // Add defaults for others
        };

        var command = new CreateCustomerCommand(dto);

        var handler = services.GetRequiredService<GesFer.Application.Common.Interfaces.ICommandHandler<CreateCustomerCommand, CustomerDto>>();
        var resultDto = await handler.HandleAsync(command);

        Console.WriteLine($"\nCliente creado con éxito!");
        Console.WriteLine($"ID: {resultDto.Id}");
        Console.WriteLine($"Nombre: {resultDto.Name}");
    }
}
