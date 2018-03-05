namespace RoadmapViewerModel
{
	public class Sujet
	{
		private readonly string _libelle;
		private readonly double _chargeEstimee;
		private readonly VersionDuProduit _versionCible;
		private readonly VersionDuProduit _versionDeRealisation;

		public Sujet(string libelle, double chargeEstimee, VersionDuProduit versionCible, VersionDuProduit versionDeRealisation)
		{
			_libelle = libelle;
			_chargeEstimee = chargeEstimee;
			_versionCible = versionCible;
			_versionDeRealisation = versionDeRealisation;
		}

		public string Libelle => _libelle;

		public double ChargeEstimee => _chargeEstimee;

		public VersionDuProduit VersionCible => _versionCible;

		public VersionDuProduit VersionDeRealisation => _versionDeRealisation;
	}
}