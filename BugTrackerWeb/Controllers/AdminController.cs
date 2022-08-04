using BugTrackerWeb.Data;
using BugTrackerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace BugTrackerWeb.Controllers
{
    public class AdminController : Controller
    {

        // for image upload
        private readonly IWebHostEnvironment _webHostEnvironment;
        //for image upload
        public AdminController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }


        public IActionResult AllMembers()
        {

            return View(Models.User.getAllUsers());
        }
        public IActionResult EditUser(string email)
        {
            
            User u = Models.User.getUser(email);           

            return View(u);
        }
        [HttpPost]
        public IActionResult EditUser(User user,string btn,List<int> titles)
        {

            if (btn.Equals("Save"))
            {
                if (Models.User.updateUser(user))
                    ViewBag.updated = true;
            }
            else
            {
                if(Models.User.assignProject(user.Email, titles))
                    ViewBag.updated = true;
            }
            return View(user);
        }
        public IActionResult deleteUser(string email)
        {

            Database db = new Database();
            db.insertData("delete Ticket where submitter='" + email + "'");
            db.insertData("delete ProjectAssignment where UserId='" + email + "'");
            db.insertData("delete Comment where userId='" + email + "'");
            db.insertData("delete Users where email='" + email + "'");
            
            db.closeConnection();
            return RedirectToAction("AllMembers");
        }


        public IActionResult CreateUser()
        {
            TempData["allMembers"] = true;
            return RedirectToAction("Register", "Authentication");
        }
        public IActionResult AllProjects()
        {
            if (HomeController.mRole.Equals("Admin"))
            {
                return View(Project.getAllProjects());
            }
            else
            {
                return View(Project.getProjects("select * from project p inner join ProjectAssignment pa on pa.ProjectId=p.id where pa.userId='" + HomeController.mEmail + "'"));
            }
        }


        public IActionResult EditProject(int id)
        {

            Project? p = Project.getProject(id);
            return View(p);

        }
        [HttpPost]
        public IActionResult EditProject(Project p,string btn,List<string> emails)
        {
            if (btn.Equals("Save"))
            {
                if(Project.updateProject(p))
                    ViewBag.updated = true; 
            }
            else
            {
                if (Project.assignProjectToUsers(p.Id, emails))
                    ViewBag.updated = true;
            }
           
            return View(p);

        }
        public IActionResult CreateProject()
        {

            return View();
        }
        [HttpPost]
        public IActionResult CreateProject(Project p)
        {
            string q = "insert into Project values ('" + p.title + "')";
            Database.nonQuery(q);


            return RedirectToAction("AllProjects");
        }

        public IActionResult DeleteProject(int id)
        {
            Database.nonQuery("delete Ticket where ProjectId=" + id);
            Database.nonQuery("delete ProjectAssignment where ProjectId=" + id);
            Database.nonQuery("delete Project where id=" + id);
            return RedirectToAction("AllProjects");
        }

        public IActionResult AllTickets()
        {
            if (HomeController.mRole.Equals("Admin"))
            {
                return View(Ticket.getAllTickets());
            }
            else if (!HomeController.mRole.Equals("User"))
            {
                // getting all the ticket to whom  current project manager and developer are responsbile
                return View(Ticket.getTickets("select * from ticket t where t.ProjectId  in ("+HomeController.projectIds+")"));
            }            
            else 
            {
                // getting all the tickets which are submitted by current project manager
                return View(Ticket.getTickets("select * from ticket t inner join users u on u.email = t.submitter and u.email='" + HomeController.mEmail + "'"));
            }
        }
        public IActionResult EditTicket(string id)
        {
            Ticket? t = Ticket.getTicket(id);
            
           

            return View(t);
        }
        [HttpPost]
        public IActionResult EditTicket(Ticket t)
        {
            
            if(t.Description!=null && t.Title != null)
            {
                if (Ticket.updateTicket(t,_webHostEnvironment))
                    ViewBag.updated = true;
            }
            t = Ticket.getTicket(t.Id);
         
            return View(t);
        }


        public IActionResult CreateTicket()
        {

            return View();
        }
        [HttpPost]
        public IActionResult CreateTicket(Ticket t,string project)
        {
            if(t.Title!=null && t.Description!=null)
            {
                if (project != null)
                {

                    string submitterEmail = HttpContext.Session.GetString("email");
                    if (Ticket.createTicket(t, int.Parse(project), submitterEmail,_webHostEnvironment))
                    {
                        return RedirectToAction("AllTickets");
                    }

                }
                else
                {
                    ViewBag.projectIsNull = true;
                    //return View(t);
                }
            }
            return View(t);
        }
        public IActionResult DeleteTicket(string id)
        {

            Ticket.deleteTicket(id,_webHostEnvironment);
            return RedirectToAction("AllTickets");

        }

        [HttpPost]
        public IActionResult AddComment(string comment,string ticketId)
        {

            Comment.createComment(comment, ticketId, HomeController.mEmail);
            return RedirectToAction("DetailsTicket", "Home",new { id = ticketId });
            
        }
        public IActionResult DeleteComment(int Id,string ticketId)
        {
            Database.nonQuery("delete Comment where id=" + Id);
            return RedirectToAction("DetailsTicket", "Home", new { id = ticketId });

        }

    }
}
