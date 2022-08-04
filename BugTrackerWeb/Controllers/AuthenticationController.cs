using BugTrackerWeb.Data;
using BugTrackerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace BugTrackerWeb.Controllers
{
    public class AuthenticationController : Controller
    {
        public IActionResult Index()
        {
            TempData.Remove("allMembers");
            return View();
        }
        [HttpPost]
        public IActionResult Index(User user)
        {
            if(user.Email!=null && user.Password != null)
            {

                Database db=   new Database();
                SqlDataReader? sdr = db.readData("select * from users where email='" + user.Email + "' and password='" + user.Password + "'");
                if (sdr!=null && sdr.Read())
                {
                    HttpContext.Session.SetString("name", sdr["name"].ToString());
                    HttpContext.Session.SetString("email", sdr["email"].ToString());
                    HttpContext.Session.SetString("role", sdr["role"].ToString());
                    return RedirectToAction("Index", "Home");
                }
                else ViewBag.invalid = true;
            }
            else ViewBag.invalid = true;
            return View(user);
        }

        public IActionResult Register()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Register(User  user)
        {

            if (ModelState.IsValid)
            {

                Database db = new Database();
                int rows=db.insertData("insert into Users values('"+user.Email+"','"+user.Password+"','"+user.Name+"','"+user.Role+"')");
                if (rows < 0)
                {
                    ViewBag.isSuccess = false;
                }
                else
                {
                    //return to list of members if user is begin created by admin
                    if (TempData["allMembers"] !=null)
                        return RedirectToAction("AllMembers", "Admin");
                    else
                    return RedirectToAction("Index");
                }

            }
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }


    }
}
