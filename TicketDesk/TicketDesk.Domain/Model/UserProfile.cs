using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketDesk.Domain.Model
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        public long UserId { get; set; }
        public string ContactNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public int UserStatus { get; set; }
        public string CreationDate { get; set; }
        public string Email { get; set; }
    }
}
