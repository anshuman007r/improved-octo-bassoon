using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_for_deployment.Models
{
    public class Contacts
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ContactID { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Image { get; set; }
    }
}
