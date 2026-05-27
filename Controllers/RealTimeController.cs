using System;
using System.Web.Mvc;
using RPACProductionPlanner.Services;

namespace RPACProductionPlanner.Controllers
{
    public class RealTimeController : Controller
    {
        [HttpGet]
        public void StreamUpdates()
        {
            Response.ContentType = "text/event-stream";
            Response.BufferOutput = false;
            Response.CacheControl = "no-cache";
            
            // Deactivated synchronous SSE loop to prevent thread exhaustion
            Response.Write("data: {\"type\": \"info\", \"message\": \"Real-time updates are currently optimized for polling.\"}\n\n");
            Response.Flush();
            // End the request immediately instead of looping
            return;
        }
    }
}
