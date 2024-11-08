using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForestChurches.Models
{
    public class WhitelistedUsers
    {
        [Column("ID")]
        public Guid ID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "RegistrationDate")]
        public string RegistrationDate { get; set; }
    }
}
