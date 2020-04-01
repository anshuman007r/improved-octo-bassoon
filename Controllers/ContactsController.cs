using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp_for_deployment.Data;
using WebApp_for_deployment.Models;
using WebApp_for_deployment.Project;

namespace WebApp_for_deployment.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly ContactContext _context;
        private readonly IWebHostEnvironment hostEnvironment;
        public DateTime now = DateTime.Now;

        public ContactsController(ContactContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            this.hostEnvironment = hostEnvironment;
        }

        // GET: Contacts
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber
            )
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var contacts = from s in _context.Contacts
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                contacts = contacts.Where(s => s.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    contacts = contacts.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    contacts = contacts.OrderBy(s => s.DOB);
                    break;
                case "date_desc":
                    contacts = contacts.OrderByDescending(s => s.DOB);
                    break;
                default:
                    contacts = contacts.OrderBy(s => s.Name);
                    break;
            }
            int pageSize = 3;
            return View(await PaginatedList<Contacts>.CreateAsync(contacts.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactID == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        //[Authorize(Roles ="Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContactID,Name,DOB,Email,Mobile,Photo")]PhotoUploadModel pic)
        {
            if (ModelState.IsValid)
            {
                string uid = null;
                DateTime now = DateTime.Now;

                string date = now.ToString("dd-MM-yy");
                string[] paths = { ProjectConstant.dir, date };
                string upload = Path.Combine(paths);
                if (!Directory.Exists(upload))
                {
                    Directory.CreateDirectory(upload);
                }
                string uniqueImage = null;
                if (pic.Photo != null)
                {
                    uid = Guid.NewGuid().ToString() + "_" + pic.Photo.FileName;
                    string filepath = Path.Combine(upload, uid);
                    pic.Photo.CopyTo(new FileStream(filepath, FileMode.Create));
                    uniqueImage = date + "/" + uid;
                }
                Contacts contact = new Contacts
                {
                    ContactID = pic.ContactID,
                    Name = pic.Name,
                    DOB = pic.DOB,
                    Email = pic.Email,
                    Mobile = pic.Mobile,
                    Image = uniqueImage
                };
                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        //[Authorize(Roles = "Admin,Acceptor")]
        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            return View(contact);
        }
        //[Authorize(Roles = "Admin,Acceptor")]
        // POST: Contacts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContactID,Name,DOB,Email,Mobile,Photo")] PhotoUploadModel pic)
        {
            Contacts contact = _context.Contacts.Find(id);
            string contact_old_image = contact.Image;
            string uniqueId = null;
            string uniqueImage = null;
            if (id != contact.ContactID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (pic.Photo == null)
                {
                    //Debug.WriteLine("\nIfcaseforPhoto:",pic.Photo);
                    contact.Name = pic.Name;
                    contact.DOB = pic.DOB;
                    contact.Email = pic.Email;
                    contact.Mobile = pic.Mobile;
                    contact.Image = contact_old_image;
                    _context.SaveChanges();
                }
                else
                {
                    //Debug.WriteLine("\nElsecaseforPhoto:", pic.Photo);
                    string uid = contact.Image;
                    string date = now.ToString("dd-MM-yy");
                    string[] paths = { ProjectConstant.dir, uid };
                    string delete = Path.Combine(paths);
                    System.IO.File.Delete(delete);
                    uniqueId = Guid.NewGuid().ToString() + "_" + pic.Photo.FileName;
                    string[] new_paths = { ProjectConstant.dir, date };
                    string create = Path.Combine(new_paths);
                    string filepath = Path.Combine(create, uniqueId);
                    pic.Photo.CopyTo(new FileStream(filepath, FileMode.Create));
                    uniqueImage = date + "/" + uniqueId;
                    contact.Name = pic.Name;
                    contact.DOB = pic.DOB;
                    contact.Email = pic.Email;
                    contact.Mobile = pic.Mobile;
                    contact.Image = uniqueImage;
                    _context.SaveChanges();
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.ContactID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(contact);
        }
        //[Authorize(Roles = "Admin")]
        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.ContactID == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }
        //[Authorize(Roles = "Admin")]
        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            string uid = contact.Image;
            string[] paths = { ProjectConstant.dir, uid };
            string delete = Path.Combine(paths);
            System.IO.File.Delete(delete);
            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.ContactID == id);
        }
    }
}
