using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SqlServerRunnerNet.Utils
{
	public static class SqlServerEnumeration
	{
		private static Regex PatternRegex = new Regex(@"^ServerName;(?<server>\w+);InstanceName;(?<instance>\w+);.*", RegexOptions.Compiled);

		public static List<SqlServerInstance> EnumLocalInstances()
		{
			var retVal = new List<SqlServerInstance>();

			var registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
			using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
			{
				var instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
				if (instanceKey != null)
				{
					retVal.AddRange(instanceKey.GetValueNames().Select(SqlServerInstance.CreateLocal));
				}
			}

			return retVal;
		}

		public static List<SqlServerInstance> EnumRemoteInstances()
		{
			var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)
			{
				EnableBroadcast = true,
				ReceiveTimeout = 1000,
				DualMode = true
			};

			var bytes = new List<byte>(4096);

			try
			{
				var ipv4ep = new IPEndPoint(IPAddress.Broadcast, 1434);
				var ipv6ep = new IPEndPoint(IPAddress.Parse("ff02::1"), 1434);
				byte[] msg = { 0x02 };
				socket.SendTo(msg, ipv4ep);
				socket.SendTo(msg, ipv6ep);

				int cnt = 0;
				byte[] byteBuffer = new byte[256];

				do
				{
					cnt = socket.Receive(byteBuffer);

					bytes.AddRange(byteBuffer.Take(cnt));

				} while (cnt != 0);
			}
			catch (SocketException sex)
			{
				const int WSAETIMEDOUT = 10060; // Connection timed out. 
				const int WSAEHOSTUNREACH = 10065; // No route to host. 

				if (sex.ErrorCode == WSAETIMEDOUT || sex.ErrorCode == WSAEHOSTUNREACH)
				{
				}
				else
				{
					throw;
				}
			}
			finally
			{
				socket.Close();
			}

			string text = Encoding.ASCII.GetString(bytes.ToArray());

			var servers = text.Split(new[] { ";;" }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Substring(s.IndexOf("ServerName", StringComparison.Ordinal)));

			return servers
				.Select(server => PatternRegex.Match(server))
				.Select(match => SqlServerInstance.Create(match.Groups["server"].Value, match.Groups["instance"].Value))
				.Distinct(SqlServerInstance.Comparer)
				.Where(instance => !instance.IsLocal)
				.OrderBy(instance => instance.ServerName)
				.ThenBy(instance => instance.InstanceName)
				.ToList();
		}
	}

	public class SqlServerInstance
	{
		public string ServerName { get; private set; }
		public string InstanceName { get; private set; }
		public bool IsLocal { get; private set; }

		private SqlServerInstance()
		{
		}

		public static SqlServerInstance Create(string serverName, string instanceName)
		{
			var localComputerName = Environment.MachineName;

			return new SqlServerInstance
			{
				ServerName = serverName.ToUpper(),
				InstanceName = instanceName.ToUpper(),
				IsLocal = string.Compare(serverName, localComputerName, StringComparison.OrdinalIgnoreCase) == 0
			};
		}

		public static SqlServerInstance CreateLocal(string instanceName)
		{
			var localComputerName = Environment.MachineName;

			return new SqlServerInstance
			{
				ServerName = localComputerName,
				InstanceName = instanceName.ToUpper(),
				IsLocal = true
			};
		}

		public override string ToString()
		{
			return string.Format(@"{0}\{1}", ServerName, InstanceName);
		}

		private sealed class ServerNameInstanceNameIsLocalEqualityComparer : IEqualityComparer<SqlServerInstance>
		{
			public bool Equals(SqlServerInstance x, SqlServerInstance y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null)) return false;
				if (ReferenceEquals(y, null)) return false;
				if (x.GetType() != y.GetType()) return false;
				return string.Equals(x.ServerName, y.ServerName) && string.Equals(x.InstanceName, y.InstanceName) && x.IsLocal.Equals(y.IsLocal);
			}

			public int GetHashCode(SqlServerInstance obj)
			{
				unchecked
				{
					int hashCode = (obj.ServerName != null ? obj.ServerName.GetHashCode() : 0);
					hashCode = (hashCode*397) ^ (obj.InstanceName != null ? obj.InstanceName.GetHashCode() : 0);
					hashCode = (hashCode*397) ^ obj.IsLocal.GetHashCode();
					return hashCode;
				}
			}
		}

		private static readonly IEqualityComparer<SqlServerInstance> ServerNameInstanceNameIsLocalComparerInstance = new ServerNameInstanceNameIsLocalEqualityComparer();

		public static IEqualityComparer<SqlServerInstance> Comparer
		{
			get { return ServerNameInstanceNameIsLocalComparerInstance; }
		}
	}
}
