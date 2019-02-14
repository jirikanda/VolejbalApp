﻿using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Registration.Lifestyle;

namespace KandaEu.Volejbal.WindsorInstallers
{
	internal class InstallConfiguration
	{
		public string DatabaseConnectionString { get; set; }
		public Func<LifestyleGroup<object>, ComponentRegistration<object>> ScopedLifestyle { get; set; }
		public string[] ServiceProfiles { get; set; }
	}
}