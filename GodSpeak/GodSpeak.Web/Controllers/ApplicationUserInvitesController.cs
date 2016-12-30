using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Controllers
{
    public class ApplicationUserInvitesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ApplicationUserInvites
        public async Task<ActionResult> Index()
        {
            return View(await db.Invites.ToListAsync());
        }

        // GET: ApplicationUserInvites/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUserInvite applicationUserInvite = await db.Invites.FindAsync(id);
            if (applicationUserInvite == null)
            {
                return HttpNotFound();
            }
            return View(applicationUserInvite);
        }

        // GET: ApplicationUserInvites/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ApplicationUserInvites/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ApplicationUserInviteId,Code")] ApplicationUserInvite applicationUserInvite)
        {
            if (ModelState.IsValid)
            {
                applicationUserInvite.ApplicationUserInviteId = Guid.NewGuid();
                db.Invites.Add(applicationUserInvite);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(applicationUserInvite);
        }

        // GET: ApplicationUserInvites/Edit/5
        public async Task<ActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUserInvite applicationUserInvite = await db.Invites.FindAsync(id);
            if (applicationUserInvite == null)
            {
                return HttpNotFound();
            }
            return View(applicationUserInvite);
        }

        // POST: ApplicationUserInvites/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ApplicationUserInviteId,Code")] ApplicationUserInvite applicationUserInvite)
        {
            if (ModelState.IsValid)
            {
                db.Entry(applicationUserInvite).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(applicationUserInvite);
        }

        // GET: ApplicationUserInvites/Delete/5
        public async Task<ActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUserInvite applicationUserInvite = await db.Invites.FindAsync(id);
            if (applicationUserInvite == null)
            {
                return HttpNotFound();
            }
            return View(applicationUserInvite);
        }

        // POST: ApplicationUserInvites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            ApplicationUserInvite applicationUserInvite = await db.Invites.FindAsync(id);
            db.Invites.Remove(applicationUserInvite);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
