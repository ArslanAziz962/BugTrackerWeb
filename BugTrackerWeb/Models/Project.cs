using BugTrackerWeb.Data;
using System.Data.SqlClient;

namespace BugTrackerWeb.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string title { get; set; }

        public static List<Project> getProjects(string q)
        {
            Database db = new Database();
            SqlDataReader sdr = db.readData(q);
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

        public static List<Project> getAllProjects()
        {
            //Database db = new Database();
            //SqlDataReader sdr = db.readData("select * from project");
            //List<Project> projects = new List<Project>();
            //if (sdr != null)
            //{
            //    while (sdr.Read())
            //    {
            //        projects.Add(new Models.Project
            //        {
            //            Id = int.Parse(sdr["Id"].ToString()),
            //            title = sdr["title"].ToString(),

            //        });
            //    }
            //    sdr.Close();
            //}

            //db.closeConnection();
            //return projects;
            return getProjects("select * from project");
        }
        public static Project? getProject(int id)
        {
            Database db = new Database();
            SqlDataReader? sdr = db.readData("select * from project where id=" + id);
            Project? p = null;
            if (sdr != null)
            {
                sdr.Read();
                p = new Project
                {
                    Id = int.Parse(sdr["Id"].ToString()),
                    title = sdr["title"].ToString(),

                };
            }
            db.closeConnection();
            return p;
        }
        public static List<User> getAssignedUsers(int id)
        {
            Database db = new Database();
            SqlDataReader? sdr = db.readData("select * from Users u inner join projectAssignment pa on u.email=pa.UserId where pa.ProjectId="+id);
            List<User> users = new List<User>();
            if (sdr != null)
            {
                while (sdr.Read())
                {
                    users.Add(new Models.User
                    {
                        Email = sdr["email"].ToString(),
                        Name = sdr["name"].ToString(),
                        Role = sdr["role"].ToString(),
                    });
                }
                sdr.Close();
            }

            db.closeConnection();
            return users;
        }
        public static bool updateProject(Project p)
        {
            string q = "update project set title='"+p.title+"' where Id=" + p.Id;
            return Database.nonQuery(q);
        }
        public static bool assignProjectToUsers(int pId,List<string> emails)
        {
            
            string q = "delete ProjectAssignment where ProjectId=" + pId;
            bool success=Database.nonQuery(q);


            q = "insert into ProjectAssignment values ";

            int count = 0;// to handle commas
            foreach (var e in emails)
            {
                if (count != 0)
                    q += ",";

                q += "(" + pId + ",'" + e + "')";
                count++;

            }
            return Database.nonQuery(q) ? true: success;
        }
       

    }
}
