﻿namespace Sentinel.Web.Services
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Web;
	using Sentinel.Web.Models.Gallery;

	using Ninject;


	/// <summary>
	/// Service component which encapsulates the details of the image file operations.
	/// </summary>
	public class ImageService : IImageService
	{
		[Inject]
		public IConfigService ConfigService { get; set; }


		/// <summary>
		/// Gets the list of the available galleries.
		/// </summary>
		/// <returns>The list of galleries.</returns>
		public ViewGalleriesVM GetGalleries()
		{
			ViewGalleriesVM model = new ViewGalleriesVM
			{
				Galleries = new List<ViewGallerySummaryVM>()
			};

			// Get the full physical path of the folder which contains the galleries (eg. C:\inetpub\wwwroot\Photos).
			string storageFolderPhysicalPath = HttpContext.Current.Server.MapPath( this.ConfigService.StorageFolderVirtualPath );

			// Get the full physical paths of each galleries (eg. list of C:\inetpub\wwwroot\Photos\First, C:\inetpub\wwwroot\Photos\Second etc.).
			IOrderedEnumerable<string> folderPhysicalPaths = Directory.GetDirectories( storageFolderPhysicalPath ).OrderByDescending( p => p );

			// Enumerate the galleries.
			foreach( string folderPhysicalPath in folderPhysicalPaths )
			{
				// Get the full physical paths of the image files in the gallery (eg. list of C:\inetpub\wwwroot\Photos\First\img1.jpg, C:\inetpub\wwwroot\Photos\First\img2.jpg etc.).
				string[] filePhysicalPaths = Directory.GetFiles( folderPhysicalPath, "*.*" )
					.Where( p => p.EndsWith( ".jpg", StringComparison.OrdinalIgnoreCase ) || p.EndsWith( ".png", StringComparison.OrdinalIgnoreCase ) )
					.ToArray();

				// If there is at least one image in the gallery.
				if( filePhysicalPaths.Length > 0 )
				{
					// Get the name of the gallery folder (eg. First).
					string folderName = Path.GetFileName( folderPhysicalPath );

					// Get the full physical path of the gallery thumbnail image (eg. C:\inetpub\wwwroot\First\folder.jpg).
					string thumbnailPhysicalPath = Path.Combine( folderPhysicalPath, "folder.jpg" );

					// If the thumbnail file exists, use it, otherwise use the first image as the thumbnail image of the gallery.
					thumbnailPhysicalPath = filePhysicalPaths.Contains( thumbnailPhysicalPath )
						? thumbnailPhysicalPath
						: filePhysicalPaths[ 0 ];

					// Get the file name of the thumbnail (eg. folder.jpg or img1.jpg).
					string thumbnailFileName = Path.GetFileName( thumbnailPhysicalPath );

					// Get the application relative virtual path of the gallery thumbnail image (eg. ~/Photos/First/img1.jpg);
					string thumbnailVirtualPath = Path.Combine( Path.Combine( this.ConfigService.StorageFolderVirtualPath, folderName ), thumbnailFileName );

					// Build the returned model item for the gallery.
					ViewGallerySummaryVM gallery = new ViewGallerySummaryVM
					{
						FolderName = folderName,
						Count = filePhysicalPaths.Length,
						ThumbnailUrl = thumbnailVirtualPath
					};
					model.Galleries.Add( gallery );
				}
			}

			return model;
		}


		/// <summary>
		/// Gets the details of a gallery.
		/// </summary>
		/// <param name="folderName">The name of the folder which contains the gallery.</param>
		/// <returns>The details of the specified gallery.</returns>
		public ViewGalleryVM GetGallery( string folderName )
		{
			// Input validation: the folder name must be specified.
			if( String.IsNullOrEmpty( folderName ) )
			{
				throw new ArgumentNullException( "folderName" );
			}

			// Input validation: the folder name must not contain any invalid character.
			if( folderName.IndexOfAny( Path.GetInvalidPathChars() ) > -1 || folderName.IndexOfAny( Path.GetInvalidFileNameChars() ) > -1 )
			{
				throw new ArgumentException( "The folder name contains invalid characters!", "folderName" );
			}

			// Get the application relative virtual path of the gallery folder (eg. ~/Photos/First).
			string folderVirtualPath = Path.Combine( this.ConfigService.StorageFolderVirtualPath, folderName );

			// Get the full physical path of the gallery folder (eg. C:\inetpub\wwwroot\Photos\First).
			string folderPhysicalPath = HttpContext.Current.Server.MapPath( folderVirtualPath );

			// Integrity check: the folder must exist.
			if( !Directory.Exists( folderPhysicalPath ) )
			{
				throw new ArgumentException( "The specified folder does not exist!", "folderName" );
			}

			// Integrity check: the requested folder must be the direct child of the storage folder.
			string storagePhysicalPath = HttpContext.Current.Server.MapPath( this.ConfigService.StorageFolderVirtualPath );
			string folderParentPhysicalPath = Directory.GetParent( folderPhysicalPath ).FullName;
			if( !folderParentPhysicalPath.Equals( storagePhysicalPath, StringComparison.OrdinalIgnoreCase ) )
			{
				throw new ArgumentOutOfRangeException( "folderName", "Invalid folder name!" );
			}

			// Enumerate the image files in the requested folder (except the thumbnail image).
			List<string> photoUrls = Directory.GetFiles( folderPhysicalPath, "*.*" )
				.Where( p => ( p.EndsWith( ".jpg", StringComparison.OrdinalIgnoreCase ) || p.EndsWith( ".png", StringComparison.OrdinalIgnoreCase ) ) && !p.EndsWith( "folder.jpg", StringComparison.OrdinalIgnoreCase ) )
				.Select( filePhysicalPath => Path.Combine( folderVirtualPath, Path.GetFileName( filePhysicalPath ) ) )
				.ToList();

			// Build the returned model.
			ViewGalleryVM model = new ViewGalleryVM
			{
				Title = folderName,
				PhotoUrls = photoUrls
			};

			return model;
		}
	}
}