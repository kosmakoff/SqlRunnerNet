using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerRunnerNet.Infrastructure.Commands
{
	public interface IRaiseCanExecuteChanged
	{
		void RaiseCanExecuteChanged();
	}

	// And an extension method to make it easy to raise changed events
}
