using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using E_Library.Models;

namespace E_Library.Controllers
{
    public class BooksController : Controller
    {
        private Entities1 db = new Entities1();

        // GET: Books
        public ActionResult view()
        {
            var book = db.Book.Include(b => b.Department);
            return View(book.ToList());
        }
        public ActionResult AdminView()
        { var book = db.Book.Include(b => b.Department);
            return View(book.ToList());
        }

        // GET: Books/Details/5
        public ActionResult Details(int? id)
        {
            if (Session["User_Id"] == null) {
                ViewBag.errMessage = "لا يمكنك الوصول لهذه الصفحة ، قم بتسجيل الدخول أولاً";
                return RedirectToAction("Error","Home"); }
            Entities1 db = new Entities1();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Book.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            int ui = Int32.Parse(Session["User_Id"].ToString());
      
            bool l = false;
            Likes lik = (from result in db.Likes where result.Book_id == id && result.User_id == ui select result).FirstOrDefault();
            if (lik != null) l = true;
            ViewBag.liked = l;
            return View(book);
        }
        int[] thisbook = new int[2];
        double[] notrelated = new double[2];
        int ss = 0;
        void kmeans(int[,] arr,int len)
        {
            ss++;
            double[] lastnotrelated = notrelated;
            int[,] oth = new int[len, 2];
            int n = 0;
            for(int i = 0; i < len; i++)
            {
                if(
                    Math.Sqrt((notrelated[0]-arr[i,0])*(notrelated[0]-arr[i,0]) 
                    + (notrelated[1]-arr[i,1])*(notrelated[1]-arr[i,1]))
                    <
                    Math.Sqrt((thisbook[0]-arr[i,0])*(thisbook[0]-arr[i,0]) 
                    + (thisbook[1]-arr[i,1])*(thisbook[1]-arr[i,1]))
                  )
                {
                    oth[n, 0] = arr[i, 0];
                    oth[n, 1] = arr[i, 1];
                    n++;
                }
            }
            int all0 = 0, all1 = 0;
            for(int i = 0; i < n; i++)
            {
                all0 += oth[i, 0];
                all1 += oth[i, 1];
            }
            notrelated[0] = (double)all0 / n;
            notrelated[1] = (double)all1 / n;
            bool ok = true;
            for (int i = 0; i < 2; i++)
                if (lastnotrelated[i] != notrelated[i]) ok = false;
            if (ok) return;
            lastnotrelated = notrelated;
            kmeans(arr,len);
        }
       // [HttpPost]
        public ActionResult Liked(int id)
        {
            Entities1 db = new Entities1();
            #region s
          
            Book book = db.Book.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            #endregion
            Book tb = db.Book.FirstOrDefault(b => b.Book_Id == id);
            thisbook[0] = tb.Book_Pages;
            thisbook[1] = tb.Likes.Count;
            var liks = db.Likes.ToList();

            int[,] arr = new int[9999, 2];
            int i = 0;
            notrelated[0] = notrelated[1] = 0;
            foreach(Likes l in liks)
            {
                arr[i, 0] = l.Book.Book_Pages;
                arr[i, 1] = l.Book.Likes.Count;
                if (arr[i, 0] + arr[i, 1] > notrelated[0] + notrelated[1])
                {
                    notrelated[0] = arr[i,0]; notrelated[1] = arr[i, 1];}
                i++;
            }
            kmeans(arr, i);
            List<Book> lb = new List<Book>();
            foreach(Likes l in liks)
            {
                Book b = l.Book;
                if(
                    Math.Sqrt((notrelated[0] - b.Book_Pages) * (notrelated[0] - b.Book_Pages)
                    + (notrelated[1] - b.Likes.Count) * (notrelated[1] - b.Likes.Count))
                    <
                    Math.Sqrt((thisbook[0] - b.Book_Pages) * (thisbook[0] - b.Book_Pages)
                    + (thisbook[1] - b.Likes.Count) * (thisbook[1] - b.Likes.Count))
                  )
                {
                    bool ex = false;
                    foreach (Book bk in lb)
                        if (b.Book_Id == bk.Book_Id)
                            ex = true;
                    if(!ex)lb.Add(b);
                }
            }
            ViewBag.suggests = lb;
            return View(book);
        }
        // GET: Books/Create
        public ActionResult Create()
        {
            ViewBag.Dep_Id = new SelectList(db.Department, "Dep_id", "Dep_name");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
      //  [ValidateAntiForgeryToken]
        public ActionResult Create(HttpPostedFileBase UploadImage ,Book book) {
            string n = "";
            if (UploadImage.ContentLength > 0)
            {
                string filename = Path.GetFileName(UploadImage.FileName);
                string folder = Path.Combine(Server.MapPath("~/Images/"), filename);
                UploadImage.SaveAs(folder);
                n = filename;
            }
            else { return View(); }
            if (ModelState.IsValid)
            {
                book.Book_Pic = n;
                db.Book.Add(book);
                db.SaveChanges();
                return RedirectToAction("AdminView");
            }

            ViewBag.Dep_Id = new SelectList(db.Department, "Dep_id", "Dep_name", book.Dep_Id);
            return View(book);
        }

        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Book.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.Dep_Id = new SelectList(db.Department, "Dep_id", "Dep_name", book.Dep_Id);
            return View(book);
        }

        public ActionResult Like(int? id)
        {
            Entities1 db = new Entities1();
            Likes lik = new Likes();
           
            lik.Book_id = Int32.Parse(id.ToString());
            lik.User_id = Int32.Parse(Session["User_Id"].ToString());
            db.Likes.Add(lik);
            db.SaveChanges();
            return RedirectToAction("Liked/"+id);

        }
        // POST: Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Book_Id,Book_Name,Book_Author,Publish_Date,Dep_Id,Book_link,Book_Pic")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("AdminView");
            }
            ViewBag.Dep_Id = new SelectList(db.Department, "Dep_id", "Dep_name", book.Dep_Id);
            return View(book);
        }
        public ActionResult search(string txtname) {
            return View(db.Book.Where(x => x.Book_Name == txtname).ToList());
        }
        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Book.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
           
            Book book = db.Book.Find(id);
            List<Likes> lik = db.Likes.Where(like => like.Book_id == id).ToList();
            db.Book.Remove(book);
            foreach (Likes likes in lik)
            {
                db.Likes.Remove(likes);
            }
            db.SaveChanges();
            return RedirectToAction("AdminView");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }   
}
