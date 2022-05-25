using bookShop.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bookShop.Dtos.Requests
{
    public class AddWriterRequest
    {
        [Required(ErrorMessage = "Yazar ismi girilmelidir")]
        [MinLength(2)]
        public string FullName { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"(05(\d{9}))$", ErrorMessage = "Geçersiz telefon numarası")]
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }
        public int Age { get; set; }
        public DateTime? CreatedDate { get; set; }
        public ICollection<Book> Books { get; set; }
        public ICollection<Publisher> Publishers { get; set; }
    }
}
