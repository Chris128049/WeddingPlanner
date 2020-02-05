using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;


namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        public IActionResult Index()
        {
            return View("index");
        }

        [HttpPost("register")]
        public IActionResult Register(IndexViewModels modelData)
        {

            if (ModelState.IsValid)
            {
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                modelData.newUser.Password = Hasher.HashPassword(modelData.newUser, modelData.newUser.Password);
                // If a User exists with provided email
                if (dbContext.Users.Any(u => u.Email == modelData.newUser.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ViewBag.Email = "Email already in use";


                    return View("Index");
                }
                dbContext.Add(modelData.newUser);
                // OR dbContext.Users.Add(newUser);
                dbContext.SaveChanges();
            }

            // Check initial ModelState
            return View("Index");
        }
        [HttpPost("login")]
        public IActionResult Login(IndexViewModels userSubmission)
        {
           
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.login.LogEmail);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ViewBag.LogEmail = "Invalid Email/Password";
                    return View("Index");
                }

                // Initialize hasher object
                var hasher = new PasswordHasher<User>();

                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission.newUser, userInDb.Password, userSubmission.login.LogPassword);

                // result can be compared to 0 for failure
                if (result == 0)
                {
                    // handle failure (this should be similar to how "existing email" is handled)
                    ViewBag.LogEmail = "Invalid Email/Password";
                    return View("Index");
                }

                HttpContext.Session.SetInt32("User", userInDb.UserId);
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(HttpContext.Session.GetString("User"));
                return RedirectToAction("Success");

            }
            return View("Index");

        }
        [HttpGet("/success")]
        public IActionResult Success()
        {

            Console.ForegroundColor = ConsoleColor.Red;
            System.Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            
            System.Console.WriteLine(HttpContext.Session.GetInt32("User"));
            if (HttpContext.Session.GetInt32("User") == null)
            {
                ViewBag.ses = "Must be logged in";
                return View("Index");
            }
            else
            {
                
                var i = HttpContext.Session.GetInt32("User");
                var user = dbContext.Users.FirstOrDefault(u => u.UserId == i);
                Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine(HttpContext.Session.GetInt32("User"));
                ViewBag.see = HttpContext.Session.GetInt32("User");

                
                List<Wedding> AllWeds = dbContext.Wedding
               .Include(g => g.Guest)
               .ThenInclude(u => u.User)
               .ToList();
            
                return View("Success",AllWeds);
            }


        }

        [HttpGet("addnewweddingview")]
        public IActionResult WeddingView()
        {
            if (HttpContext.Session.GetInt32("User") == null)
            {
                ViewBag.ses = "Must be logged in";
                return View("Index");
            }
           
           
            ViewBag.ID = HttpContext.Session.GetInt32("User");
            return View("addnewweddingview");
            
        }

        public IActionResult AddWedding(Wedding newWedding){
            if (HttpContext.Session.GetInt32("User") == null)
            {
                ViewBag.ses = "Must be logged in";
                return View("Index");
            }
            if(newWedding.Date < DateTime.Now){
                ViewBag.DATE = "Must have a wedding in the future";
                ViewBag.ID = HttpContext.Session.GetInt32("User");
                return View("addnewweddingview");

            }
            
            dbContext.Add(newWedding);
            dbContext.SaveChanges();
            List<Wedding> AllWeds = dbContext.Wedding
              .Include(g => g.Guest)
              .ThenInclude(u => u.User)
              .ToList();
            DashViewModel newModel = new DashViewModel();
            newModel.AllWeddings = AllWeds;
            return RedirectToAction ("success");
        }


        [HttpGet("viewthiswedding/{id}")]
        public IActionResult ViewThisWedding (int Id){
            if (HttpContext.Session.GetInt32("User") == null)
            {
                ViewBag.ses = "Must be logged in";
                return View("Index");
            }
            List<Wedding> ThisWedding = dbContext.Wedding
              .Include(g => g.Guest)
              .ThenInclude(u => u.User)
              .Where(w => w.WeddingId == Id).ToList();
            

            Wedding viewWedding = dbContext.Wedding
                       .Include(w => w.Guest)
                       .ThenInclude(g => g.User)
                       .FirstOrDefault(wed => wed.WeddingId == Id);
            ViewWedding newModel = new ViewWedding();
            newModel.ThisWedding = ThisWedding;
            newModel.Wedding = viewWedding;
            return View("viewthiswedding", newModel);

        }

        [HttpGet("/unrsvp/{wedId}/{usrId}")]
        public IActionResult UNRSVP(int wedId, int usrId){
            if (HttpContext.Session.GetInt32("User") == null || HttpContext.Session.GetInt32("User") != usrId)
            {
                ViewBag.ses = "Stop trying to break my stuff";
                return RedirectToAction("Logout");
            }
            Wedding ThisWedding = dbContext.Wedding
            .FirstOrDefault(wed => wed.WeddingId == wedId);
         RSVP thisrsvp = dbContext.RSVP
         .Where(u => u.UserId == usrId &&  u.WeddingId == wedId).FirstOrDefault();
          dbContext.Remove(thisrsvp);
          dbContext.SaveChanges();

            return RedirectToAction ("success");
        }


        [HttpGet("/rsvp/{wedId}/{usrId}")]
        public IActionResult RSVP(int wedId, int usrId)
        {
            if (HttpContext.Session.GetInt32("User") == null || HttpContext.Session.GetInt32("User") != usrId  )
            {
                ViewBag.ses = "Stop trying to break my stuff";
                return RedirectToAction("Logout");
            }
            Wedding ThisWedding = dbContext.Wedding
            .FirstOrDefault(wed => wed.WeddingId == wedId);

            User ThisUser = dbContext.Users
            .FirstOrDefault(use => use.UserId == usrId);
             RSVP newrsvp = new RSVP();
             newrsvp.UserId = usrId;
             newrsvp.WeddingId =wedId;
             dbContext.Add(newrsvp);
             dbContext.SaveChanges();
            

            return RedirectToAction("success");
        }


        [HttpGet("/delete/{id}")]
        public IActionResult Delete(int id){
            Wedding ThisWedding = dbContext.Wedding
            .FirstOrDefault(wed => wed.WeddingId == id);

            if (HttpContext.Session.GetInt32("User") == null)
            {
                ViewBag.ses = "Must be logged in";
                return View("Index");
            }
            if(HttpContext.Session.GetInt32("User") != ThisWedding.creator){

                return RedirectToAction("Logout");
            }
            
            dbContext.Remove(ThisWedding);
            dbContext.SaveChanges();

            return RedirectToAction("success" );
        }

        [HttpGet("/logout")]
        public IActionResult Logout(){
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}