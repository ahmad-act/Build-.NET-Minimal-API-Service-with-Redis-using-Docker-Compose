using BookInformationService.DatabaseContext;
using BookInformationService.Util;
using Microsoft.EntityFrameworkCore;

namespace BookInformationService.BookInformation.Facade.Create;

public class CreateBookInformationDL : ICreateBookInformationDL
{
    private readonly ILogger<object> _logger;
    private readonly SystemDbContext _dbContext;

    public CreateBookInformationDL(ILogger<object> logger, SystemDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<Dictionary<string, object?>> CreateBookInformation(BookInformationModel bookInformation, CancellationToken ct)
    {
        try
        {
            _dbContext.BookInformation.Add(bookInformation);
            int result = await _dbContext.SaveChangesAsync();

            return new Dictionary<string, object?>
            {
                { "Message", string.Empty },
                { "ID", bookInformation.Id }
            };
        }
        catch (Exception ex)
        {
            ex.ErrorLog(_logger);

            return new Dictionary<string, object?>
            {
                { "Message", ex.Message},
                { "ID", 0 }
            };
        }
    }
}

