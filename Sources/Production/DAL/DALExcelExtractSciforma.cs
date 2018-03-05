using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Web.WebPages;
using System.Xml;
using RoadmapViewerModel;
using static System.Web.Configuration.WebConfigurationManager;

namespace RoadmapViewer.DAL
{
	public class DalExcelExtractSciforma
	{
		private readonly Roadmap _roadmap;

		private enum ColonnesFeuilleJalons
		{
			Version = 1,
			Regroup = 2,
			Code = 3,
			Nom = 4,
			ChargeFinaleIHM = 5,
			J8 = 11
		}

		private CorrespondancesNomProduitDansExcelNomAffiche _nomsProduits;

		private string _cheminDesFichiersDeParametrage;
		private string _nomOngletDansFichierExcel;

		public DalExcelExtractSciforma(string cheminFichierParametrageDataSource)
		{
			// La roadmap est affichée 3 mois avant et 1 an après la date du jour
			_roadmap = new Roadmap("Tiama", DateTime.Today.AddMonths(-3), DateTime.Today.AddYears(2));

			_cheminDesFichiersDeParametrage = Path.GetDirectoryName(cheminFichierParametrageDataSource);
			_nomOngletDansFichierExcel = OpenWebConfiguration("~").AppSettings.Settings["NomOngletDansFichierExcel"].Value;

			InitialiserCorrespondanceNomProduitsDansExcelEtValeurAffichee();

			var xmlDocument = new XmlDocument();
			xmlDocument.Load(cheminFichierParametrageDataSource);
			var noeudsFichierSource = xmlDocument.SelectNodes("//fichierSource");

			if (noeudsFichierSource == null) return;

			foreach (XmlNode noeudFichierSource in noeudsFichierSource)
			{
				if (noeudFichierSource.Attributes == null) continue;

				var produits = ChargerProduits(noeudFichierSource.Attributes["path"].Value);
				foreach (var produit in produits)
				{
					_roadmap.Produits.Add(produit);
				}
			}


		}

		private void InitialiserCorrespondanceNomProduitsDansExcelEtValeurAffichee()
		{
			_nomsProduits = new CorrespondancesNomProduitDansExcelNomAffiche();

			var xmlDocument = new XmlDocument();
			xmlDocument.Load(Path.Combine(_cheminDesFichiersDeParametrage, "NomsProduits.xml"));
			var noeudsProduit = xmlDocument.SelectNodes("//produit");

			if (noeudsProduit == null) return;

			foreach (XmlNode noeudProduit in noeudsProduit)
			{
				if (noeudProduit.Attributes == null) continue;

				if (!_nomsProduits.ContainsKey(noeudProduit.Attributes["codeLiasse"].Value))
					_nomsProduits.Add(noeudProduit.Attributes["codeLiasse"].Value, noeudProduit.Attributes["produit"].Value);
			}
		}

		public Roadmap Roadmap => _roadmap;

		private OleDbConnection _oledbConnection;

		private List<Produit> ChargerProduits(string cheminFichierExcel)
		{
			//Création d'un "listener" texte pour sortie dans un fichier texte
			Trace.Listeners.Add(new TextWriterTraceListener(@"c:\temp\WSdebug.log"));
			//On écrit directement, pas de temporisation.
			Trace.AutoFlush = true;
			try
			{
				InitialiserConnectionFichierSource(cheminFichierExcel);
				var produits = ChargerLesDonnees();
				return produits;
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.ToString());
				return null;
			}
			finally
			{
				_oledbConnection?.Close();
			}
			return null;
		}

		private List<Produit> ChargerLesDonnees()
		{
			List<Produit> produits = new List<Produit>();

			var cmd = new OleDbCommand
			{
				Connection = _oledbConnection,
				CommandType = CommandType.Text,
				CommandText = $"SELECT * FROM [{_nomOngletDansFichierExcel}$]"
			};

			var table = new DataTable();
			var oleda = new OleDbDataAdapter(cmd);
			oleda.Fill(table);

			foreach (DataRow ligne in table.Rows)
			{
				// Si pas de version cible alors on considère que ça ne doit pas apparaitre dans la roadmap
				if (ligne.ItemArray[ColonnesFeuilleJalons.Version.GetHashCode()].ToString().Length == 0) continue;

				var produit = RecupererLeProduitPourLaLigne(ligne, produits);
				if (produit == null) continue;

				var j8 = RecupererJ8DeLaLigne(ligne);
				var version = RecupererLaVersionDeLaLigne(ligne, produit, j8);


				if (produit.Versions.FirstOrDefault(v => v.NomDeCode.Equals(version.NomDeCode)) == null)
					produit.Versions.Add(version);

				var sujetDansLeFichier = ligne.ItemArray[ColonnesFeuilleJalons.Code.GetHashCode()].ToString() + " " +
										 ligne.ItemArray[ColonnesFeuilleJalons.Nom.GetHashCode()].ToString();

				var versionDeRealisation = version.NomCommercial.IsEmpty() ? null : version;

				var sujet = new Sujet(sujetDansLeFichier,
							Double.Parse(ligne.ItemArray[ColonnesFeuilleJalons.ChargeFinaleIHM.GetHashCode()].ToString()),
							version,
							versionDeRealisation);

				produit.Backlog.Add(sujet);

				if (!produits.Contains(produit))
					produits.Add(produit);
			}
			return produits;
		}

		private void MettreAJourDatesDeSortie(List<Produit> produits)
		{
			// pour tous les produits
			foreach (var produit in produits)
			{
				Dictionary<VersionDuProduit, DateTime> datesDeSortieAttendues = new Dictionary<VersionDuProduit, DateTime>();

				// on parcourt les sujets
				foreach (var sujet in produit.Backlog)
				{
					// recherche de la version
					var version = datesDeSortieAttendues.Keys.FirstOrDefault(v => v.NomDeCode.Equals(sujet.VersionCible.NomDeCode));

					// on conserve dans un dictionnaire la date max de tous les sujets pour une version
					if (version == null)
						datesDeSortieAttendues.Add(sujet.VersionCible, sujet.VersionCible.DateDeSortieAttendue);
					else
					{
						datesDeSortieAttendues[version] = new DateTime(Math.Max(sujet.VersionCible.DateDeSortieAttendue.Ticks,
							datesDeSortieAttendues[version].Ticks));
					}
				}

				// mise à jour de la date attendue des versions pour le produit
				foreach (var versionASortir in datesDeSortieAttendues.Keys)
				{
					// recherche dans la liste des versions du produit la version stockée dans le dico
					var version = produit.Versions.FirstOrDefault(v => v.NomDeCode.Equals(versionASortir.NomDeCode));

					if (version == null) continue;

					// mise à jour de la date avec celle stockée dans le dico
					version.DateDeSortieAttendue = datesDeSortieAttendues[version];
					if (version.NomCommercial.Length > 0)
						version.DateDeSortieRelle = datesDeSortieAttendues[version];
				}
			}
		}

		private string RecupererJ8DeLaLigne(DataRow ligne)
		{
			// Récupération du J8 attendu
			var j8 = ligne.ItemArray[ColonnesFeuilleJalons.J8.GetHashCode()].ToString();
			if (j8.IsEmpty())
				j8 = DateTime.Now.AddMonths(22).ToString("G");//      e.MaxValue.ToString("G");
			return j8;
		}

		private VersionDuProduit RecupererLaVersionDeLaLigne(DataRow ligne, Produit produit, string j8)
		{
			// récupération de la version associée au produit
			var versionCible = ligne.ItemArray[ColonnesFeuilleJalons.Version.GetHashCode()].ToString();

			var versionCommerciale = string.Empty;
			// si la version comporte des chiffres c'est que c'est une version commerciale
			if (versionCible.Any(char.IsDigit))
			{
				// Dans ce cas la version est de ce type 3.0(A)
				// Parse le code pour conserver le numero commerciale (3.0) et le code de la version cible (A)
				string[] resultat = versionCible.Split('(');
				versionCommerciale = resultat[0];
				// supprime la parenthèse fermante
				versionCible = resultat[1].Remove(resultat[1].Length - 1, 1);
			}

			var version = produit.Versions.FirstOrDefault(v => v.NomDeCode.Equals(produit.Nom + " " + versionCible));

			// La version cible n'existe pas encore pour le produit
			if (version == null)
			{
				if (!versionCommerciale.IsEmpty())
					version = new VersionDuProduit(produit.Nom + " " + versionCible, DateTime.Parse(j8))
					{
						NomCommercial = produit.Nom + " " + versionCommerciale,
						DateDeSortieRelle = DateTime.Parse(j8)
					};
				else
				{
					version = new VersionDuProduit(produit.Nom + " " + versionCible, DateTime.Parse(j8));
				}
			}

			// si on n'est pas sur une version dont le J8 n'a pas été renseigné
			if (!version.DateDeSortieAttendue.Year.Equals(DateTime.Now.AddYears(2).Year))
			{
				// Si J8 parsé est plus ancien que le J8 de la version, J8 version devient J8 parsé
				if (DateTime.Compare(DateTime.Parse(j8), version.DateDeSortieAttendue) > 0 && !DateTime.Parse(j8).Year.Equals(DateTime.Now.AddYears(2).Year))
				{
					version.DateDeSortieAttendue = DateTime.Parse(j8);
					if (!versionCommerciale.IsEmpty())
						version.DateDeSortieRelle = DateTime.Parse(j8);
				}
			}
			else
			{
					// Le J8 de la version n'avait pas été renseigné, on le met à jour avec le nouveau J8
					version.DateDeSortieAttendue = DateTime.Parse(j8);
					if (!versionCommerciale.IsEmpty())
						version.DateDeSortieRelle = DateTime.Parse(j8);
			}
			return version;
		}

		private Produit RecupererLeProduitPourLaLigne(DataRow ligne, List<Produit> produits)
		{
			Produit produit = null;

			// Recherche du produit pour la ligne 
			foreach (var nomProduit in _nomsProduits)
			{
				var codeSujet = ligne.ItemArray[ColonnesFeuilleJalons.Code.GetHashCode()].ToString();
				string[] sujets = codeSujet.Split('.');
				if (sujets[0].Contains(nomProduit.Key))
				{
					// On cherche le produit dans la liste et on le créé si il n'existe pas encore avec comme valeur 
					// le champ valeurAffichee du fichier xml nomsProduits
					produit = produits.Find(p => p.Nom.Equals(nomProduit.Value)) ?? new Produit(nomProduit.Value, new List<VersionDuProduit>(), new List<Sujet>());
				}
			}
			return produit;
		}

		private void InitialiserConnectionFichierSource(string cheminFichierExcel)
		{

			if (Path.GetExtension(cheminFichierExcel) == ".xls")
				_oledbConnection = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source = " + cheminFichierExcel + ";Extended Properties =\"Excel 8.0;HDR=Yes;IMEX=2\"");
			else if (Path.GetExtension(cheminFichierExcel) == ".xlsx")
				_oledbConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + cheminFichierExcel + ";Extended Properties = 'Excel 12.0;HDR=YES;IMEX=1;';");

		    _oledbConnection?.Open();


		}

	}

	internal class CorrespondancesNomProduitDansExcelNomAffiche : Dictionary<string, string>
	{
		public new void Add(string nomProduit, string nomAAfficher)
		{
			base.Add(nomProduit, nomAAfficher);
		}

		public new string this[string nomProduit]
		{
			get { return base[nomProduit]; }
			set { base[nomProduit] = value; }
		}
	}
}