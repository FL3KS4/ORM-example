using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Reflection;
using System.Collections;

namespace Project_C2
{
    class ORM
    {

        public static void bindObjectToData(object obj, SqlDataReader data)
        {
            string col_name;
            object col_val;
            PropertyInfo obj_field;
            Type obj_type;

            if (obj != null && data != null)
            {
                obj_type = obj.GetType();

                for (int i  = 0; i < data.FieldCount; i++)
                {
                    col_name = data.GetName(i);

                    obj_field = obj_type.GetProperty(col_name);


                    if (obj_field != null)
                    {

                        col_val = data.GetValue(i);
                        //Console.WriteLine(col_val.ToString());
                        if (col_val.GetType() != typeof(DBNull))
                        {
                            obj_field.SetValue(obj, col_val);
                        }
                    }
                }
            }
        }


        //SELECT for more complex queries
        public static async Task<List<T>> Select<T>(SqlConnection conn, string sql, Dictionary<string, object> test) where T : new()
        {
            List<T> res = null;
            T obj;

            //Console.WriteLine(sql);
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (test.Count != 0)
                {
                    foreach (KeyValuePair<string, object> kvp in test)
                    {
                        {
                            cmd.Parameters.AddWithValue("@" + kvp.Key, kvp.Value);
                        }
                    }
                }

                using (SqlDataReader data = await cmd.ExecuteReaderAsync())
                {
                    if (data.HasRows)
                    {
                        res = new System.Collections.Generic.List<T>();
                        while (data.Read())
                        {
                            obj = new T();
                            bindObjectToData(obj, data);
                            res.Add(obj);
                        }
                    }
                    data.Close();
                }
            }
            return res;
        }

       
        //SELECT I'm using for basic queries  *without parameter (can be used with one)*
        public static System.Collections.Generic.List<T> Select<T>(SqlConnection conn, string sql, params object[] param_vals) where T : new()
        {
            List<T> res = null;
            T obj;

         
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (param_vals != null && param_vals.Length > 0)
                {
                    cmd.Parameters.AddWithValue("@" + nameof(param_vals), param_vals[0]);
                }
                using (SqlDataReader data = cmd.ExecuteReader())
                {
                    if (data.HasRows)
                    {
                        res = new System.Collections.Generic.List<T>();
                        while (data.Read())
                        {
                            obj = new T();
                            bindObjectToData(obj, data);
                            res.Add(obj);
                        }
                    }
                    data.Close();
                }
            }
            return res;
        }

        public static string appendTgth(string original, string newone)
        {
            string complete = original;

            if(!string.IsNullOrEmpty(newone))
            {
                if(!string.IsNullOrEmpty(original))
                    {
                    complete = original + ", " + newone;
                    }
                else
                {
                    complete = newone;
                }
                    
            }
            return complete;
        }

        public static SqlCommand Builder(SqlConnection conn, string sql, params object[] param_values)
        {

           
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (param_values != null && param_values.Length > 0)
                {

                    for (int i = 0; i < param_values.Length; i++)
                    {
                        //Console.WriteLine(i);
                        cmd.Parameters.AddWithValue("@" + param_values[i].ToString(), param_values[++i] ?? DBNull.Value);
                    }
                }
                return cmd;
            }
        }
      

        public static void Insert(SqlConnection conn, object obj, params object[] param_values)
        {
            string field_names = null;
            string param_placeholders = null;

            Type obj_type = obj.GetType();
            ArrayList param_vals = new ArrayList();
          

            foreach (PropertyInfo obj_field in obj_type.GetProperties())
            {
                if (obj_field.Name != "ID")
                {
                    field_names = appendTgth(field_names, obj_field.Name);
                    param_placeholders = appendTgth(param_placeholders, "@" + obj_field.Name);
                    param_vals.Add(obj_field.Name);
                    param_vals.Add(obj_field.GetValue(obj));
                   
                }
            }

            /*if (param_values != null && param_values.Length > 0)
            {
                if(param_vals[0].GetType().ToString().Contains("bool") && param_vals[0].ToString() == true.ToString())
                {
                    string sql = "insert into " + obj_type.Name +
                    " values (" + param_placeholders + "); SELECT SCOPE_IDENTITY() ";

                    SqlCommand cmd = Builder(conn, sql, param_vals.ToArray());
                    object id = cmd.ExecuteScalar();
                    if (id != null)
                        obj_type.GetField("ID").SetValue(obj, id);
                }
            }*/
            string sql = "insert into " + obj_type.Name +
            " values (" + param_placeholders + "); SELECT SCOPE_IDENTITY() ";

            SqlTransaction transaction;
            transaction = conn.BeginTransaction();
          

            SqlCommand cmd = Builder(conn, sql, param_vals.ToArray());
            cmd.Transaction = transaction;
            object id = null;
            try
            {
                id = cmd.ExecuteScalar();
                transaction.Commit();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                Console.WriteLine("  Message: {0}", ex.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
            }
           
            if (id != null)
            {
                if(obj_type.GetProperty("ID") != null)
                {
                    obj_type.GetProperty("ID").SetValue(obj, Convert.ToInt32(id));
                }
                

            }
        }

       

        public static void Delete (SqlConnection conn, object obj)
        {
            object ID = null;

            Type object_type = obj.GetType();
            ArrayList param_vals = new ArrayList();

            foreach (PropertyInfo property in object_type.GetProperties())
            {
                if(property.Name == "ID")
                {
                    ID = property.GetValue(obj);
                    param_vals.Add(property.Name);
                    param_vals.Add(ID);
                }
            }

            string sql = "DELETE FROM " + object_type.Name + " WHERE ID = @ID";

            SqlTransaction transaction;
            transaction = conn.BeginTransaction();



            SqlCommand cmd = Builder(conn, sql, param_vals.ToArray());

            cmd.Transaction = transaction;
           
            try
            {
                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                Console.WriteLine("  Message: {0}", ex.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
            }
                
        }

        public static int Update(SqlConnection conn, object obj)
        {  
            string field_names = null;     
            object id = null;
            object id_name = null;
            int res;

            Type obj_type = obj.GetType();
            ArrayList param_vals =new ArrayList();  
    

            foreach (PropertyInfo obj_field in obj_type.GetProperties())  
            {  

                if (obj_field.Name != "ID")  
                {

                    field_names = appendTgth(field_names, obj_field.Name + "=@" + obj_field.Name);
                    param_vals.Add(obj_field.Name);
                    param_vals.Add(obj_field.GetValue(obj));
                }  
                else
                {
                    id_name = obj_field.Name;
                    id = obj_field.GetValue(obj);
                }                    
                   
            }

            param_vals.Add(id_name);
            param_vals.Add(id);

            string sql = "update " + obj_type.Name + " set " + field_names + " where ID=@ID";

            SqlTransaction transaction;
            transaction = conn.BeginTransaction();

            SqlCommand cmd = Builder(conn, sql, param_vals.ToArray());

            cmd.Transaction = transaction;
           
            try
            {
                res = cmd.ExecuteNonQuery();
                transaction.Commit();
                return res;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                Console.WriteLine("  Message: {0}", ex.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
            }

            return 0;

        }  
    }
}



