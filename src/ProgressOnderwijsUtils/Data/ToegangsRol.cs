// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Linq;
using ProgressOnderwijsUtils.ToegangsRolInternal;

namespace ProgressOnderwijsUtils
{
	public enum ToegangsRol
	{

		[MpLabel("Studentadministratie voorlopige toelating")]
		[RolSoort.Oud, ImpliedBy(Studentadministratie)]
		StudentadministratieVoorlopigeToelating = -27,

		[MpLabel("Studentadministratie balie (alleen studenttabblad)")]
		[RolSoort.Oud, ImpliedBy(StudentadministratieAlleenInkijken)]
		StudentadministratieBalieAlleenStudenttabblad = -26,

		[MpLabel("Uitsluitend adreswijzigingen, te combineren met andere rollen")]
		[RolSoort.Oud, ImpliedBy(Studentadministratie)]
		UitsluitendAdreswijzigingen, TeCombinerenMetAndereRollen = -25,

		[MpLabel("Studentadministratie alleen lezen, met accorderen toelatingseisen en kenmerken")]
		[RolSoort.Oud, ImpliedBy(Studentadministratie)]
		StudentadministratieAlleenLezen, MetAccorderenToelatingseisenEnKenmerken = -24,

		[MpLabel("Financiële administratie alleen inkijken")]
		[RolSoort.Oud, ImpliedBy(FinanciëleAdministratie)]
		FinanciëleAdministratieAlleenInkijken = -23,

		[MpLabel("Financiële administratie")]
		[RolSoort.Oud, ImpliedBy(StudentadministratieBeheerder)]
		FinanciëleAdministratie = -22,

		[MpLabel("Studentadministratie alleen inkijken")]
		[RolSoort.Oud, ImpliedBy(StudentadministratieAlleenLezen, MetAccorderenToelatingseisenEnKenmerken)]
		StudentadministratieAlleenInkijken = -21,

		[MpLabel("Studentadministratie niet-reguliere inschrijvingen")]
		[RolSoort.Oud, ImpliedBy(Studentadministratie)]
		StudentadministratieNietReguliereInschrijvingen = -20,

		[MpLabel("Studentadministratie")]
		[RolSoort.Oud, ImpliedBy(StudentadministratieExtra)]
		Studentadministratie = -18,

		[MpLabel("Studentadministratie beheerder")]
		[RolSoort.Oud]
		StudentadministratieBeheerder = -5,

		[MpLabel("Studentadministratie extra")]
		[RolSoort.Oud, ImpliedBy(StudentadministratieBeheerder)]
		StudentadministratieExtra = -4,

		[MpLabel("Bekijken wijzigen verwijderen alle tabellen")]
		[RolSoort.Recht, ImpliedBy(Superuser)]
		BekijkenWijzigenVerwijderenAlleTabellen = 1,

		[MpLabel("Student bekijk personalia")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, StudentadministratieNietReguliereInschrijvingen, BekijkenInschrijvingen)]
		StudentBekijkPersonalia = 2,

		[MpLabel("Organisatie toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BeheerInschrijvingen)]
		OrganisatieToevoegenVerwijderenWijzigen = 4,

		[MpLabel("Organisatieboom bekijken selecteren")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieVoorlopigeToelating, StudentadministratieAlleenInkijken, StudentadministratieNietReguliereInschrijvingen, Studentadministratie)]
		OrganisatieboomBekijkenSelecteren = 5,

		[MpLabel("Rapporten kengetallen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieExtra, BekijkenKengetallen)]
		RapportenKengetallen = 30,

		[MpLabel("Student wijzig personalia")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		StudentWijzigPersonalia = 32,

		[MpLabel("Student verwijderen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, BeheerInschrijvingen)]
		StudentVerwijderen = 33,

		[MpLabel("Opleidingen bekijken")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, BeheerInschrijvingen, BeheerGetuigschriften)]
		OpleidingenBekijken = 35,

		[MpLabel("Opleidingen toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BeheerInschrijvingen)]
		OpleidingenToevoegenVerwijderenWijzigen = 36,

		[MpLabel("Accounts toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(BeheerAccountsEnRollen)]
		AccountsToevoegenVerwijderenWijzigen = 37,

		[MpLabel("Student tabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieVoorlopigeToelating, StudentadministratieBalieAlleenStudenttabblad, StudentadministratieNietReguliereInschrijvingen, BekijkenInschrijvingen)]
		StudentTabblad = 38,

		[MpLabel("Bekijken wijzigen verwijderen meta data")]
		[RolSoort.Recht, ImpliedBy(Superuser)]
		BekijkenWijzigenVerwijderenMetaData = 42,

		[MpLabel("Inschrijving basis tabellen bekijken")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, BeheerInschrijvingen)]
		InschrijvingBasisTabellenBekijken = 43,

		[MpLabel("Organisatie beheer tabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BeheerFinancieel, BeheerInschrijvingen, BeheerAccountsEnRollen, BeheerGetuigschriften)]
		OrganisatieBeheerTabblad = 44,

		[MpLabel("Organisatie financieel tabblad")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, StudentadministratieExtra, BekijkenFinancieelUitgebreid)]
		OrganisatieFinancieelTabblad = 45,

		[MpLabel("Student identificatie toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		StudentIdentificatieToevoegenVerwijderenWijzigen = 46,

		[MpLabel("Student identificatie verifieeren")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, StudentadministratieExtra, WijzigenInschrijvingenEnAanmeldingen)]
		StudentIdentificatieVerifieeren = 47,

		[MpLabel("Studentoverleden toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		StudentoverledenToevoegenVerwijderenWijzigen = 48,

		[MpLabel("Student overleden verifieeren")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieExtra, WijzigenInschrijvingenEnAanmeldingen)]
		StudentOverledenVerifieeren = 49,

		[MpLabel("Student pasfoto toevoegen verwijderen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenPasfoto)]
		StudentPasfotoToevoegenVerwijderen = 50,

		[MpLabel("Student persoon tabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, StudentadministratieNietReguliereInschrijvingen, BekijkenInschrijvingen)]
		StudentPersoonTabblad = 51,

		[MpLabel("Inschrijvingen tabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieVoorlopigeToelating, StudentadministratieBalieAlleenStudenttabblad, StudentadministratieNietReguliereInschrijvingen, BekijkenInschrijvingen)]
		InschrijvingenTabblad = 52,

		[MpLabel("Student financieel tabblad")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratieAlleenInkijken, Studentadministratie, BekijkenFinancieel)]
		StudentFinancieelTabblad = 53,

		[MpLabel("Student onderwijs tabblad")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, BekijkenStudievolg)]
		StudentOnderwijsTabblad = 54,

		[MpLabel("Student kenemerken tabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, Studentadministratie, BekijkenInschrijvingenUitgebreid)]
		StudentKenemerkenTabblad = 55,

		[MpLabel("Vooropleidingen toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		VooropleidingenToevoegenVerwijderenWijzigen = 58,

		[MpLabel("Vooropleidingen verifieeren")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, StudentadministratieExtra, WijzigenInschrijvingenEnAanmeldingen)]
		VooropleidingenVerifieeren = 59,

		[MpLabel("Vooropleidingen bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingen)]
		VooropleidingenBekijken = 60,

		[MpLabel("Aanmeldingen bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieVoorlopigeToelating, StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingen)]
		AanmeldingenBekijken = 61,

		[MpLabel("Aanmeldingen toevoegen wijzigen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingenToevoegenWijzigen = 62,

		[MpLabel("Aanmelding definitief inschrijven")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingDefinitiefInschrijven = 63,

		[MpLabel("Aanmelding inschrijving intrekken")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingInschrijvingIntrekken = 64,

		[MpLabel("Aanmelding inschrijving mededelingen bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingenUitgebreid)]
		AanmeldingInschrijvingMededelingenBekijken = 65,

		[MpLabel("Inschrijving uitschrijven")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenUitschrijvingen)]
		InschrijvingUitschrijven = 66,

		[MpLabel("Inschrijving examen toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenExamens)]
		InschrijvingExamenToevoegenVerwijderenWijzigen = 67,

		[MpLabel("Aanmelding inschrijving mededeling versturen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingInschrijvingMededelingVersturen = 68,

		[MpLabel("Negatief binden studieadvies bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingenUitgebreid)]
		NegatiefBindenStudieadviesBekijken = 69,

		[MpLabel("Negatief bindend studieadvies toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieExtra, WijzigenInschrijvingenEnAanmeldingen)]
		NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen = 70,

		[MpLabel("Blokkeer inschrijving toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieExtra, WijzigenInschrijvingenEnAanmeldingen)]
		BlokkeerInschrijvingToevoegenWijzigenVerwijderen = 71,

		[MpLabel("Blokkeer inschrijving bekijken")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, BekijkenInschrijvingenUitgebreid)]
		BlokkeerInschrijvingBekijken = 72,

		[MpLabel("SM mutaties toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BeheerUitwisselingDuo)]
		SMMutatiesToevoegenWijzigenVerwijderen = 73,

		[MpLabel("SM mutaties bekijken")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, BekijkenUitwisselingDuo)]
		SMMutatiesBekijken = 74,

		[MpLabel("Studielinkberichten bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenStudielinkberichten)]
		StudielinkberichtenBekijken = 75,

		[MpLabel("Studielinkberichten beheer")]
		[RolSoort.Recht, ImpliedBy(Superuser, BeheerStudielink)]
		StudielinkberichtenBeheer = 76,

		[MpLabel("Inschrijvingen bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieVoorlopigeToelating, StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingen)]
		InschrijvingenBekijken = 77,

		[MpLabel("Financieel betalingsinformatie bekijken")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratieAlleenInkijken, Studentadministratie, BekijkenFinancieel)]
		FinancieelBetalingsinformatieBekijken = 79,

		[MpLabel("Financieel betalingsinformatie toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, WijzigenFinancieel)]
		FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen = 80,

		[MpLabel("Student identificatie bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingenUitgebreid)]
		StudentIdentificatieBekijken = 81,

		[MpLabel("Financieel clieop toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratie, BeheerFinancieel)]
		FinancieelClieopToevoegenVerwijderenWijzigen = 82,

		[MpLabel("Financieel clieop bekijken")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratie, BeheerFinancieel)]
		FinancieelClieopBekijken = 83,

		[MpLabel("Niet reguliere inschrijvingen bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, StudentadministratieNietReguliereInschrijvingen, BekijkenInschrijvingen)]
		NietReguliereInschrijvingenBekijken = 84,

		[MpLabel("Niet reguliere inschrijvingen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(Studentadministratie, InvoerNietReguliereInschrijvingen)]
		NietReguliereInschrijvingenWijzigenVerwijderen = 85,

		[MpLabel("Financieel betalingen toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratie, WijzigenFinancieel)]
		FinancieelBetalingenToevoegenVerwijderenWijzigen = 86,

		[MpLabel("Financieel collegegeld toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratie, WijzigenFinancieel)]
		FinancieelCollegegeldToevoegenVerwijderenWijzigen = 87,

		[MpLabel("Financieel collegegeld bekijken")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratieAlleenInkijken, BekijkenFinancieel)]
		FinancieelCollegegeldBekijken = 88,

		[MpLabel("Financieel machting toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratie, WijzigenFinancieel)]
		FinancieelMachtingToevoegenVerwijderenWijzigen = 89,

		[MpLabel("Financieel machtiging bekijken")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratieAlleenInkijken, BekijkenFinancieel)]
		FinancieelMachtigingBekijken = 90,

		[MpLabel("Niet reguliere inschrijvingen toevoegen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieNietReguliereInschrijvingen, InvoerNietReguliereInschrijvingen)]
		NietReguliereInschrijvingenToevoegen = 91,

		[MpLabel("Student toevoegen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieNietReguliereInschrijvingen, WijzigenCorrespondentieadressen, InvoerNietReguliereInschrijvingen)]
		StudentToevoegen = 92,

		[MpLabel("Student adres bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, StudentadministratieNietReguliereInschrijvingen, BekijkenInschrijvingen)]
		StudentAdresBekijken = 93,

		[MpLabel("Student adres toevoegen")]
		[RolSoort.Recht, ImpliedBy(UitsluitendAdreswijzigingen, TeCombinerenMetAndereRollen, StudentadministratieNietReguliereInschrijvingen, WijzigenCorrespondentieadressen, InvoerNietReguliereInschrijvingen)]
		StudentAdresToevoegen = 94,

		[MpLabel("Student adres wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(UitsluitendAdreswijzigingen, TeCombinerenMetAndereRollen, Studentadministratie, WijzigenInschrijvingenEnAanmeldingen)]
		StudentAdresWijzigenVerwijderen = 95,

		[MpLabel("Aanmelding toelatingseisen accorderen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieAlleenLezen, MetAccorderenToelatingseisenEnKenmerken, WijzigenAccorderenToelatingseisen)]
		AanmeldingToelatingseisenAccorderen = 96,

		[MpLabel("Student eisen tabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BekijkenInschrijvingenUitgebreid)]
		StudentEisenTabblad = 97,

		[MpLabel("Voorlopige toelating invoeren wijzigin verwijderen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieVoorlopigeToelating, StudentadministratieExtra, WijzigenVoorlopigeToelating)]
		VoorlopigeToelatingInvoerenWijziginVerwijderen = 98,

		[MpLabel("Student kenmerk toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieAlleenLezen, MetAccorderenToelatingseisenEnKenmerken, Studentadministratie, WijzigenInschrijvingenEnAanmeldingen, BeheerStudievolg)]
		StudentKenmerkToevoegenVerwijderenWijzigen = 99,

		[MpLabel("Organisatie financieel collegegeld tabel beheer")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BeheerFinancieel)]
		OrganisatieFinancieelCollegegeldTabelBeheer = 100,

		[MpLabel("Organisatie onderwijs tab")]
		[RolSoort.Recht, ImpliedBy(BekijkenStudievolg)]
		OrganisatieOnderwijsTab = 101,

		[MpLabel("Examenstabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingenUitgebreid)]
		Examenstabblad = 102,

		[MpLabel("Uitschrijvingentabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingenUitgebreid)]
		Uitschrijvingentabblad = 103,

		[MpLabel("Student taaltoetstabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenInschrijvingenUitgebreid)]
		StudentTaaltoetstabblad = 104,

		[MpLabel("Webservice alle tabellen lezen")]
		[RolSoort.Recht]
		WebserviceAlleTabellenLezen = 105,

		[MpLabel("Document templates")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BeheerInschrijvingen, BeheerGetuigschriften)]
		DocumentTemplates = 106,

		[MpLabel("Document generatie")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, WijzigenInschrijvingenEnAanmeldingen, WijzigenToevoegenAfdrukkenBbc, WijzigenCrm)]
		DocumentGeneratie = 107,

		[MpLabel("BBCs bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, BekijkenFinancieel)]
		BbcBekijken = 108,

		[MpLabel("BBCs toevoegen wijzigen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, WijzigenToevoegenAfdrukkenBbc)]
		BbcToevoegenWijzigen = 109,

		[MpLabel("BBCs afdrukken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, WijzigenToevoegenAfdrukkenBbc)]
		BbcAfdrukken = 110,

		[MpLabel("Student communicatie tabblad")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBalieAlleenStudenttabblad, Superuser, BekijkenInschrijvingenUitgebreid, WijzigenCrm)]
		StudentCommunicatieTabblad = 111,

		[MpLabel("Student communicatie toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieAlleenInkijken, StudentadministratieExtra, WijzigenInschrijvingenEnAanmeldingen, WijzigenCrm)]
		StudentCommunicatieToevoegenVerwijderenWijzigen = 112,

		[MpLabel("Communicatie template toevoegen verwijderen wijzigen")]
		[RolSoort.Recht, ImpliedBy(Superuser, BeheerGetuigschriften)]
		CommunicatieTemplateToevoegenVerwijderenWijzigen = 113,

		[MpLabel("Volg onderwijs beheer alles")]
		[RolSoort.Recht, ImpliedBy(BeheerStudievolg)]
		VolgOnderwijsBeheerAlles = 114,

		[MpLabel("Volg onderwijs beheer beperkt")]
		[RolSoort.Recht, ImpliedBy(WijzigenStudievolg)]
		VolgOnderwijsBeheerBeperkt = 115,

		[MpLabel("Volg onderwijs inzien")]
		[RolSoort.Recht, ImpliedBy(BekijkenStudievolg)]
		VolgOnderwijsInzien = 116,

		[MpLabel("Bekijken kengetallen")]
		[RolSoort.Nieuw, ImpliedBy(BeheerInschrijvingen)]
		BekijkenKengetallen = 119,

		[MpLabel("Organisatietabblad en boom")]
		[RolSoort.Recht]
		OrganisatietabbladEnBoom = 120,

		[MpLabel("Bekijken studievolg")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenStudievolg, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding, WijzigenBsaStatus, WijzigenStudentdecaan)]
		BekijkenStudievolg = 121,

		[MpLabel("Bekijken studielinkberichten")]
		[RolSoort.Nieuw, ImpliedBy(BeheerStudielink, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenStudielinkberichten = 123,

		[MpLabel("Bekijken uitwisseling DUO")]
		[RolSoort.Nieuw, ImpliedBy(BeheerUitwisselingDuo)]
		BekijkenUitwisselingDuo = 124,

		[MpLabel("Wijzigen inschrijvingen en aanmeldingen")]
		[RolSoort.Nieuw, ImpliedBy(BeheerInschrijvingen)]
		WijzigenInschrijvingenEnAanmeldingen = 125,

		[MpLabel("Wijzigen uitschrijvingen")]
		[RolSoort.Nieuw, ImpliedBy(BeheerInschrijvingen)]
		WijzigenUitschrijvingen = 126,

		[MpLabel("Wijzigen examens")]
		[RolSoort.Nieuw, ImpliedBy(BeheerInschrijvingen, BeheerWaardepapierenSjablonen)]
		WijzigenExamens = 127,

		[MpLabel("Wijzigen correspondentieadressen")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenCorrespondentieadressen = 128,

		[MpLabel("Invoer niet-reguliere inschrijvingen")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		InvoerNietReguliereInschrijvingen = 129,

		[MpLabel("Wijzigen financieel")]
		[RolSoort.Nieuw, ImpliedBy(BeheerFinancieel)]
		WijzigenFinancieel = 132,

		[MpLabel("Wijzigen/ toevoegen/ afdrukken BBC's")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenFinancieel)]
		WijzigenToevoegenAfdrukkenBbc = 133,

		[MpLabel("Beheer financieel")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerFinancieel = 134,

		[MpLabel("Beheer inschrijvingen")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerInschrijvingen = 135,

		[MpLabel("Wijzigen accorderen toelatingseisen")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenAccorderenToelatingseisen = 136,

		[MpLabel("Wijzigen voorlopige toelating")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenVoorlopigeToelating = 137,

		[MpLabel("Superuser")]
		[RolSoort.Nieuw]
		Superuser = 138,

		[MpLabel("Wijzigen studievolg")]
		[RolSoort.Nieuw, ImpliedBy(BeheerStudievolg)]
		WijzigenStudievolg = 139,

		[MpLabel("Beheer studievolg")]
		[RolSoort.Nieuw, ImpliedBy(Superuser)]
		BeheerStudievolg = 140,

		[MpLabel("Bekijken inschrijvingen")]
		[RolSoort.Nieuw, ImpliedBy(BekijkenStudievolg, BekijkenStudielinkberichten, BekijkenUitwisselingDuo, WijzigenCorrespondentieadressen, InvoerNietReguliereInschrijvingen, WijzigenAccorderenToelatingseisen, WijzigenVoorlopigeToelating, BekijkenInschrijvingenUitgebreid, BekijkenFinancieel, WijzigenAlumniNetwerk)]
		BekijkenInschrijvingen = 160,

		[MpLabel("Bekijken inschrijvingen uitgebreid")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen, WijzigenUitschrijvingen, WijzigenExamens, WijzigenStudentdecaan, WijzigenVerblijfsvergunning, WijzigenPasfoto, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenInschrijvingenUitgebreid = 161,

		[MpLabel("Bekijken financieel")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenToevoegenAfdrukkenBbc, BekijkenFinancieelUitgebreid)]
		BekijkenFinancieel = 162,

		[MpLabel("Bekijken financieel uitgebreid")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenFinancieel, WijzigenStudentdecaan, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenFinancieelUitgebreid = 163,

		[MpLabel("Beheer accounts en rollen")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerAccountsEnRollen = 165,

		[MpLabel("Beheer studielink")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerStudielink = 166,

		[MpLabel("Beheer uitwisseling DUO")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerUitwisselingDuo = 167,

		[MpLabel("Wijzigen CRM")]
		[RolSoort.Nieuw, ImpliedBy(BeheerInschrijvingen, WijzigenStudievolg, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		WijzigenCrm = 168,

		[MpLabel("Taken bekijken wijzigen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieBeheerder, BeheerInschrijvingen)]
		TakenBekijkenWijzigen = 169,

		[MpLabel("Wijzigen BSA bijzondere omstandigheden/ studiebegeleiding")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenStudievolg, BeheerBsaStudiebegeleiding)]
		WijzigenBsaBijzondereOmstandighedenStudiebegeleiding = 172,

		[MpLabel("Student communicatie inschrijving toevoegen")]
		[RolSoort.Recht, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentCommunicatieInschrijvingToevoegen = 173,

		[MpLabel("Inschrijving notities bekijken")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieAlleenInkijken, BekijkenInschrijvingenUitgebreid)]
		InschrijvingNotitiesBekijken = 174,

		[MpLabel("Inschrijving notities toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(StudentadministratieExtra, WijzigenInschrijvingenEnAanmeldingen, WijzigenUitschrijvingen, WijzigenExamens)]
		InschrijvingNotitiesToevoegenWijzigenVerwijderen = 175,

		[MpLabel("Financieel notities bekijken")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratieAlleenInkijken, BekijkenFinancieelUitgebreid)]
		FinancieelNotitiesBekijken = 176,

		[MpLabel("Financieel notities toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(FinanciëleAdministratie, WijzigenFinancieel)]
		FinancieelNotitiesToevoegenWijzigenVerwijderen = 177,

		[MpLabel("Studiebegeleiding gespreksnotities bekijken")]
		[RolSoort.Recht, ImpliedBy(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		StudiebegeleidingGespreksnotitiesBekijken = 178,

		[MpLabel("Studiebegeleiding gespreksnotities toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		StudiebegeleidingGespreksnotitiesToevoegenWijzigenVerwijderen = 179,

		[MpLabel("Studievoortgang notities bekijken")]
		[RolSoort.Recht, ImpliedBy(WijzigenStudievolg, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		StudievoortgangNotitiesBekijken = 180,

		[MpLabel("Studievoortgang notities toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenStudievolg)]
		StudievoortgangNotitiesToevoegenWijzigenVerwijderen = 181,

		[MpLabel("BSA status bekijken")]
		[RolSoort.Recht, ImpliedBy(BekijkenStudievolg)]
		BsaStatusBekijken = 182,

		[MpLabel("BSA bijzondere omstandigheden toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		BsaBijzondereOmstandighedenToevoegenWijzigenVerwijderen = 183,

		[MpLabel("BSA ontheffing toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenBsaStatus)]
		BsaOntheffingToevoegenWijzigenVerwijderen = 184,

		[MpLabel("BSA definitief toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenBsaStatus)]
		BsaDefinitiefToevoegenWijzigenVerwijderen = 185,

		[MpLabel("Wijzigen BSA status")]
		[RolSoort.Nieuw, ImpliedBy(BeheerBsaStudiebegeleiding)]
		WijzigenBsaStatus = 186,

		[MpLabel("Communicatie bekijken")]
		[RolSoort.Recht, ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		CommunicatieBekijken = 187,

		[MpLabel("Beheer BSA/ studiebegeleiding")]
		[RolSoort.Nieuw, ImpliedBy(Superuser)]
		BeheerBsaStudiebegeleiding = 188,

		[MpLabel("Decaan notities toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenStudentdecaan)]
		DecaanNotitiesToevoegenWijzigenVerwijderen = 189,

		[MpLabel("Decaan notities bekijken")]
		[RolSoort.Recht, ImpliedBy(WijzigenStudentdecaan)]
		DecaanNotitiesBekijken = 190,

		[MpLabel("Wijzigen studentdecaan")]
		[RolSoort.Nieuw, ImpliedBy(Superuser)]
		WijzigenStudentdecaan = 191,

		[MpLabel("Statische groepen aanmaken wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(BeheerInschrijvingen)]
		StatischeGroepenAanmakenWijzigenVerwijderen = 192,

		[MpLabel("Statische groepen gebruiken")]
		[RolSoort.Recht, ImpliedBy(BeheerInschrijvingen)]
		StatischeGroepenGebruiken = 193,

		[MpLabel("Alumni netwerk toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenAlumniNetwerk)]
		AlumniNetwerkToevoegenWijzigenVerwijderen = 194,

		[MpLabel("Wijzigen alumni-netwerk")]
		[RolSoort.Nieuw, ImpliedBy(Superuser)]
		WijzigenAlumniNetwerk = 195,

		[MpLabel("Student wijzig personalia beperkt")]
		[RolSoort.Recht, ImpliedBy(InvoerNietReguliereInschrijvingen)]
		StudentWijzigPersonaliaBeperkt = 196,

		[MpLabel("Wijzigen verblijfsvergunning")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenVerblijfsvergunning = 197,

		[MpLabel("Student wijzig verblijfsvergunning")]
		[RolSoort.Recht, ImpliedBy(WijzigenVerblijfsvergunning)]
		StudentWijzigVerblijfsvergunning = 198,

		[MpLabel("Wijzigen pasfoto")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenPasfoto = 199,

		[MpLabel("Waardepapier templates")]
		[RolSoort.Recht, ImpliedBy(BeheerWaardepapierenSjablonen)]
		WaardepapierTemplates = 201,

		[MpLabel("Examen waardepapier generatie")]
		[RolSoort.Recht, ImpliedBy(WijzigenExamens)]
		ExamenWaardepapierGeneratie = 202,

		[MpLabel("Examen waardepapier bekijken")]
		[RolSoort.Recht, ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		ExamenWaardepapierBekijken = 203,

		[MpLabel("Examencommissies beheer")]
		[RolSoort.Recht, ImpliedBy(WijzigenExamens, BeheerGetuigschriften)]
		ExamencommissiesBeheer = 204,

		[MpLabel("Beheer waardepapieren sjablonen")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerWaardepapierenSjablonen = 205,

		[MpLabel("Cursusaanbod toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenCursusaanbod)]
		CursusaanbodToevoegenWijzigenVerwijderen = 206,

		[MpLabel("Cursusaanbod bekijken")]
		[RolSoort.Recht, ImpliedBy(BekijkenCursusdeelnamesEnAanbod, WijzigenCursusaanbod)]
		CursusaanbodBekijken = 207,

		[MpLabel("Beheer cursusaanbod/ cursusdeelnames")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerCursusaanbodCursusdeelnames = 208,

		[MpLabel("Diploma supplement beheer")]
		[RolSoort.Recht, ImpliedBy(WijzigenExamens, BeheerGetuigschriften)]
		DiplomaSupplementBeheer = 209,

		[MpLabel("Cursusdeelnames bekijken")]
		[RolSoort.Recht, ImpliedBy(BekijkenCursusdeelnamesEnAanbod)]
		CursusdeelnamesBekijken = 210,

		[MpLabel("Cursusdeelname toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenCursusdeelnames)]
		CursusdeelnameToevoegenWijzigenVerwijderen = 211,

		[MpLabel("Wijzigen cursusdeelnames")]
		[RolSoort.Nieuw, ImpliedBy(Superuser)]
		WijzigenCursusdeelnames = 212,

		[MpLabel("Kenmerken toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(BeheerInschrijvingen)]
		KenmerkenToevoegenWijzigenVerwijderen = 213,

		[MpLabel("Batches toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(BeheerInschrijvingen)]
		BatchesToevoegenWijzigenVerwijderen = 214,

		[MpLabel("Studenten samenvoegen")]
		[RolSoort.Recht, ImpliedBy(BeheerInschrijvingen)]
		StudentenSamenvoegen = 215,

		[MpLabel("Taken van iedereen wijzigen verwijderen annuleren")]
		[RolSoort.Recht]
		TakenVanIedereenWijzigenVerwijderenAnnuleren = 216,

		[MpLabel("Beheer getuigschriften")]
		[RolSoort.Nieuw, ImpliedBy(Superuser, Combi_BeheerAlles, Combi_FontysBeheerFO)]
		BeheerGetuigschriften = 217,

		[MpLabel("Bekijken cursusdeelnames en aanbod")]
		[RolSoort.Nieuw, ImpliedBy(WijzigenCursusdeelnames, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenCursusdeelnamesEnAanbod = 218,

		[MpLabel("Wijzigen cursusaanbod")]
		[RolSoort.Nieuw, ImpliedBy(BeheerCursusaanbodCursusdeelnames, Combi_FontysBeheerFO)]
		WijzigenCursusaanbod = 219,

		[MpLabel("Cursus stam tabellen toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(BeheerCursusaanbodCursusdeelnames)]
		CursusStamTabellenToevoegenWijzigenVerwijderen = 220,

		[MpLabel("Cursussen tab")]
		[RolSoort.Recht, ImpliedBy(BekijkenCursusdeelnamesEnAanbod, WijzigenCursusaanbod)]
		CursussenTab = 221,

		[MpLabel("Student richting toevoegen wijzigen verwijderen")]
		[RolSoort.Recht, ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentRichtingToevoegenWijzigenVerwijderen = 222,

		[MpLabel("COMBI: bekijk inschrijving/financieel/studielink/cursus")]
		[RolSoort.Nieuw, ImpliedBy(Combi_FontysBeheerFO)]
		Combi_BekijkInschrijvingFinancieelStudielinkCursus = 223,

		[MpLabel("COMBI: beheer alles")]
		[RolSoort.Nieuw]
		Combi_BeheerAlles = 224,

		[MpLabel("Document generatie vooropleidingen")]
		[RolSoort.Recht]
		DocumentGeneratieVooropleidingen = 225,

		[MpLabel("COMBI: Fontys Beheer FO")]
		[RolSoort.Nieuw]
		Combi_FontysBeheerFO = 226,
	}



	/*
	 * Ontwikkelmethodiek:
	 *  
	 * - Code generator die van DB toegangrecht een grote enum maakt
	 * - Zorg ervoor dan nieuwe rechten en oude rechten identiek namen hebben (makkelijker refactoren later).
	 * 
	 * - Hierarchyclass maken die declaratief toestaat om ouder rollen te defineren
	 * - Code die de transitive closure van de rollen structuur bepaald en zo dus de relevante onderliggende rollen van een gegeven voorouderrol
	 * - wat "common sense" unit tests"
	 * - wat unit tests die voor elk account, voor elke organisatie checked dat exact dezelfde rechten uitgekeerd worden.
	 * - wat unit tests die checken of de db wel met de enum in sync is.
	 * 
	 * - alle queries naar toegantrecht etc. aanpassen
	 * - later: alle tabellen behalve de kern recht tabel verwijderen.
	 * 
	 * */
}
