﻿using System;
using System.Collections.Generic;
using ALE.ETLBox;
using ALE.ETLBox.ConnectionManager;
using ALE.ETLBox.ControlFlow;
using ALE.ETLBox.DataFlow;

namespace ALE.BasicExample
{
    class Program
    {

        static void Main(string[] args)
        {
            var masterConnection = new SqlConnectionManager("Data Source=.;Integrated Security=false;User=sa;password=reallyStrongPwd123");
            DropDatabaseTask.DropIfExists(masterConnection, "demo");
            CreateDatabaseTask.Create(masterConnection, "demo");

            var dbConnection = new SqlConnectionManager("Data Source=.;Initial Catalog=demo;Integrated Security=false;User=sa;password=reallyStrongPwd123");

            CreateTableTask.Create(dbConnection, "Table1", new List<TableColumn>()
            {
                new TableColumn("ID","int",allowNulls:false, isPrimaryKey:true, isIdentity:true),
                new TableColumn("Col1","nvarchar(100)",allowNulls:true),
                new TableColumn("Col2","smallint",allowNulls:true)
            });

            CsvSource<string[]> source = new CsvSource<string[]>("input.csv");
            RowTransformation<string[], MyData> row = new RowTransformation<string[], MyData>(
            input => new MyData() { Col1 = input[0], Col2 = input[1] });
            DbDestination<MyData> dest = new DbDestination<MyData>(dbConnection, "Table1");

            source.LinkTo(row);
            row.LinkTo(dest);
            source.Execute();
            dest.Wait();

            SqlTask.ExecuteReader(dbConnection, "Read all data from table1",
            "select Col1, Col2 from Table1",
                col1 => Console.WriteLine(col1.ToString() + ","),
                col2 => Console.WriteLine(col2.ToString()));

            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }

    }
}