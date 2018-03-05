using System;
using System.Collections.Generic;

namespace RoadmapViewerModel
{
	public class Roadmap
	{
		private readonly string _nom;
		private readonly ICollection<Produit> _produits;
		private readonly DateTime _dateDebut;
		private readonly DateTime _dateFin;

		public Roadmap(string nom, DateTime dateDebut, DateTime dateFin)
		{
			_nom = nom;
			_dateDebut = dateDebut;
			_dateFin = dateFin;
			_produits = new List<Produit>();
		}

		public string Nom => _nom;

		public ICollection<Produit> Produits => _produits;

		public DateTime DateDebut => _dateDebut;

		public DateTime DateFin => _dateFin;
	}
}