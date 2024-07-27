
namespace BookInformationService.BookInformation.Facade.Delete
{
    public interface IDeleteBookInformationDL
    {
        Task<Dictionary<string, object?>> DeleteBookInformation(BookInformationModel bookInformation, CancellationToken ct);
        Task<Dictionary<string, object?>> GetBookInformation(int id, CancellationToken ct);
    }
}