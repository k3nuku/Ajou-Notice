using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Ajou_Notice.Code.Modules
{
    class IOModule
    {
        /*
        private static bool initialized = false;

        public static bool Initialized
        {
            get { return initialized; }
        }
        private static void CreateDatabase()
        {
            SQLiteConnection.CreateFile("droplet.db");

            using (SQLiteConnection con = new SQLiteConnection("data source = droplet.db"))
            {
                using (SQLiteCommand com = new SQLiteCommand(con))
                {
                    con.Open();

                    com.CommandText = @"CREATE TABLE IF NOT EXISTS `calendar_data` (
                                        `no` integer primary key NOT NULL,
                                        `title` varchar(30) NOT NULL,
                                        `startdate` date  ,
                                        `enddate` date                     )";
                    com.ExecuteNonQuery();


                    com.CommandText = @"CREATE TABLE IF NOT EXISTS `class_data` (
                                    `no` integer primary key NOT NULL ,
                                    `clubname` varchar(20) NOT NULL,
                                    `clubcode` varchar(10) NOT NULL,
                                    `staffname` varchar(10) NOT NULL,
                                    `clubid` varchar(20) NOT NULL,
                                    `yearsmst` varchar(6) NOT NULL,
                                    `time` char(1) NOT NULL,
                                    `room` varchar(6) NOT NULL               )";

                    com.ExecuteNonQuery();

                    com.CommandText = @"CREATE TABLE IF NOT EXISTS `eclass_data` (
                                        `no` integer primary key NOT NULL ,
                                        `title` varchar(20) NOT NULL,
                                        `type` varchar(10) NOT NULL,
                                        `duedate` varchar(12) NOT NULL,
                                        `finished` tinyint(1) NOT NULL,
                                        `filelink` varchar(50) NOT NULL         )";
                    com.ExecuteNonQuery();

                    com.CommandText = @"CREATE TABLE IF NOT EXISTS `notice_list` (
                                    `no` integer primary key NOT NULL ,
                                    `title` varchar(30) NOT NULL,
                                    `startdate` date NOT NULL,
                                    `duedate` date NOT NULL,
                                    `section` varchar(5) NOT NULL,
                                    `link` varchar(30) NOT NULL,
                                    `filelink` varchar(30) NOT NULL       )";
                    com.ExecuteNonQuery();
                }
            }
        }

        public static bool initialize_IO()
        {
            if (new FileStream("droplet.db", FileMode.Open) == null)
            {
                try
                {
                    CreateDatabase();
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }*/
    }
}
