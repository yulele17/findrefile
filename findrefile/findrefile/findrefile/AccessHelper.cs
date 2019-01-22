using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace findrefile
{
    class AccessHelper
    {
        //创建链接对象
        private static OleDbConnection conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=F:\winform\mdb\Database1.mdb");

        public static int addHashFile(List<HashFile> list)
        {
            int count = 0;
            foreach (HashFile hashfile in list) {


                conn.Open();

                //增
                string sql = "insert into filedata(hashcode,path) values ('" + hashfile.Hashcode + "','" + hashfile.Path + "')";
                //删 
                //string sql = "delete from 表名 where 字段1="...; 
                //改 
                //string sql = "update student set 学号=" ...; 

                OleDbCommand comm = new OleDbCommand(sql, conn);

                int i = comm.ExecuteNonQuery();

                conn.Close();
                count += i;

            }
            return count;
        }

        /// <summary>
        /// 查找重复的hash值
        /// </summary>
        /// <returns></returns>
        private static List<string> findReHash()
        {
           
            OleDbCommand cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT hashcode FROM filedata GROUP BY hashcode HAVING Count(hashcode) >1 ";
            conn.Open();
            OleDbDataReader dr = cmd.ExecuteReader();
            /*DataTable dt = new DataTable();
            if (dr.HasRows)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dt.Columns.Add(dr.GetName(i));
                }
                dt.Rows.Clear();
            }*/
            List<string> rehash = new List<string>();
            while (dr.Read())
            {
                // DataRow row = dt.NewRow();

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    rehash.Add(dr[i].ToString());
                }
                // dt.Rows.Add(row);
            }
            cmd.Dispose();
            conn.Close();
            return rehash;
        }
        /// <summary>
        /// 查找重复文件
        /// </summary>
        /// <returns></returns>
        public static DataTable findrefile()
        { 
            List<string> list = findReHash();
            DataTable dt = new DataTable();
            string sqlhash = "(";
            foreach (string sthash in list)

            {
                sqlhash += "'" + sthash + "',";

            }
            sqlhash += "'1')";
            OleDbCommand cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT hashcode,path from  filedata where hashcode in " + sqlhash + " order by hashcode";
            conn.Open();
            OleDbDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dt.Columns.Add(dr.GetName(i));
                }

                dt.Columns.Add("是否删除", typeof(bool));
                dt.Rows.Clear();
            }
            string stzhongjian = "";
            while (dr.Read())
            {
                DataRow row = dt.NewRow();
                // for (int i = 0; i < dr.FieldCount; i++)
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    //与上一行相同hash值的便勾选，不同则不勾选
                 
                    if (i < dr.FieldCount) { row[i] = dr[i]; }
                    else if (dr[0].ToString().Equals(stzhongjian))
                    {
                        row[i] = true;
                    }
                    else
                    {
                        row[i] = false;
                    }


                }
                
                dt.Rows.Add(row);
                if (dt.Rows.Count >= 1) {
                    stzhongjian = dt.Rows[dt.Rows.Count - 1][0].ToString();//将当前行的hash值保存，留作下一行判断用
                }

            }
            cmd.Dispose();
            conn.Close();
            return dt;
        }

        /// <summary>
        /// 根据路径删除文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static int delFile(string path)
        {
            
            conn.Open();

            //增
            //string sql = "insert into filedata(hashcode,path) values ('" + hash + "','" + path + "')";
            //删 
            string sql = "delete from filedata where path = '" + path + "'";
            //改 
            //string sql = "update student set 学号=" ...; 

            OleDbCommand comm = new OleDbCommand(sql, conn);

            int i = comm.ExecuteNonQuery();

            conn.Close();
            return i;
        }

        /// <summary>
        /// 清除表中所有记录
        /// </summary>
        /// <param name="path"></param>
        public static  int  delAllFile()
        { 

            conn.Open();

            //增
            //string sql = "insert into filedata(hashcode,path) values ('" + hash + "','" + path + "')";
            //删 
            string sql = "delete from filedata ";
            //改 
            //string sql = "update student set 学号=" ...; 

            OleDbCommand comm = new OleDbCommand(sql, conn);

            int i = comm.ExecuteNonQuery();

            conn.Close();
            return i;
        }

        /// <summary>
        /// 查询表中所有数据
        /// </summary>
        /// <returns></returns>
        public static DataTable findAllfile()
        {
            List<string> list = findReHash();
            DataTable dt = new DataTable();
           
            OleDbCommand cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT ID,hashcode,path from  filedata ";
            conn.Open();
            OleDbDataReader dr = cmd.ExecuteReader();          
            if (dr.HasRows)
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    dt.Columns.Add(dr.GetName(i));
                }
                dt.Columns.Add("是否删除", typeof(bool));
                dt.Rows.Clear();
            }
            while (dr.Read())
            {
                DataRow row = dt.NewRow();
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    row[i] = dr[i];
                }
                dt.Rows.Add(row);
            }
            cmd.Dispose();
            conn.Close();
            return dt;
        }

    }
}
