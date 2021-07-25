using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Mimir._database
{
    class DBUtils
    {


        

        public static SqlConnection GetDBConnection()
        {
            string datasource = @Properties.Settings.Default.SQL_DATASOURCE;

            string database = Properties.Settings.Default.SQL_DATABASE;
            string username = Properties.Settings.Default.SQL_USER;
            string password = Properties.Settings.Default.SQL_PASSWORD;

            return DBSQLServerUtils.GetDBConnection(datasource, database, username, password);
        }



    }
}
