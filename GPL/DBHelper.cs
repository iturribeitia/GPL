/*
 ____                                                         _   _               
|  _ \ _ __ ___   __ _ _ __ __ _ _ __ ___  _ __ ___   ___  __| | | |__  _   _   _ 
| |_) | '__/ _ \ / _` | '__/ _` | '_ ` _ \| '_ ` _ \ / _ \/ _` | | '_ \| | | | (_)
|  __/| | | (_) | (_| | | | (_| | | | | | | | | | | |  __/ (_| | | |_) | |_| |  _ 
|_|   |_|  \___/ \__, |_|  \__,_|_| |_| |_|_| |_| |_|\___|\__,_| |_.__/ \__, | (_)
                 |___/                                                  |___/     
 __  __                         
|  \/  | __ _ _ __ ___ ___  ___ 
| |\/| |/ _` | '__/ __/ _ \/ __|
| |  | | (_| | | | (_| (_) \__ \
|_|  |_|\__,_|_|  \___\___/|___/

 ___ _                   _ _          _ _   _       
|_ _| |_ _   _ _ __ _ __(_) |__   ___(_) |_(_) __ _ 
 | || __| | | | '__| '__| | '_ \ / _ \ | __| |/ _` |
 | || |_| |_| | |  | |  | | |_) |  __/ | |_| | (_| |
|___|\__|\__,_|_|  |_|  |_|_.__/ \___|_|\__|_|\__,_|
 
*/

/* This file is part of GPL DLL.

    GPL is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version of the License.

    GPL is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with GPL.  If not, see <http://www.gnu.org/licenses/>.
/* 

/*
 Copyright  Code4Forever 2012.  All rights reserved.
 Visit code4forever.blogspot.com for more information about us.

 Modified and enhanced by Marcos A. Iturribeitia.

 This Class acts as Data Access Layer.

 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace GPL
{
    /// <summary>
    /// dbHelper is a helper class that takes the common data classes and allows you
    /// to specify the provider to use, execute commands, add parameters, and return datasets.
    /// See examples for usage.
    /// </summary>
    public class DBHelper : IDisposable
    {
        #region private members
        private string _connectionstring = "";
        private DbConnection _connection;
        private DbCommand _command;
        private DbProviderFactory _factory = null;
        private bool _Rollbacked = false;
        #endregion private members

        #region properties

        /// <summary>
        /// Gets or Sets the connection string for the database
        /// </summary>
        public string connectionstring
        {
            get
            {
                return _connectionstring;
            }
            set
            {
                if (value != "")
                {
                    _connectionstring = value;
                }
            }
        }

        /// <summary>
        /// Gets the connection object for the database
        /// </summary>
        public DbConnection connection
        {
            get
            {
                return _connection;
            }
        }

        /// <summary>
        /// Gets the command object for the database
        /// </summary>
        public DbCommand command
        {
            get
            {
                return _command;
            }
        }

        /// <summary>
        /// Get or Set to use UseTransaction
        /// </summary>
        public Boolean UseTransaction
        { get; set; }

        /// <summary>
        /// Get Rollbacked
        /// </summary>
        public Boolean Rollbacked
        { get { return _Rollbacked; } }

        #endregion properties

        # region methods

        /// <summary>
        /// Constructor for this class.
        /// </summary>
        public DBHelper()
        {
            UseTransaction = true;
        }

        /// <summary>
        /// Constructor for this class.
        /// </summary>
        /// <param name="useTransaction">useTransaction</param>
        public DBHelper(Boolean useTransaction)
        {
            UseTransaction = useTransaction;
        }


        /// <summary>
        /// Determines the correct provider to use and sets up the connection and command
        /// objects for use in other methods
        /// </summary>
        /// <param name="connectString">The full connection string to the database.</param>
        /// <param name="providerList">The provider list.</param>
        /// <param name="CommandTimeout">The command timeout.</param>
        public void CreateDBObjects(string connectString, Providers providerList, int? CommandTimeout = null)
        {
            //CreateDBObjects(connectString, providerList, null);
            switch (providerList)
            {
                case Providers.SqlServer:
                    _factory = SqlClientFactory.Instance;
                    break;

                case Providers.Oracle:
                    _factory = OracleClientFactory.Instance;
                    break;

                case Providers.OleDB:
                    _factory = OleDbFactory.Instance;
                    break;

                case Providers.ODBC:
                    _factory = OdbcFactory.Instance;
                    break;
            }

            _connection = _factory.CreateConnection();

            _command = _factory.CreateCommand();

            if (!CommandTimeout.Equals(null))
                _command.CommandTimeout = (int)CommandTimeout;

            _connection.ConnectionString = connectString;
            _command.Connection = connection;
        }


        /// <summary>
        /// Gets the name of the provider from provider.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        public Providers GetProviderFromProviderName(string providerName)
        {
            DBHelper.Providers myProvider;
            switch (providerName)
            {
                case "System.Data.SqlClient":
                    myProvider = DBHelper.Providers.SqlServer;
                    break;

                case "System.Data.Odbc":
                    myProvider = DBHelper.Providers.ODBC;
                    break;

                case "System.Data.OleDb":
                    myProvider = DBHelper.Providers.OleDB;
                    break;

                case "System.Data.OracleClient":
                    myProvider = DBHelper.Providers.Oracle;
                    break;

                default:
                    myProvider = DBHelper.Providers.SqlServer;
                    break;
            }
            return myProvider;
        }

        /// <summary>
        /// Creates a parameter and adds it to the command object
        /// </summary>
        /// <param name="name">The parameter name</param>
        /// <param name="value">The paremeter value</param>
        /// <returns></returns>
        public int AddParameter(string name, object value)
        {
            DbParameter parm = _factory.CreateParameter();
            parm.ParameterName = name;
            parm.Value = value;
            return command.Parameters.Add(parm);
        }

        /// <summary>
        /// Creates a parameter and adds it to the command object
        /// </summary>
        /// <param name="parameter">A parameter object</param>
        /// <returns></returns>
        public int AddParameter(DbParameter parameter)
        {
            return command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Execute Dispose
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
                if (command != null) command.Parameters.Clear();
                if (command != null) command.Dispose();
                if (connection != null) connection.Dispose();
            }
            catch
            {
                throw;
            }
        }

        #endregion methods

        #region transactions

        /// <summary>
        /// Starts a transaction for the command object
        /// </summary>
        public void BeginTransaction()
        {
            if (connection.State == System.Data.ConnectionState.Closed)
            {
                connection.Open();
            }
            command.Transaction = connection.BeginTransaction();
        }

        /// <summary>
        /// Commits a transaction for the command object
        /// </summary>
        public void CommitTransaction()
        {
            if (command.Transaction != null)
                command.Transaction.Commit();
            //connection.Close();
        }

        /// <summary>
        /// Rolls back the transaction for the command object
        /// </summary>
        public void RollbackTransaction()
        {
            if (command.Transaction != null)
                command.Transaction.Rollback();
            //connection.Close();
        }

        #endregion

        #region execute database functions

        /// <summary>
        /// Executes a statement that does not return a result set, such as an INSERT, UPDATE, DELETE, or a data definition statement
        /// </summary>
        /// <param name="commandText">The query, either SQL or Procedures</param>
        /// <param name="commandType">The command type, text, storedprocedure, or tabledirect</param>
        /// <param name="connectionState">State of the connection after the execution.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(string commandText, CommandType commandType, ConnectionState connectionState = ConnectionState.Closed)
        {
            _Rollbacked = false;
            command.CommandText = commandText;
            command.CommandType = commandType;
            int i = -1;
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                if (UseTransaction) BeginTransaction();

                i = command.ExecuteNonQuery();
            }
            catch
            {
                if (UseTransaction)
                {
                    _Rollbacked = true;
                    RollbackTransaction();
                }
                throw;
            }
            finally
            {
                if (UseTransaction && !_Rollbacked) CommitTransaction();
                //command.Parameters.Clear();

                if (connection.State == System.Data.ConnectionState.Open && connectionState == ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                    //command.Dispose();
                }
            }

            return i;
        }

        /// <summary>
        /// Executes a statement that returns a single value.
        /// If this method is called on a query that returns multiple rows and columns, only the first column of the first row is returned.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <param name="connectionState">State of the connection after the execution.</param>
        /// <returns></returns>
        public object ExecuteScalar(string commandText, CommandType commandType, ConnectionState connectionState = ConnectionState.Closed)
        {
            _Rollbacked = false;
            command.CommandText = commandText;
            command.CommandType = commandType;
            object obj = null;

            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }

                if (UseTransaction) BeginTransaction();
                obj = command.ExecuteScalar();
            }
            catch
            {
                if (UseTransaction)
                {
                    _Rollbacked = true;
                    RollbackTransaction();
                }
                throw;
            }
            finally
            {
                if (UseTransaction && !_Rollbacked) CommitTransaction();
                //command.Parameters.Clear();

                if (connection.State == System.Data.ConnectionState.Open && connectionState == ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                    command.Dispose();
                }
            }

            return obj;
        }

        /// <summary>
        /// Executes a SQL statement that returns a result set.
        /// </summary>
        /// <param name="commandText">The query, either SQL or Procedures</param>
        /// <param name="commandType">The command type, text, storedprocedure, or tabledirect</param>
        /// <param name="connectionState">State of the connection after the execution.</param>
        /// <returns>A datareader object</returns>
        public DbDataReader ExecuteReader(string commandText, CommandType commandType, ConnectionState connectionState = ConnectionState.Closed)
        {
            command.CommandText = commandText;
            command.CommandType = commandType;
            DbDataReader reader = null;
            try
            {
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
                if (connectionState == System.Data.ConnectionState.Closed)
                {
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                }
                else
                {
                    reader = command.ExecuteReader();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                //command.Parameters.Clear();

                if (connection.State == System.Data.ConnectionState.Open && connectionState == ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                    command.Dispose();
                }
            }

            return reader;
        }

        /// <summary>
        /// Generates a dataset
        /// </summary>
        /// <param name="commandText">The query, either SQL or Procedures</param>
        /// <param name="commandType">The command type, text, storedprocedure, or tabledirect</param>
        /// <param name="connectionState">The connection state</param>
        /// <returns>A dataset containing data from the database</returns>
        public DataSet GetDataSet(string commandText, CommandType commandType, ConnectionState connectionState = ConnectionState.Closed)
        {
            DbDataAdapter adapter = _factory.CreateDataAdapter();
            command.CommandText = commandText;
            command.CommandType = commandType;
            adapter.SelectCommand = command;
            DataSet ds = new DataSet();
            try
            {
                adapter.Fill(ds);
            }
            catch
            {
                throw;
            }
            finally
            {
                //command.Parameters.Clear();

                if (connection.State == System.Data.ConnectionState.Open && connectionState == ConnectionState.Closed)
                {
                    connection.Close();
                    connection.Dispose();
                    command.Dispose();
                }
            }
            return ds;
        }

        #endregion

        #region Bulk Insert

        /// <summary>
        /// Do SqlBulkCopy of the givin DbDataReader
        /// </summary>
        /// <param name="ConnectionString">The connection string.</param>
        /// <param name="dataTable">The dt.</param>
        /// <param name="TableName">Name of the table.</param>
        /// <param name="BulkBatchSize">Size of the bulk batch.</param>
        /// <param name="columnMapping">The column mapping.</param>
        public static void DoSqlBulkCopy(string ConnectionString, DataTable dataTable, string TableName, int BulkBatchSize, List<SqlBulkCopyColumnMapping> columnMapping = null)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(ConnectionString))
                {
                    bulkCopy.DestinationTableName = TableName;
                    // Set timeout to 0 to avoid timeout errors.
                    bulkCopy.BulkCopyTimeout = 0;
                    bulkCopy.BatchSize = BulkBatchSize;

                    // create the mapping if it is supplied.
                    if (columnMapping != null && columnMapping.Count >0)
                        foreach (var cm in columnMapping)
                        {
                            bulkCopy.ColumnMappings.Add(cm);
                        }

                    // Write from the source to the destination.
                    bulkCopy.WriteToServer(dataTable);

                    bulkCopy.Close();
                }
        }

        /// <summary>
        /// Do SqlBulkCopy of the givin DbDataReader
        /// </summary>
        /// <param name="ConnectionString">The connection string.</param>
        /// <param name="dataReader">The DataReader.</param>
        /// <param name="TableName">Name of the table.</param>
        /// <param name="BulkBatchSize">Size of the bulk batch.</param>
        /// <param name="columnMapping">The column mapping.</param>
        public static void DoSqlBulkCopy(string ConnectionString, IDataReader dataReader, string TableName, int BulkBatchSize, List<SqlBulkCopyColumnMapping> columnMapping = null)
        {
            // TODO try to use a generic parameter to avoid this overloading, look the in extension as example.
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(ConnectionString))
            {
                bulkCopy.DestinationTableName = TableName;
                // Set timeout to 0 to avoid timeout errors.
                bulkCopy.BulkCopyTimeout = 0;
                bulkCopy.BatchSize = BulkBatchSize;

                // create the mapping if it is supplied.
                if (columnMapping != null && columnMapping.Count > 0)
                    foreach (var cm in columnMapping)
                    {
                        bulkCopy.ColumnMappings.Add(cm);
                    }

                // Write from the source to the destination.
                bulkCopy.WriteToServer(dataReader);

                bulkCopy.Close();
            }
        }

        #endregion Bulk Insert

    }
}