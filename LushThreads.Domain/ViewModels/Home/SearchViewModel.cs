using LushThreads.Domain.Entites;

namespace LushThreads.Domain.ViewModels.Home
{
    public class SearchViewModel
    {
        public string SearchTerm { get; set; }
        public List<Product> Results { get; set; }
    }
}