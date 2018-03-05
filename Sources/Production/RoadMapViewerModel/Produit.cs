using System.Collections.Generic;
using System.Linq;

namespace RoadmapViewerModel
{
	public class Produit
	{
		private readonly ICollection<VersionDuProduit> _versions;
		private ICollection<VersionDuProduit> _versionsSorties;
		private readonly string _nom;
		private readonly ICollection<Sujet> _backlog;

		public Produit(string nom, ICollection<VersionDuProduit> versions, ICollection<Sujet> backlog)
		{
			_nom = nom;
			_versions = versions;
			_backlog = backlog;
		}

		public Produit(ICollection<VersionDuProduit> versions, ICollection<Sujet> backlog)
		{
			_nom = "Unknown";
			_versions = versions;
			_backlog = backlog;
		}


		public string Nom => _nom;

		public ICollection<VersionDuProduit> Versions => _versions;

		public ICollection<VersionDuProduit> VersionsSorties
		{
			get
			{
				if (_versionsSorties == null)
				{
					_versionsSorties = new List<VersionDuProduit>();
					foreach (var version in _versions)
					{
						if(version.NomCommercial.Length > 0)
							_versionsSorties.Add(version);
					}
				}
				return _versionsSorties;
			}
		}

		public ICollection<Sujet> Backlog => _backlog;
	}
}