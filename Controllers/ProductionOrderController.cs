using System;
using System.Web.Mvc;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;
using RPACProductionPlanner.Repositories;
using RPACProductionPlanner.Services;

namespace RPACProductionPlanner.Controllers
{
    [RoleAuthorize("Admin", "Production Planner", "Shift Executive")]
    public class ProductionOrderController : Controller
    {
        private readonly IProductionOrderRepository _orderRepo;
        private readonly ProductionService _productionService;
        private readonly SchedulerService _schedulerService;

        public ProductionOrderController(
            IProductionOrderRepository orderRepo,
            ProductionService productionService,
            SchedulerService schedulerService)
        {
            _orderRepo = orderRepo;
            _productionService = productionService;
            _schedulerService = schedulerService;
        }

        public async System.Threading.Tasks.Task<ActionResult> Index(string status, DateTime? start, DateTime? end, string dept = null)
        {
            ViewBag.ActiveModule = "Production";

            // Default to Thermal dept to avoid loading all records at once
            if (string.IsNullOrEmpty(dept))
                dept = "Thermal";

            ViewBag.CurrentDept = dept;
            ViewBag.Machines = _schedulerService.GetMachines();
            ViewBag.Departments = _schedulerService.GetDepartments();

            var orders = await _orderRepo.GetAllAsync(status, start, end, dept);
            return View(orders);
        }

        public async System.Threading.Tasks.Task<ActionResult> GetOrdersPartial(string status, DateTime? start, DateTime? end, string dept = null)
        {
            var orders = await _orderRepo.GetAllAsync(status, start, end, dept);
            return PartialView("_OrdersTable", orders);
        }

        [HttpGet]
        [RoleAuthorize("Admin", "Production Planner", "Shift Executive")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Production Planner", "Shift Executive")]
        public ActionResult Create(ProductionOrder order)
        {
            order.CreatedBy = SessionHelper.UserId;
            var result = _productionService.CreateOrder(order);

            if (result.Success) return RedirectToAction("Index");

            ViewBag.Error = result.Message;
            return View(order);
        }

        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return HttpNotFound();
            var order = _orderRepo.GetByOrderCode(id);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateStatus(string id, string status)
        {
            var order = _orderRepo.GetByOrderCode(id);
            if (order != null)
            {
                order.Status = status;
                if (status == "In Progress") order.ActualStart = DateTime.Now;
                if (status == "Completed") order.ActualEnd = DateTime.Now;

                _orderRepo.Update(order);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdatePlanning(string orderCode, int machineId, string startTime, string endTime, string priority = "N/A")
        {
            if (string.IsNullOrEmpty(startTime))
                return Json(new { success = false, message = "Start time is required." });

            DateTime parsedStart;
            if (!DateTime.TryParse(startTime, out parsedStart))
                return Json(new { success = false, message = "Invalid start time." });

            DateTime? parsedEnd = null;
            if (!string.IsNullOrEmpty(endTime) && DateTime.TryParse(endTime, out DateTime endDt))
                parsedEnd = endDt;

            try
            {
                var success = _orderRepo.UpdatePlanning(orderCode, machineId, parsedStart, parsedEnd, priority);
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Unschedule(string orderCode)
        {
            try
            {
                var success = _orderRepo.Unschedule(orderCode);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateNotes(string orderCode, string notes)
        {
            if (string.IsNullOrEmpty(orderCode)) return Json(new { success = false });

            try
            {
                var success = _orderRepo.UpdateNotes(orderCode, notes);
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SyncSAP()
        {
            return Json(new { success = true });
        }
    }
}
