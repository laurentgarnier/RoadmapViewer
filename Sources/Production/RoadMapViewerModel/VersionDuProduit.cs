using System;

namespace RoadmapViewerModel
{
	public class VersionDuProduit
	{
		private readonly string _nomDeCode;
	//	private readonly DateTime _dateDeSortieAttendue;

		public VersionDuProduit(string nomDeCode, DateTime dateDeSortieAttendue)//, ICollection<Sujet> listeDeSujets)
		{
			_nomDeCode = nomDeCode;
			DateDeSortieAttendue = dateDeSortieAttendue;
		}

		public string NomDeCode => _nomDeCode;

		public DateTime DateDeSortieAttendue { get; set; }

		public DateTime DateDeSortieRelle { get; set; }

		public string NomCommercial { get; set; } = string.Empty;
	}
}