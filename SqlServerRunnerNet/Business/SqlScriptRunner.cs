using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace SqlServerRunnerNet.Business
{
	public static class SqlScriptRunner
	{
		public static bool RunSqlScriptOnConnection(string connectionString, string filePath, out string errorMessage)
		{
			try
			{
				var scriptContents = File.ReadAllText(filePath);
				var sqlConnection = new SqlConnection(connectionString);
				var server = new Server(new ServerConnection(sqlConnection));
				server.ConnectionContext.ExecuteNonQuery(scriptContents);

				errorMessage = string.Empty;

				return true;
			}
			catch (ExecutionFailureException ex)
			{
				var sqlException = ex.InnerException as SqlException;
				if (sqlException != null)
				{
					errorMessage = string.Format("At line {0}:\n{1}", sqlException.LineNumber, sqlException.Message);
				}
				else if (ex.InnerException != null)
				{
					errorMessage = ex.InnerException.Message;
				}
				else
					errorMessage = ex.Message;
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}

			return false;
		}
	}
}
