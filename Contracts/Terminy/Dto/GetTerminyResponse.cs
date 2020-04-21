﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[DataContract]
	public class GetTerminyResponse
	{
		[DataMember(Order = 1)]
		public List<TerminDto> Terminy { get; set; } = new List<TerminDto>();
	}
}