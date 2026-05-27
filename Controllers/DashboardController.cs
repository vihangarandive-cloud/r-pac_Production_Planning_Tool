using System.Web.Mvc;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Repositories;
using RPACProductionPlanner.Services;

namespace RPACProductionPlanner.Controllers
{
    [RoleAuthorize("Admin", "Production Planner", "Shift Executive", "Manager")]
    public class DashboardController : Controller
    {
        private readonly IProductionOrderRepository _orderRepo;
        private readonly SchedulerService _schedulerService;
        private readonly NotificationService _notificationService;

        public DashboardController(
            IProductionOrderRepository orderRepo,
            SchedulerService schedulerService,
            NotificationService notificationService)
        {
            _orderRepo = orderRepo;
            _schedulerService = schedulerService;
            _notificationService = notificationService;
        }

        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            ViewBag.ActiveModule = "Dashboard";
            ViewBag.UserRole = SessionHelper.UserRole;

            // Fire alert processing in the background without blocking the page load
            _ = System.Threading.Tasks.Task.Run(() => _notificationService.ProcessAlerts());

            var kpis = await _orderRepo.GetDashboardKPIsAsync();
            var machines = _schedulerService.GetMachines();
            ViewBag.Machines = machines;

            return View(kpis);
        }

        [HttpGet]
        public JsonResult GetNotifications()
        {
            var repo = new NotificationRepository();
            var notifications = repo.GetUnread();
            return Json(notifications, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MarkNotificationRead(int id)
        {
            var repo = new NotificationRepository();
            repo.MarkAsRead(id);
            return Json(new { success = true });
        }
    }
}
