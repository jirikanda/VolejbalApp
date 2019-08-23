﻿using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Blazor.StateManagement.Prihlasovani
{
	public class TerminyState
	{
		public bool IsLoading { get; set; }
		public bool LoadingFailed { get; set; }
		public List<TerminDto> Terminy { get; set; }
	}
}
