using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using purrfect_blog_starter_1757957984.Models;
using System.Security.Cryptography;
using System.Text;

namespace purrfect_blog_starter_1757957984.Controllers
{
    public class AccountController : Controller
    {

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            using (var db = new ApplicationDbContext())
            {
                model.Username = model.Username?.Trim();
                model.Email = model.Email?.Trim();

                // Check if username and email already exists
                bool usernameExists = db.Users.Any(u => u.Username == model.Username);
                bool emailExists = db.Users.Any(u => u.Email == model.Email);

                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                }

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                }

                if (!ModelState.IsValid)
                {
                    return View("Register", model);
                }

                // Hash the password
                string hashedPassword = HashPassword(model.Password);

                // Save the new user
                var newUser = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = hashedPassword
                };
                db.Users.Add(newUser);
                db.SaveChanges();

                // After successful registration
				TempData["SuccessMessage"] = "Registration successful!";
				return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginUser(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Account/Login.cshtml", model);
            }
            using (var db = new ApplicationDbContext())
            {
                string hashedPassword = HashPassword(model.Password);
                var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.PasswordHash == hashedPassword);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View("~/Views/Account/Login.cshtml", model);
                }
                // Set session or authentication cookie
                System.Web.Security.FormsAuthentication.SetAuthCookie(user.Username, true);
                return RedirectToAction("Dashboard");
            }
        }
        public ActionResult Logout()
        {
            System.Web.Security.FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [Authorize]
        public ActionResult Dashboard(string sort = "recent")
        {
            var username = User?.Identity?.Name;
            using (var db = new ApplicationDbContext())
            {
                    var query =
                    from p in db.Posts.Where(p => p.AuthorUsername == username)
                    join v in db.Votes on p.Id equals v.PostId into pv
                    select new PostListItemViewModel
                    {
                        Post = p,
                        Score = pv.Select(x => (int?)x.Value).Sum() ?? 0,
                        CurrentUserVote = pv.Where(x => x.VoterUsername == username).Select(x => x.Value).FirstOrDefault()
                    };

                switch ((sort ?? "recent").ToLowerInvariant())
                {
                    case "oldest":
                        query = query.OrderBy(x => x.Post.CreatedAt);
                        break;
                    case "mostupvoted":
                        query = query.OrderByDescending(x => x.Score);
                        break;
                    case "mostdownvoted":
                        query = query.OrderBy(x => x.Score);
                        break;
                    default:
                        query = query.OrderByDescending(x => x.Post.CreatedAt);
                        sort = "recent";
                        break;
                }

                var items = query.ToList();
                ViewBag.Sort = sort;
                return View(items);
            }
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}