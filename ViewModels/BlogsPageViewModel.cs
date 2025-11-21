using PagedList;
using old_phone.Models;

namespace old_phone.ViewModels
{
    public class BlogsPageViewModel
    {
        public IPagedList<Blog> Blogs { get; set; }
        public string Search { get; set; }
    }
}