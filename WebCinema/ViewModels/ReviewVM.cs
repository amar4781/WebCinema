namespace WebCinema.ViewModels
{
    public record ReviewVM(int orderId, string comment, int rate, List<IFormFile> imgs);
}
