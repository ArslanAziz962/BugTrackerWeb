using BugTrackerWeb.Data;
using BugTrackerWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
namespace BugTrackerWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public static string? mEmail;
        public static string? mName;
        public static   string? mRole;

        //project manager's project ids 
        public static string projectIds="";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            mName = HttpContext.Session.GetString("name");
            mEmail = HttpContext.Session.GetString("email");
            mRole = HttpContext.Session.GetString("role");
            DetailsViewModel details = new DetailsViewModel();

            if (mName == null)
            {
                return RedirectToAction("Index", "Authentication");
            }

        

            if (mRole.Equals("Admin" ))
            {
                List<Models.User> users = Models.User.getAllUsers();
                details.users = users;

                List<Project> projects = Project.getAllProjects();
                details.projects = projects;

                //List<Ticket> tickets = Ticket.getAllTickets();
                //details.tickets = tickets;
            }
            else if(mRole.Equals("ProjectManager") || mRole.Equals("Developer"))
            {


                //get all projects which are assigned to current logged in user
                List<Project> projects = Project.getProjects("select * from project p inner join ProjectAssignment pa on pa.ProjectId=p.id where pa.userId='" + mEmail + "'");
                details.projects = projects;

                // get all the ids of projects which are assigned to this project manager
                projectIds = "";
                for(int i=0; i<projects.Count;i++)
                {
                    if (i != 0)
                        projectIds += ",";
                    projectIds += "'" + projects[i].Id + "'";
                }

                List<Models.User> users = Models.User.getUsers("select * from users u inner join ProjectAssignment pa on pa.UserId=u.email where ProjectId in("+projectIds+")");
                details.users = users;


                //// getting all the tickets which are submitted by current logged in user
                //List<Ticket> tickets = Ticket.getTickets("select * from ticket t inner join users u on u.email = t.submitter and u.email='"+ mEmail+ "'");
                //details.tickets = tickets;
            }

            // getting all the tickets which are submitted by current logged in user
            List<Ticket> tickets = Ticket.getTickets("select * from ticket t inner join users u on u.email = t.submitter and u.email='" + mEmail + "'");
            details.tickets = tickets;
            return View(details);
        }


        private DetailsViewModel getUserDetails(string q)
        {
            Database db = new Database();
            SqlDataReader? sdr = db.readData(q);
            DetailsViewModel details = new DetailsViewModel();

            if (sdr != null)
            {
                while (sdr.Read())
                {
                    Project p = new Project { Id = int.Parse(sdr["PId"].ToString()), title = sdr["PTitle"].ToString() };
                    details.projects.Add(p);

                    User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };

                    if (sdr["TId"].ToString() == String.Empty)
                        continue;
                    Ticket t = new Ticket { Id = sdr["TId"].ToString(), Title = sdr["TicketTitle"].ToString(), Description = sdr["description"].ToString(), Priority = sdr["priority"].ToString(), Type = sdr["Type"].ToString(), Status = sdr["status"].ToString(), project = p, submitter = u };

                    details.tickets.Add(t);

                }
            }
            db.closeConnection();
            return details;
        }

    
        [HttpPost]
        public IActionResult DetailsUser(User? user,string? email)
        {
            DetailsViewModel details = new DetailsViewModel();
            if (email != null && user.Name == null)
            {
                user = Models.User.getUser(email);
            }
            ViewBag.user = user;
            ViewBag.name = user.Name;

            if (mRole.Equals("Admin"))
            {
                

                //get project which is assigned to user and tickets which are issued against these projects
               details = getUserDetails("Select p.Id  as PId,p.title as PTitle,t.id as TId,t.title as TicketTitle,t.description,t.priority,t.type,t.status,t.projectId,u.Name,u.email,u.role from project p inner join ProjectAssignment pa on p.id=pa.ProjectId left join Ticket t on t.ProjectId=p.id left join users u on u.email=t.submitter  where pa.userid='" + user.Email + "' ");
               

            }
            else if (mRole.Equals("ProjectManager"))
            {
                //get projects and users which are assigned by this project manager 
               details = getUserDetails("Select p.Id  as PId,p.title as PTitle,t.id as TId,t.title as TicketTitle,t.description,t.priority,t.type,t.status,t.projectId,u.Name,u.email,u.role from project p inner join ProjectAssignment pa on p.id=pa.ProjectId left join Ticket t on t.ProjectId=p.id left join users u on u.email=t.submitter  where pa.userid='" + user.Email + "' and pa.ProjectId in(" + projectIds + ")");
               
            }

            return View(details);
        }

        [HttpPost]
        public IActionResult DetailsProject(Project? p,string? id)
        {

            if(id!=null && (p==null || p.title == null))
            {

                p = Project.getProject(int.Parse(id));
            }

            ViewBag.project = p;
            //Database db = new Database();
            
            //SqlDataReader sdr = db.readData("select * from users u inner join projectAssignment pa on pa.UserId=u.email and pa.ProjectId=" + p.Id);
            DetailsViewModel details = new DetailsViewModel();
            details.users= Models.User.getUsers("select * from users u inner join projectAssignment pa on pa.UserId=u.email and pa.ProjectId=" + p.Id); ;

            //if (sdr != null)
            //{
            //    while (sdr.Read())
            //    {                    
            //        User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };
            //        details.users.Add(u);

            //    }
            //}
            //sdr.Close();
            //sdr = db.readData("select * from ticket t inner join users u on u.email=t.submitter where t.projectid=" + p.Id);
            //  if (sdr != null)
            //  {
            //    while (sdr.Read())
            //    {


            //        User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };

            //        Ticket t = new Ticket { Id = sdr["Id"].ToString(), Title = sdr["Title"].ToString(), Description = sdr["description"].ToString(), Priority = sdr["priority"].ToString(), Type = sdr["Type"].ToString(), Status = sdr["status"].ToString(),submitter=u};                    
            //        details.tickets.Add(t);

            //    }
            //}
            details.tickets = Ticket.getTickets("select * from ticket t inner join users u on u.email=t.submitter where t.projectid=" + p.Id);
            return View(details);
        }


        
        public IActionResult DetailsTicket(Ticket? t,string? id)
        {
            if (id != null && (t == null || t.Title == null))
            {

                t = Ticket.getTicket(id);
            }

            ViewBag.ticket = t;
            Database db = new Database();
            SqlDataReader sdr = db.readData("select * from users u inner join ticket t on t.submitter=u.email where t.id='" + t.Id+"'");
            DetailsViewModel details = new DetailsViewModel();
            
            if (sdr != null)
            {
                sdr.Read();
               
                   User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };
                //submiiter who submitted the ticket
                ViewBag.users = u;
            }
           
            sdr.Close();
            sdr = db.readData("select * from Project p inner join ticket t on t.ProjectId=p.id where t.id='" + t.Id+"'");
            if (sdr != null)
            {
                sdr.Read();
                // project against which the ticket is submitted (i.e., ticket is unique for each project)
                Project p = new Project { Id = int.Parse(sdr["Id"].ToString()), title = sdr["Title"].ToString() };
                ViewBag.ticket.project = p;
                sdr.Close();

                //getting all the users which are assigned to the project against which ticket is issued
                sdr = db.readData("select * from users u inner join projectAssignment pa on u.email=pa.UserId where pa.projectid=" + p.Id);
                if (sdr != null)
                {
                    while (sdr.Read())
                    {
                        User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };
                        details.users.Add(u);

                    }
                }
            }
            ViewBag.comments = Ticket.getComments(t.Id);

            return View(details);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}