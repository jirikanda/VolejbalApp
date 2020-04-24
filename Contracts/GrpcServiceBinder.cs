using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KandaEu.Volejbal.Contracts
{
	public class GrpcServiceBinder : ServiceBinder
	{
		public override bool IsServiceContract(Type contractType, out string name)
		{
			// voláno jen pro zaregistrované služby
			name = GetDefaultName(contractType);
			return true; 
		}
	}
}
