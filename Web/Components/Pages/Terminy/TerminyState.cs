﻿using KandaEu.Volejbal.Contracts.Terminy.Dto;

namespace KandaEu.Volejbal.Web.Components.Pages.Terminy;

public class TerminyState
{
	public List<TerminDto> Terminy { get; set; }
	public int? CurrentTerminId { get; set; }
}
