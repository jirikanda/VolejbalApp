using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace KandaEu.Volejbal.Contracts
{
	public class GrpcServiceBinder : ServiceBinder
	{
		// voláno jen pro zaregistrované služby
		public override bool IsServiceContract(Type contractType, out string name)
		{
			// name - zde použito bez namespace
			string resultName = (contractType.IsInterface && contractType.Name.StartsWith("I"))
				? contractType.Name.Substring(1)
				: contractType.Name;

			if (resultName.EndsWith("Facade"))
			{
				resultName = resultName.Substring(0, resultName.Length - "Facade".Length);
			}

			name = resultName;
			return true; 
		}
	}
}
