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
        private readonly IInMemoryDataRepository _memoryDataRepo;


        public MessageController(ApplicationDbContext context, IAuthRepository authRepo, IInMemoryDataRepository memoryDataRepo):base(authRepo)
        {
            _context = context;
            _memoryDataRepo = memoryDataRepo;
        }

        [HttpGet]
        [ResponseType(typeof(ApiResponse))]
        [ActionName("queue")]
        public async Task<HttpResponseMessage> Queue(bool refresh = false)
        {
            var messages = new List<MessageApiObject> ();
            foreach (var messageSpec in _context.Messages)
            {
                var code = messageSpec.VerseCode;
                if(code.Contains("-"))
                    continue;
                try
                {
                    var verse = _memoryDataRepo.VerseCache[code];
                    messages.Add(new MessageApiObject()
                    {
                        DateTimeToDisplay = DateTime.Now,
                        Id = messageSpec.MessageId,
                        Verse = new VerseApiObject()
                        {
                            Title = code,
                            Text = verse.Text
                        }
                    });
                }
                catch (Exception ex)
                {
                    var chapter = code.Split(' ')[0];
                    var keys = _memoryDataRepo.VerseCache.Keys.Where(k => k.Contains(chapter));
                    Debug.WriteLine($"Couldn't find verse {code}");
                }
                //BibleVerse prevVerse = null;
                //if (verse.Verse != 1)
                    //prevVerse = _memoryDataRepo.VerseCache[$"{verse.Book} {verse.Chapter}:{verse.Verse}"];
                
            }

            return CreateResponse(HttpStatusCode.OK, "Messages Retrieved",
                "Message Queue successfully retrieved/generated", messages);
        }
    }
}
