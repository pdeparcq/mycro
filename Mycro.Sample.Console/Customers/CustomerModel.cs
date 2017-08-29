using System.ComponentModel.DataAnnotations;

namespace Mycro.Sample.Console.Customers
{
    public class CustomerModel
    {
        [Required(AllowEmptyStrings = false)]
        public string FirstName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string LastName { get; set; }
    }
}
