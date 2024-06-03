using System.Windows.Media;

namespace adrilight_shared.Models.User
{
    public class AppUser
    {
        public AppUser()
        {

        }
        public AppUser(string name, string loginName, string databaseUserName, string loginPassword, Geometry geometry)
        {
            Name = name;
            DataBaseUserName = databaseUserName;
            LoginPassword = loginPassword;
            Geometry = geometry;
            LoginName = loginName;
        }
        public string Name { get; set; }
        public int Level { get; set; }
        public string DataBaseUserName { get; set; }
        public string LoginPassword { get; set; }
        public string LoginName { get; set; }
        public Geometry Geometry { get; set; }
    }
}
