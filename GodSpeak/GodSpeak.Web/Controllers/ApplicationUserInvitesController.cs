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
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{
    public class ApplicationUserInvitesController : Controller
    {
        private readonly IApplicationUserInviteRepository _inviteRepository;
        

        public ApplicationUserInvitesController(IApplicationUserInviteRepository inviteRepository)
        {
            _inviteRepository = inviteRepository;
        }

        // GET: ApplicationUserInvites
        public async Task<ActionResult> Index()
        {
            return View(await _inviteRepository.All());
        }

        // GET: ApplicationUserInvites/Details/5
        public async Task<ActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUserInvite applicationUserInvite = await _inviteRepository.GetById(id);
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
                await _inviteRepository.Insert(applicationUserInvite);
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
            ApplicationUserInvite applicationUserInvite = await _inviteRepository.GetById(id);
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
                
                await _inviteRepository.Update(applicationUserInvite);
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
            ApplicationUserInvite applicationUserInvite = await _inviteRepository.GetById(id);
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
            ApplicationUserInvite applicationUserInvite = await _inviteRepository.GetById(id);
            await _inviteRepository.Delete(applicationUserInvite);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inviteRepository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
