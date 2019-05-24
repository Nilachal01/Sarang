using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sarang.Models
{
    public class Validation
    {
        [Required(AllowEmptyStrings =false,ErrorMessage ="EmailID required")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Enter EmailId")]
        //Using Remote validation attribute   
        [Remote("IsAlreadySigned", "User", HttpMethod = "POST", ErrorMessage = "EmailId already exists in database.")]
        public string EmailID { get; set; }
    }
}