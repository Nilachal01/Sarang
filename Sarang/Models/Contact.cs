using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sarang.Models
{
    public class Contact
    {
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use letters only please")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Name required")]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "EmailID required")]
        public string EmailID { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Subject required")]
        public string Subject { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Message required")]
        public string Message { get; set; }
    }
}