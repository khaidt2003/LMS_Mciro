using System.Net.NetworkInformation;

using BooksCatalogService.Protos;

namespace BorrowingService.GrpcService;

using Grpc.Net.Client;

public class BookGrpcClient : IBookGrpcClient
{
    private readonly IConfiguration _configuration;

    public BookGrpcClient(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public CheckBookResult CheckAvailability(int bookId)
    {
        var client = GetClient();
        var request = new CheckAvailabilityRequest{BookId = bookId};
        try
        {
            var response = client.CheckAvailability(request);
            return new CheckBookResult(){IsAvailable =  response.IsAvailable, Title = response.Title};
        }
        catch(Exception ex)
        {
            Console.WriteLine($"--> Can Not Call gRPC ReturnBook: {ex.Message}");
            return new CheckBookResult { IsAvailable = false };
        }
    }

    public bool ReserveBook(int bookId)
    {
        var client = GetClient();
        var request = new BookActionRequest{BookId = bookId};
        try
        {
            var response = client.ReserveBook(request);
            return response.Success;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public bool ReturnBook(int bookId)
    {
        var client = GetClient();
        var request = new BookActionRequest{BookId = bookId};
        try
        {
            var response = client.ReturnBook(request);
            return response.Success;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    private BookStockService.BookStockServiceClient GetClient()
    {
        string address = _configuration["GrpcSettings:BookCatalogServiceUrl"];

        var channel = GrpcChannel.ForAddress(address);
        return new BookStockService.BookStockServiceClient(channel);
    }
}

    