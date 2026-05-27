using System.Web.Mvc;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Repositories;

namespace RPACProductionPlanner.Controllers
{
    [RoleAuthorize("Admin", "Shift Executive")]
    public class AdminController : Controller
    {
        private readonly IUserRepository _userRepo;

        public AdminController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Users");
        }

        public ActionResult Users()
        {
            ViewBag.ActiveModule = "Admin";
            var users = _userRepo.GetAllUsers();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(string FullName, string Username, string Role, string Password)
        {
            try
            {
                int roleId = Role == "Planner" ? 2 : Role == "ShiftExecutive" ? 3 : Role == "Manager" ? 4 : 1;
                var user = new RPACProductionPlanner.Models.UserAccount
                {
                    FullName = FullName,
                    Username = Username,
                    PasswordHash = SecurityHelper.HashPassword(Password),
                    RoleId = roleId
                };
                _userRepo.AddUser(user);
                return RedirectToAction("Users");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to create user: " + ex.Message;
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ToggleStatus(string username)
        {
            _userRepo.ToggleStatus(username);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(string FullName, string Username, string Role)
        {
            int roleId = Role == "Planner" ? 2 : Role == "ShiftExecutive" ? 3 : Role == "Manager" ? 4 : 1;
            var user = new RPACProductionPlanner.Models.UserAccount
            {
                FullName = FullName,
                Username = Username,
                RoleId = roleId
            };
            _userRepo.UpdateUser(user);
            return RedirectToAction("Users");
        }

        public ActionResult Roles()
        {
            ViewBag.ActiveModule = "Admin";
            return View();
        }
    }
}
