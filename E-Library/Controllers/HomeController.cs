using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Library.Models;
using System.IO;
namespace E_Library.Controllers
{
    public class HomeController : Controller
    {
        private Entities1 db = new Entities1();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }


        public ActionResult LogIn()
        { return View(); }

        [HttpPost]
        public ActionResult LogIn(Users user)
        {
            using (Entities1 ucon = new Entities1())
            {
                var userdetails = ucon.Users.Where(x => x.User_Name == user.User_Name && x.Password == user.Password).FirstOrDefault();
                if (userdetails == null)
                {
                    //     user.LoginErrorMessage = "Error usename or password";
                    return View("Login");

                }
                else
                {
                    Session["User_Id"] = userdetails.User_Id;
                    Session["User_name"] = userdetails.User_Name;
                    Session["IsAdmin"] = userdetails.isAdmin;
                    if (userdetails.isAdmin == true)
                        return View("index");
                    else
                        return View("index");
                }


            } 
        }
        public ActionResult Error() { return View(); }

        public ActionResult signup() { return View(); }
        [HttpPost]
        public ActionResult signup([Bind(Include = "User_Id,User_Name,Password,Birthdate,isAdmin")] Users users) {
            if (db.Users.Any(x=>x.User_Name==users.User_Name)) {
                ViewBag.DuplicateMessage = "اسم المستخدم موجود مسبقاً";
                return View("signup",users);
           }
            if (ModelState.IsValid)
            {
                db.Users.Add(users);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View("signup");
        }
    
    public ActionResult Managment()
        {
            return View();
        }

        public ActionResult Contact()
        {

            return View();
        }

     
    }
}