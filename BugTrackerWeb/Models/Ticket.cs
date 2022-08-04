using BugTrackerWeb.Data;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;

namespace BugTrackerWeb.Models
{
    public class Ticket
    {
        public string Id { get; set; }
        [Required(ErrorMessage ="Title is Required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is Required")]
        public string Description { get; set; }
        
        public string Priority { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }

        public IFormFile file { get; set; }
        public string imgPath { get; set; }

        public Project project { get; set; }
        public User submitter { get; set; }


        public static bool createTicket(Ticket t, int projectId,string submitterEmail,IWebHostEnvironment webHostEnvironment)
        {
            t.Id = DateTime.Now.ToString("ddMMyyyyhhmmsstt");
            var task = saveAndGetImagePath(t,  webHostEnvironment);
            task.Wait();
            var path = task.Result;
            string q = "insert into Ticket values('"+t.Id+"','"+t.Title+"','"+t.Description+"','"+t.Priority+"','"+t.Type+"','"+t.Status+"','"+path+"',"+projectId+",'"+submitterEmail+"')";
            return Database.nonQuery(q);

        }


        public static bool deleteTicket(string id,IWebHostEnvironment webHostEnvironment)
        {
            Database.nonQuery("delete comment where ticketId='" + id + "'");
            Ticket t = getTicket(id);
            if (Database.nonQuery("delete Ticket where Id='" + id + "'"))
            {               
                if (t.imgPath != null)
                {
                    File.Delete(webHostEnvironment.WebRootPath + t.imgPath);
                    
                }
                return true;
            }
            return false;
        }

        public static List<Ticket> getTickets(string q)
        {
            Database db = new Database();
            SqlDataReader sdr = db.readData(q);
            List<Ticket> tickets = new List<Ticket>();
            if (sdr != null)
            {
                while (sdr.Read())
                {
                    Ticket t;
                    // if we are fetching user as well
                    if (sdr.VisibleFieldCount > 9)
                    {
                        User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };

                         t = new Ticket { Id = sdr["Id"].ToString(), Title = sdr["Title"].ToString(), Description = sdr["description"].ToString(), Priority = sdr["priority"].ToString(), Type = sdr["Type"].ToString(), Status = sdr["status"].ToString(), imgPath = sdr["ImagePath"].ToString(), submitter = u };
                    }
                    else
                    {
                         t = new Ticket { Id = sdr["Id"].ToString(), Title = sdr["Title"].ToString(), Description = sdr["description"].ToString(), Priority = sdr["priority"].ToString(), Type = sdr["Type"].ToString(), Status = sdr["status"].ToString(), imgPath = sdr["ImagePath"].ToString()};
                    }
                    
                    tickets.Add(t);
                }
            }
            return tickets;
        }

        public static List<Ticket> getAllTickets()
        {
            //Database db = new Database();
            //SqlDataReader sdr = db.readData("select * from ticket t inner join users u on u.email = t.submitter");
            //List<Ticket> tickets = new List<Ticket>();
            //if (sdr != null)
            //{
            //    while (sdr.Read())
            //    {
            //        User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };

            //        Ticket t = new Ticket { Id = sdr["Id"].ToString(), Title = sdr["Title"].ToString(), Description = sdr["description"].ToString(), Priority = sdr["priority"].ToString(), Type = sdr["Type"].ToString(), Status = sdr["status"].ToString(), imgPath = sdr["ImagePath"].ToString(), submitter = u };
            //        tickets.Add(t);
            //    }
            //}
            //return tickets;
            return getTickets("select * from ticket t inner join users u on u.email = t.submitter");
        }
        public static Ticket? getTicket(string id)
        {
            Database db = new Database();
            SqlDataReader? sdr = db.readData("select * from ticket t inner join users u on u.email = t.submitter where t.Id='"+id+"'");
            Ticket? t = null;
            if (sdr != null)
            {
                sdr.Read();
               
                User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };

                t = new Ticket { Id =sdr["Id"].ToString(), Title = sdr["Title"].ToString(), Description = sdr["description"].ToString(), Priority = sdr["priority"].ToString(), Type = sdr["Type"].ToString(), Status = sdr["status"].ToString(),imgPath= sdr["ImagePath"].ToString(), submitter = u };                                                    
            }
            return t;
        }

        public static bool updateTicket(Ticket t,IWebHostEnvironment webHostEnvironment)
        {

             var task = saveAndGetImagePath(t, webHostEnvironment);
            task.Wait();
            var path = task.Result;
            string q;
            if (path != null) {
                 q = "update Ticket set title='" + t.Title + "', description='" + t.Description + "' ,priority='" + t.Priority + "', type ='" + t.Type + "',status='" + t.Status + "',ImagePath='"+path+"' where id='" + t.Id+"'";

            }
            else                
            {
                q = "update Ticket set title='" + t.Title + "', description='" + t.Description + "' ,priority='" + t.Priority + "', type ='" + t.Type + "',status='" + t.Status + "' where id='" + t.Id+"'";
            }
            return Database.nonQuery(q);
        }


        private async static Task<string> saveAndGetImagePath(Ticket t, IWebHostEnvironment webHostEnvironment)
        {
            if (t.file != null) {
                string webRootPath = webHostEnvironment.WebRootPath+"/Images/";

                //path = Path.Combine(webRootPath, "CSS");


                string sugName = t.Id + Path.GetExtension(t.file.FileName);

                String path = Path.Combine(webRootPath, sugName);

                //if file already exists then change the file name and make sure that file is 
                // submitted through from other ticket. If that file is submitted by current 
                // ticket then just delete it and upload new image                                

                // to handle edit of image
                if (t.imgPath != null)
                {
                    //String tempPath = t.imgPath;                    
                    //tempPath = tempPath.Replace(Path.GetExtension(t.imgPath),"");
                    
                    //if (path.Contains(tempPath))
                    //{
                        File.Delete(webHostEnvironment.WebRootPath + t.imgPath);
                    //}
                }
                //int count = 1;
                //while ( true)
                //{
                   
                //    if (File.Exists(path))
                //    {
                     
                //        string newSugName = t.Id + count + Path.GetExtension(t.file.FileName);
                //        path = path.Replace(sugName, newSugName);
                //        sugName = newSugName;
                //        count++;
                //        continue;
                //    }
                //    break;
                //}

                using (Stream fileStream = new FileStream(path, FileMode.Create))
                {
                   
                    await t.file.CopyToAsync(fileStream);
                }
                return "/Images/" + sugName;
            }
            return null;
        }

        public static List<Comment> getComments(string ticketId)
        {
            Database db = new Database();
            SqlDataReader sdr =db.readData("select * from comment c inner join Users u on u.Email=c.userId where TicketId='" + ticketId + "'");
            List<Comment> comments = new List<Comment>();
            if (sdr != null)
            {

                while (sdr.Read())
                {
                    User u = new User { Name = sdr["name"].ToString(), Email = sdr["email"].ToString(), Role = sdr["role"].ToString() };
                    Comment c = new Comment { Id = int.Parse(sdr["Id"].ToString()), CommentText = sdr["Comment"].ToString(),TicketId=ticketId,SubmittedBy=u };
                    comments.Add(c);
                }
                sdr.Close();
            }
            db.closeConnection();
            return comments;

        }
        

    }
}
