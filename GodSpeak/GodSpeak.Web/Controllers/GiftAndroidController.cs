﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;
using Microsoft.AspNet.Identity;

namespace GodSpeak.Web.Controllers
{
    [Authorize]
    public class GiftAndroidController : Controller
    {
        private readonly IApplicationUserProfileRepository _profileRepository;
        private readonly IPayPalTransactionRepository _payPalTransactionRepository;
        private readonly IInviteRepository _inviteRepository;

        public GiftAndroidController(IApplicationUserProfileRepository profileRepository, IPayPalTransactionRepository payPalTransactionRepository, IInviteRepository inviteRepository)
        {
            _profileRepository = profileRepository;
            _payPalTransactionRepository = payPalTransactionRepository;
            _inviteRepository = inviteRepository;
        }

        // GET: GiftAndroid
        public async Task<ActionResult> Index(string message = null)
        {
            var userId = User.Identity.GetUserId();
            var profile = await _profileRepository.GetByUserId(userId); ViewBag.InviteCode = profile.Code;
            ViewBag.GiftsUnclaimedCount = profile.InviteBalance;
//            ViewBag.GiftsUnclaimedCount = 0;
            ViewBag.InviteCode = profile.Code;
            ViewBag.EmailBody = $@"Hi there,%0D%0AI want to give you this gift of GodSpeak for Android. To claim this gift please do the following:%0D%0A%0D%0A1) Download GodSpeak from the Google Playstore:%0D%0A%0D%0Ahttps://play.google.com/store/apps/details?id=com.givegodspeak.android%0D%0A%0D%0A2) After installing and running the app, when prompted, enter this Gift Code:%0D%0A%0D%0A{profile.Code}%0D%0A%0D%0A- {profile.FirstName}";
            ViewBag.TextBody = $@"Hi there,%0D%0AI want to give you this gift of GodSpeak for Android. To claim this gift please do the following:%0D%0A%0D%0A1) Download GodSpeak from the Google Playstore:%0D%0A%0D%0Ahttps://play.google.com/store/apps/details%3Fid=com.givegodspeak.android%0D%0A%0D%0A2) After installing and running the app, when prompted, enter this Gift Code:%0D%0A%0D%0A{profile.Code}%0D%0A%0D%0A- {profile.FirstName}";
            ViewBag.NotificationMessage = message;
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Purchase(PayPalTransaction transaction)
        {
            var message = "";
            if (!ModelState.IsValid)
                message = "Submission is missing required info.";
            var transactionAlreadyProcess = await _payPalTransactionRepository.TranscationExists(transaction.PayPalPaymentId);
            if (transactionAlreadyProcess)
                message = "Sorry, this transaction was already completed";

            if (ModelState.IsValid && !transactionAlreadyProcess)
            {
                transaction.DateTimePurchased = DateTime.Now;

                await _payPalTransactionRepository.SaveTransactions(transaction);
                var profile = await _profileRepository.GetByCode(transaction.InviteCode);
                
                profile.InviteBalance += transaction.InviteCount;

                await _profileRepository.Update(profile);
                
            }

            message = $"You have successfully purchased {transaction.InviteCount} gifts";

            return RedirectToAction("Index", new { message });
        }



        public async Task<ActionResult> Purchase()
        {
            var userId = User.Identity.GetUserId();
            var profile = await _profileRepository.GetByUserId(userId); ViewBag.InviteCode = profile.Code;
            ViewBag.InviteCode = profile.Code;
            ViewBag.UnclaimedCount = profile.InviteBalance;
            ViewBag.Bundles = (await _inviteRepository.Bundles()).OrderBy(b => b.NumberOfInvites);
            return View();
        }
    }
}