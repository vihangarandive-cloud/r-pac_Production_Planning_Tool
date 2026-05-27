// /Controllers/SchedulerController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Repositories;
using RPACProductionPlanner.Services;

namespace RPACProductionPlanner.Controllers
{
    [RoleAuthorize("Admin", "Production Planner", "Shift Executive", "Manager")]
    public class SchedulerController : Controller
    {
        private readonly SchedulerService _schedulerService;
        private readonly IProductionOrderRepository _orderRepo;

        public SchedulerController(SchedulerService schedulerService, IProductionOrderRepository orderRepo)
        {
            _schedulerService = schedulerService;
            _orderRepo = orderRepo;
        }

        public async System.Threading.Tasks.Task<ActionResult> Index(string dept = null)
        {
            ViewBag.ActiveModule = "Scheduler";
            ViewBag.Machines = _schedulerService.GetMachines();
            ViewBag.KPIs = await _orderRepo.GetDashboardKPIsAsync(); 
            ViewBag.Departments = _schedulerService.GetDepartments();
            ViewBag.CurrentDept = dept;
            
            // Start from Today as requested
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(21);
            var allOrders = _schedulerService.GetScheduleData(startDate, endDate, dept);
            
            return View(allOrders);
        }

        [HttpPost]
        public JsonResult Solve()
        {
            // Real connection: Assign unassigned orders to the first available machine
            var allOrders = _schedulerService.GetScheduleData(DateTime.Today.AddDays(-60), DateTime.Today.AddDays(30));
            var machines = _schedulerService.GetMachines().ToList();
            
            int count = 0;
            foreach(var order in allOrders)
            {
                if (order.MachineId == 0 && machines.Any())
                {
                    // Assign to a machine sequentially for the demo/solve logic
                    var machine = machines[count % machines.Count];
                    _orderRepo.UpdatePlanning(order.OrderCode, machine.MachineId, DateTime.Now.AddHours(count * 4));
                    count++;
                }
            }
            
            return Json(new { success = true, message = $"Optimized {count} orders." });
        }

        [HttpGet]
        public JsonResult GetBoardData(DateTime? start, DateTime? end)
        {
            var startDate = start ?? DateTime.Today.AddDays(-60);
            var endDate = end ?? DateTime.Today.AddDays(30);
            var data = _schedulerService.GetScheduleData(startDate, endDate);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
