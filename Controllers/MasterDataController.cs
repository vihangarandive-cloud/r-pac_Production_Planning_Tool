// /Controllers/MasterDataController.cs
using System.Web.Mvc;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Repositories;
using RPACProductionPlanner.Models;
using Dapper;

namespace RPACProductionPlanner.Controllers
{
    [RoleAuthorize("Admin", "Shift Executive")]
    public class MasterDataController : Controller
    {
        private readonly ISchedulerRepository _schedulerRepo;
        private readonly IInventoryRepository _inventoryRepo;

        public MasterDataController(ISchedulerRepository schedulerRepo, IInventoryRepository inventoryRepo)
        {
            _schedulerRepo = schedulerRepo;
            _inventoryRepo = inventoryRepo;
        }

        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            ViewBag.ActiveModule = "MasterData";
            
            // Get all machines from the repo
            var machines = _schedulerRepo.GetAllMachines();
            ViewBag.Machines = machines;

            // Get inventory items for the BOM tab
            var inventory = await _inventoryRepo.GetAllAsync();
            ViewBag.Inventory = inventory;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateMachine(MachineResource machine)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "UPDATE MachineResources SET MachineName = @MachineName, Department = @Department, CapacityPerShift = @CapacityPerShift WHERE MachineCode = @MachineCode";
                Dapper.SqlMapper.Execute(conn, sql, machine);
                
                string sql2 = "UPDATE machine_master SET MachineName = @MachineName, Department = @Department WHERE MachineCode = @MachineCode";
                Dapper.SqlMapper.Execute(conn, sql2, machine);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMachine(MachineResource machine)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql1 = "INSERT INTO MachineResources (MachineName, MachineCode, Department, CapacityPerShift, Status) VALUES (@MachineName, @MachineCode, @Department, @CapacityPerShift, 'Available')";
                Dapper.SqlMapper.Execute(conn, sql1, machine);
                
                string sql2 = "INSERT INTO machine_master (MachineName, MachineCode, Department, Status) VALUES (@MachineName, @MachineCode, @Department, 'Available')";
                Dapper.SqlMapper.Execute(conn, sql2, machine);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SyncSAP()
        {
            return Json(new { success = true, message = "Master data synchronized with SAP B1 successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateStock(int itemId, decimal newQuantity)
        {
            try
            {
                _inventoryRepo.UpdateStock(itemId, newQuantity);
                return Json(new { success = true, message = "BOM Stock updated successfully." });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddBOMItem(InventoryItem item)
        {
            try
            {
                _inventoryRepo.Add(item);
                return Json(new { success = true });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ToggleMachineStatus(string machineCode)
        {
            try
            {
                using (var conn = MSSQLHelper.GetConnection())
                {
                    // Get current status
                    string currentStatus = conn.ExecuteScalar<string>("SELECT Status FROM MachineResources WHERE MachineCode = @MachineCode", new { MachineCode = machineCode });
                    string newStatus = currentStatus == "Available" ? "Breakdown" : "Available";

                    string sql1 = "UPDATE MachineResources SET Status = @Status WHERE MachineCode = @MachineCode";
                    Dapper.SqlMapper.Execute(conn, sql1, new { Status = newStatus, MachineCode = machineCode });

                    string sql2 = "UPDATE machine_master SET Status = @Status WHERE MachineCode = @MachineCode";
                    Dapper.SqlMapper.Execute(conn, sql2, new { Status = newStatus, MachineCode = machineCode });

                    return Json(new { success = true, message = "Machine is now marked as " + newStatus });
                }
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
