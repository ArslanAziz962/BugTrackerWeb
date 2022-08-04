using System.Data.SqlClient;
namespace BugTrackerWeb.Data
{
    public class Database
    {
        SqlConnection con;
        SqlCommand cmd;
        public Database()
        {
            con = new SqlConnection("Data Source=DESKTOP-1DF1UDC; initial catalog=BugTracker;integrated security=true");
            con.Open();
        }

        public SqlDataReader? readData(string query)
        {
            try
            {
                cmd = new SqlCommand(query, con);
                return cmd.ExecuteReader();
            }catch(Exception e)
            {
                return null;
            }
            
        }

        public int insertData(string query)
        {
            try
            {
                cmd=new SqlCommand(query,con);
                return cmd.ExecuteNonQuery();

            }catch(Exception e)
            {
                return -1;
            }
        }
        public static bool nonQuery(string q)
        {
            Database db = new Database();

            if (db.insertData(q) != -1)
            {
                db.closeConnection();
                return true;
            }
            db.closeConnection();
            return false;
        }
        public void closeConnection()
        {
            con.Close();
        }
    }
}
