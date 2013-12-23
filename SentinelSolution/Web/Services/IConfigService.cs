﻿namespace Sentinel.Web.Services
{
	using System.Diagnostics.Contracts;


	/// <summary>
	/// Contract for service components which encapsulate the configuration settings.
	/// </summary>
	[ContractClass( typeof( ConfigServiceContract ) )]
	public interface IConfigService
	{
		/// <summary>
		/// Gets the application realtive virtual path of the folder which contains the galleries.
		/// </summary>
		string StorageFolderVirtualPath { get; }
	}
}
