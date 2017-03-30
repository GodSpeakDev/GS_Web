using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using GodSpeak.Web.Models;
using GodSpeak.Web.Repositories;

namespace GodSpeak.Web.Controllers
{
    [Route("api/message/{action}")]
    public class MessageController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IApplicationUserProfileRepository _profileRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IInMemoryDataRepository _memoryDataRepo;


        public MessageController(ApplicationDbContext context, IApplicationUserProfileRepository profileRepo, IAuthRepository authRepo, IInMemoryDataRepository memoryDataRepo):base(authRepo)
        {
            _context = context;
            _profileRepo = profileRepo;
            _authRepo = authRepo;
            _memoryDataRepo = memoryDataRepo;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("queue")]
        public async Task<HttpResponseMessage> Queue(bool refresh = false)
        {
            if (!await RequestHasValidAuthToken(Request))
                return CreateMissingTokenResponse();

            var userId = await _authRepo.GetUserIdForToken(GetAuthToken(Request));
            var profile = await _profileRepo.GetByUserId(userId);
            var enabledMessageCategoriesIds = profile.MessageCategorySettings.Where(cat => cat.Enabled).Select(cat => cat.Category.MessageCategoryId).ToList();

            var messageSpecs =
                _context.Messages.Where(message => message.Categories.Any(cat => enabledMessageCategoriesIds.Contains(cat.MessageCategoryId))).ToList();

            var messages = new List<MessageApiObject> ();
            GetMessageAPIObjects(messages, messageSpecs);

            return CreateResponse(HttpStatusCode.OK, "Messages Retrieved",
                "Message Queue successfully retrieved/generated", messages);
        }

        private void GetMessageAPIObjects(List<MessageApiObject> messages, List<Message> messageSpecs)
        {
            foreach (var messageSpec in messageSpecs)
            {
                var startCode = messageSpec.VerseCode;
                var endVerse = -1;
                if (startCode.Contains("-"))
                {
                    endVerse = int.Parse(startCode.Split('-')[1]);
                    startCode = startCode.Split('-')[0];
                }

                try
                {
                    var verse = _memoryDataRepo.VerseCache[startCode];
                    VerseApiObject prevVerse = null;
                    if (verse.Verse != 1)
                        prevVerse =
                            VerseApiObject.FromModel(
                                _memoryDataRepo.VerseCache[$"{verse.Book} {verse.Chapter}:{verse.Verse - 1}".Trim()]);

                    if (endVerse == -1)
                        endVerse = verse.Verse;
                    VerseApiObject nextVerse = null;
                    var nextVerseKey = $"{verse.Book} {verse.Chapter}:{endVerse + 1}".Trim();
                    if (_memoryDataRepo.VerseCache.ContainsKey(nextVerseKey))
                        nextVerse =
                            VerseApiObject.FromModel(
                                _memoryDataRepo.VerseCache[nextVerseKey]);


                    var currentVerse = VerseApiObject.FromModel(verse);
                    if (endVerse != verse.Verse)
                    {
                        currentVerse.Title = messageSpec.VerseCode;
                        for (var i = verse.Verse + 1; i <= endVerse; i++)
                            currentVerse.Text += " " +
                                                 _memoryDataRepo.VerseCache[$"{verse.Book} {verse.Chapter}:{i}".Trim()]
                                                     .Text;
                    }
                    messages.Add(new MessageApiObject()
                    {
                        DateTimeToDisplay = DateTime.Now,
                        Id = messageSpec.MessageId,
                        PreviousVerse = prevVerse,
                        Verse = currentVerse,
                        NextVerse = nextVerse
                    });
                }
                catch (Exception ex)
                {
                    var chapter = startCode.Split(' ')[0];
                    var keys = _memoryDataRepo.VerseCache.Keys.Where(k => k.Contains(chapter));
                    Debug.WriteLine($"Couldn't find verse {startCode}");
                }
            }
        }
    }
}
