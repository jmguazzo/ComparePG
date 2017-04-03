using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparePG
{
    public class Database
    {
        public string Server { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Schema { get; set; }

        public string ConnectionString()
        {
            Npgsql.NpgsqlConnectionStringBuilder builder = new Npgsql.NpgsqlConnectionStringBuilder();
            builder.ConvertInfinityDateTime = true;
            builder.Database = Name;
            builder.Host = Server;
            builder.Password = Password;
            builder.Username = Login;
            builder.Port = Convert.ToInt32(Port);
            return builder.ConnectionString;
        }

    }
}
