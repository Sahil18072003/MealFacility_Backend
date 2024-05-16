using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealFacility_Backend.DAL
{
    public class UserDTO
    {
        [Key]
        public int Id { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string? Token { get; set; }
    }
}
