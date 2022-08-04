using BugTrackerWeb.Data;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BugTrackerWeb.Models
{
    public class User
    {
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage ="Enter valid Email")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public string Role { get; set; }




        public static User getUser(string email)
        {
            
            Database db = new Database();
            SqlDataReader? sdr = db.readData("select * from users where  email='"+email+"'");
            User? u = null;
            if(sdr != null)
            {
                sdr.Read();
                 u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };
            }
            return u;
        } 

        public static List<Project> getAssignedProjects(string email)
        {
            Database db = new Database();
            SqlDataReader? sdr = db.readData("select * from project p inner join projectAssignment pa on p.Id=pa.ProjectId where pa.UserId='"+email+"'");
            List<Project> projects = new List<Project>();
            if (sdr != null)
            {
                while (sdr.Read())
                {
                    projects.Add(new Models.Project
                    {
                        Id = int.Parse(sdr["Id"].ToString()),
                        title = sdr["title"].ToString(),

                    });
                }
                sdr.Close();
            }

            db.closeConnection();
            return projects;
        }
        public static bool assignProject(string email,List<int> ids)
        {
            string q = "delete ProjectAssignment where userId='"+email+"'";
            bool success= Database.nonQuery(q);


            q = "insert into ProjectAssignment values ";
                       
            int count = 0;// to handle commas
            foreach(var p in ids)
            {              
                if (count != 0)
                    q += ",";

                q += "("+p+ ",'" + email + "')";
                count++;

            }
            return Database.nonQuery(q) ? true : success;

        }

        public static bool updateUser(User u)
        {
            string q = "update Users set Name='"+u.Name+"' ,  Role='"+u.Role+"' where email='"+u.Email+"'";
            return Database.nonQuery(q);
        }

        public static List<User> getUsers(string q)
        {

            Database db = new Database();
            SqlDataReader sdr = db.readData(q);
            List<User> users = new List<User>();
            if (sdr != null)
            {
                while (sdr.Read())
                {
                    users.Add(new User
                    {
                        Email = sdr["email"].ToString(),
                        Name = sdr["name"].ToString(),
                        Role = sdr["role"].ToString(),
                    });
                }
                sdr.Close();
            }
            return users;

        }
      

        public static List<User> getAllUsers()
        {

            //Database db = new Database();
            //SqlDataReader sdr = db.readData("select * from users");
            //List<User> users = new List<User>();
            //if (sdr != null)
            //{
            //    while (sdr.Read())
            //    {
            //       users.Add(new User
            //        {
            //            Email = sdr["email"].ToString(),
            //            Name = sdr["name"].ToString(),
            //            Role = sdr["role"].ToString(),
            //        });
            //    }
            //    sdr.Close();
            //}
            //return users;
            return getUsers("select * from users");
        }
    }
}
