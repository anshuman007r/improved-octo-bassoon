using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp_for_deployment.Models;

namespace WebApp_for_deployment.Data
{
    public class DbInitializer
    {
        public static void Initialize(ContactContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Contacts.Any())
            {
                return;   // DB has been seeded
            }
            var contacts = new Contacts[]
            {
            new Contacts{Name="Ankit",DOB=DateTime.Parse("1996-08-02"),Email="ankit@gmail.com",Mobile="9322232232"},
            new Contacts{Name="Kabir",DOB=DateTime.Parse("1998-09-12"),Email="kabir@gmail.com",Mobile="8324556672"}
            };
            foreach (Contacts s in contacts)
            {
                context.Contacts.Add(s);
            }
            context.SaveChanges();

        }
    }
}
