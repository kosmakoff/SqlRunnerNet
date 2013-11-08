using System.ComponentModel;

namespace SqlServerRunnerNet.Infrastructure
{
	public enum AuthenticationType
	{
		[Description("Windows Authentication")]
		WindowsAuthentication,
		[Description("SQL Server Authentication")]
		SqlServerAuthentication
	}
}
