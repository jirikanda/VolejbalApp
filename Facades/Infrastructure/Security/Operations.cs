﻿using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Havit.NewProjectTemplate.Facades.Infrastructure.Security
{
    /// <summary>
    /// Aplikační requirements pro ověření oprávnění k resource.
    /// </summary>
    public static class Operations
    {
		/// <summary>
		/// Práva pro čtení.
		/// </summary>
        public static readonly OperationAuthorizationRequirement Read = new OperationAuthorizationRequirement { Name = nameof(Read) };

		/// <summary>
		/// Requirement na administraci systému.
		/// Toto je taková zjednodušená úlitba, spíš bychom měli mít oprávnění na úrovni s větší granularitou (seedovat data, importovat data, ...), 
		/// ale pro praktické zjednodušení administrativní záležitosti shlukujeme pod administraci systému.
		/// </summary>
		public static readonly OperationAuthorizationRequirement SystemAdministration = new OperationAuthorizationRequirement { Name = nameof(SystemAdministration) };
    }
}
