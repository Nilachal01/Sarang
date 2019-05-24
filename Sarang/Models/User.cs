using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Sarang.Models
{
    [MetadataType(typeof(UserMetadata))]
    public partial class User
    {
       

    }

    public class UserMetadata
    {


        [Display(Name = "First Name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use letters only please")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name required")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Use letters only please")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name")]
        public string LastName { get; set; }


        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email Id required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }


        [Display(Name = "Date of birth")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Date Of Birth required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public string DateOfBirth { get; set; }


        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 character is required")]
        public string Password { get; set; }


    }

}