
using BugTrackerWeb.Data;

namespace BugTrackerWeb.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        public string TicketId { get; set; }

        public User SubmittedBy { get; set; }


        public static bool createComment(string comment,string ticketId,string submitterEmail)
        {
            string q = "insert into comment values('" + comment + "','" + ticketId + "','" + submitterEmail + "')";
            return Database.nonQuery(q);
        }

    }
}
