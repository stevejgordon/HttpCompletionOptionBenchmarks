using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace SampleApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BooksController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Book> Get()
        {
            return Enumerable.Range(1, 100).Select(index => new Book
            {
                Author = "Steve Gordon",
                Date = "2020-03-20",
                ISBN = "123456789",
                Name = "This is a book title!"
            })
            .ToArray();
        }
    }
}
