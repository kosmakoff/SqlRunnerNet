using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SqlServerRunnerNet.ViewModel;

namespace SqlServerRunnerNet.Business
{
	public static class SqlScriptRunner
	{
		public static bool RunSqlScriptOnConnection(string connectionString, ScriptViewModel script)
		{
			try
			{
				var filePath = script.Path;

				var scriptContents = File.ReadAllText(filePath);
				var sqlConnection = new SqlConnection(connectionString);
				var server = new Server(new ServerConnection(sqlConnection));
				server.ConnectionContext.ExecuteNonQuery(scriptContents);

				script.ErrorMessage = string.Empty;

				return true;
			}
			catch (ExecutionFailureException ex)
			{
				var sqlException = ex.InnerException as SqlException;
				if (sqlException != null)
				{
					script.ErrorMessage = string.Format("At line {0}:\n{1}", sqlException.LineNumber, sqlException.Message);
				}
				else if (ex.InnerException != null)
				{
					script.ErrorMessage = ex.InnerException.Message;
				}
				else
					script.ErrorMessage = ex.Message;
			}
			catch (Exception ex)
			{
				script.ErrorMessage = ex.Message;
			}

			return false;
		}
	}
}
