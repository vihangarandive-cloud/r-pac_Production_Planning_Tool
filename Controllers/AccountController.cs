using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Repositories;

namespace RPACProductionPlanner.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;

        public AccountController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
                SessionHelper.Clear();
                HttpContext.User = new System.Security.Principal.GenericPrincipal(
                    new System.Security.Principal.GenericIdentity(""), null);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<ActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both username and password.";
                return View();
            }

            string cleanUsername = username.Trim();
            string cleanPassword = password.Trim();

            try
            {
                var user = await _userRepo.GetUserByUsernameAsync(cleanUsername);

                if (user != null)
                {
                    bool isValid = SecurityHelper.VerifyPassword(cleanPassword, user.PasswordHash);

                    if (!isValid)
                    {
                        // Fallback check for accounts that haven't migrated to the new hash format yet
                        string oldHash = GetSha256Hash(cleanPassword);
                        isValid = (user.PasswordHash == oldHash || user.PasswordHash == cleanPassword);
                    }

                    if (isValid)
                    {
                        FormsAuthentication.SetAuthCookie(cleanUsername, false);
                        SessionHelper.UserId = user.UserId;
                        SessionHelper.Username = user.Username;
                        SessionHelper.FullName = user.FullName;
                        SessionHelper.UserRole = user.RoleName;

                        return RedirectToAction("Index", "Dashboard");
                    }
                }

                ViewBag.Error = "Invalid credentials. Please try again.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Connection error: " + ex.Message;
            }

            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            SessionHelper.Clear();
            return RedirectToAction("Login");
        }

        private string GetSha256Hash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
