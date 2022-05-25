using bookShop.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookShop.Dtos.Requests
{
    public class AddCategoryRequest
    {
        [Required(ErrorMessage = "Kategori ismi girilmelidir")]
        [MinLength(2)]
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
