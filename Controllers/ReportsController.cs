// /Controllers/ReportsController.cs
using System;
using System.Web.Mvc;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Repositories;
using System.Linq;
using Newtonsoft.Json;

namespace RPACProductionPlanner.Controllers
{
    [RoleAuthorize("Admin", "Production Planner", "Shift Executive", "Manager")]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepo;
        private readonly IProductionOrderRepository _orderRepo;
        private readonly IInventoryRepository _inventoryRepo;

        public ReportsController(IReportRepository reportRepo, IProductionOrderRepository orderRepo, IInventoryRepository inventoryRepo)
        {
            _reportRepo = reportRepo;
            _orderRepo = orderRepo;
            _inventoryRepo = inventoryRepo;
        }

        public ActionResult Index()
        {
            ViewBag.ActiveModule = "Reports";
            
            var deptVolume = _reportRepo.GetDepartmentVolume().ToList();
            var monthlyTrend = _reportRepo.GetMonthlyTrend().ToList();

            ViewBag.DeptLabels = JsonConvert.SerializeObject(deptVolume.Select(x => Convert.ToString(x.Department)));
            ViewBag.DeptData = JsonConvert.SerializeObject(deptVolume.Select(x => Convert.ToDecimal(x.Volume)));

            ViewBag.TrendLabels = JsonConvert.SerializeObject(monthlyTrend.Select(x => Convert.ToString(x.Month)));
            ViewBag.TrendData = JsonConvert.SerializeObject(monthlyTrend.Select(x => Convert.ToDecimal(x.Volume)));
            
            // Initial KPI data (will also be refreshed live via AJAX)
            var kpiData = _reportRepo.GetAnalyticsKpis();
            ViewBag.TotalThroughput = kpiData.TotalThroughput;
            ViewBag.TotalOrders = kpiData.TotalOrders;
            ViewBag.ActiveOrders = kpiData.ActiveOrders;
            ViewBag.OverdueOrders = kpiData.OverdueOrders;
            ViewBag.OtdRate = kpiData.OtdRate;
            
            return View();
        }

        [HttpGet]
        public JsonResult GetAnalyticsKpis()
        {
            var kpi = _reportRepo.GetAnalyticsKpis();
            var deptVolume = _reportRepo.GetDepartmentVolume().ToList();
            var monthlyTrend = _reportRepo.GetMonthlyTrend().ToList();
            return Json(new {
                totalThroughput = kpi.TotalThroughput,
                totalOrders = kpi.TotalOrders,
                activeOrders = kpi.ActiveOrders,
                overdueOrders = kpi.OverdueOrders,
                otdRate = kpi.OtdRate,
                deptLabels = deptVolume.Select(x => Convert.ToString(x.Department)).ToList(),
                deptData = deptVolume.Select(x => Convert.ToDecimal(x.Volume)).ToList(),
                trendLabels = monthlyTrend.Select(x => Convert.ToString(x.Month)).ToList(),
                trendData = monthlyTrend.Select(x => Convert.ToDecimal(x.Volume)).ToList(),
                lastRefresh = DateTime.Now.ToString("HH:mm:ss")
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult ExportProductionSchedule()
        {
            var orders = _orderRepo.GetAll();
            
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Order Code,Product,Department,Machine Name,Quantity,Status,Planned Start,Planned End");

            foreach (var o in orders)
            {
                csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                    o.OrderCode, o.ProductName, o.Department, o.MachineName, o.Quantity, o.Status, o.PlannedStart, o.PlannedEnd));
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "ProductionSchedule_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
        }

        [HttpGet]
        public ActionResult ExportInventory()
        {
            var items = _inventoryRepo.GetAll();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Item Code,Item Name,Category,Quantity On Hand,Reorder Level,Unit");

            foreach (var i in items)
            {
                csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
                    i.ItemCode, i.ItemName, i.Category, i.QuantityOnHand, i.ReorderLevel, i.UnitOfMeasure));
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "InventoryReport_" + DateTime.Now.ToString("yyyyMMdd") + ".csv");
        }
    }
}
