# ORM-example
Custom made ORM for mssql using reflection <br>
Relatively easy to use and to understand


<h2>Example of simple query</h2>

```c#
        public static List<Cars> getAll()
        {
            List<Cars> cars;
            
            //Here use your ConnectionString
            using (SqlConnection connection = new SqlConnection(DBConnector.GetBuilder().ConnectionString))
            {
                connection.Open();
                cars = ORM.Select<Cars>(connection, "SELECT * FROM Cars");
                connection.Close();
            }
            
            return cars;
        }
```

<h2>Example of more advanced query</h2>

```c#
        public async static Task<List<Cars>> getbyBrand(string brand)
        {
            List<Cars> cars;
            Dictionary<string, object> createParam = new Dictionary<string, object>();
            createParam.Add(nameof(brand), brand);
            
            //Here use your ConnectionString
            using (SqlConnection connection = new SqlConnection(DBConnector.GetBuilder().ConnectionString))
            {
                connection.Open();
                cars = await ORM.Select<Cars>(connection, "SELECT * FROM Cars WHERE brand=@brand", createParam);
                connection.Close();
            }

            return cars;
        }
```

<h2>Example of query with 'LIKE'</h2>

```c#
        public async static Task<List<Customer>> getbyLastname(string lastname)
        {
            List<Customer> customers;
            Dictionary<string, object> createParam = new Dictionary<string, object>();
            createParam.Add(nameof(lastname), lastname);
            
            //Here use your ConnectionString
            using (SqlConnection connection = new SqlConnection(DBConnector.GetBuilder().ConnectionString))
            {
                connection.Open();
                
                customers =await ORM.Select<Customer>(connection, "SELECT * FROM Customer WHERE lastname LIKE '%'+@lastname+'%'", createParam);

                connection.Close();
            }           
            return customers;
        }
```
