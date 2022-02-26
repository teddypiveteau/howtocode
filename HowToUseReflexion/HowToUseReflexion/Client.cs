using System;
namespace HowToUseReflexion
{
    public class Client
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Firstname { get; set; }


        public Client() { }

        public Client(string name, string firstname)
        {
            Name = name;
            Firstname = firstname;
        }
    }
}
