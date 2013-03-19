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
		[RolSoort.OudAttribute, Implies(OrganisatieboomBekijkenSelecteren, StudentTabblad, StudentInschrijvingenTabblad, AanmeldingenBekijken, InschrijvingenBekijken, VoorlopigeToelatingInvoerenWijziginVerwijderen)]
		StudentadministratieVoorlopigeToelating = -27,

		[MpLabel("Studentadministratie balie (alleen studenttabblad)")]
		[RolSoort.OudAttribute, Implies(StudentBekijkPersonalia, StudentTabblad, StudentPersoonTabblad, StudentInschrijvingenTabblad, StudentKenemerkenTabblad, VooropleidingenBekijken, AanmeldingenBekijken, AanmeldingInschrijvingMededelingenBekijken, NegatiefBindenStudieadviesBekijken, StudielinkberichtenBekijken, InschrijvingenBekijken, StudentIdentificatieBekijken, NietReguliereInschrijvingenBekijken, StudentAdresBekijken, Examenstabblad, Uitschrijvingentabblad, StudentTaaltoetstabblad, DocumentGeneratie, BbcBekijken, BbcToevoegenWijzigen, BbcAfdrukken, StudentCommunicatieTabblad)]
		StudentadministratieBalieAlleenStudenttabblad = -26,

		[MpLabel("Uitsluitend adreswijzigingen, te combineren met andere rollen")]
		[RolSoort.OudAttribute, Implies(StudentAdresWijzigenBeperkt, StudentAdresWijzigenVerwijderen)]
		UitsluitendAdreswijzigingenTeCombinerenMetAndereRollen = -25,

		[MpLabel("Studentadministratie alleen lezen, met accorderen toelatingseisen en kenmerken")]
		[RolSoort.OudAttribute, Implies(StudentadministratieAlleenInkijken, AanmeldingToelatingseisenAccorderen, StudentKenmerkToevoegenVerwijderenWijzigen)]
		StudentadministratieAlleenLezenMetAccorderenToelatingseisenEnKenmerken = -24,

		[MpLabel("Financiële administratie alleen inkijken")]
		[RolSoort.OudAttribute, Implies(StudentFinancieelTabblad, FinancieelBetalingsinformatieBekijken, FinancieelCollegegeldBekijken, FinancieelMachtigingBekijken, FinancieelNotitiesBekijken)]
		FinanciëleAdministratieAlleenInkijken = -23,

		[MpLabel("Financiële administratie")]
		[RolSoort.OudAttribute, Implies(FinanciëleAdministratieAlleenInkijken, FinancieelClieopToevoegenVerwijderenWijzigen, FinancieelClieopBekijken, FinancieelBetalingenToevoegenVerwijderenWijzigen, FinancieelCollegegeldToevoegenVerwijderenWijzigen, FinancieelMachtingToevoegenVerwijderenWijzigen, FinancieelNotitiesToevoegenWijzigenVerwijderen)]
		FinanciëleAdministratie = -22,

		[MpLabel("Studentadministratie alleen inkijken")]
		[RolSoort.OudAttribute, Implies(StudentadministratieBalieAlleenStudenttabblad, OrganisatieboomBekijkenSelecteren, StudentCommunicatieToevoegenVerwijderenWijzigen, InschrijvingNotitiesBekijken)]
		StudentadministratieAlleenInkijken = -21,

		[MpLabel("Studentadministratie niet-reguliere inschrijvingen")]
		[RolSoort.OudAttribute, Implies(StudentBekijkPersonalia, OrganisatieboomBekijkenSelecteren, StudentTabblad, StudentPersoonTabblad, StudentInschrijvingenTabblad, NietReguliereInschrijvingenBekijken, NietReguliereInschrijvingenToevoegen, StudentToevoegen, StudentAdresBekijken, StudentAdresWijzigenBeperkt)]
		StudentadministratieNietReguliereInschrijvingen = -20,

		[MpLabel("Studentadministratie")]
		[RolSoort.OudAttribute, Implies(StudentadministratieVoorlopigeToelating, UitsluitendAdreswijzigingenTeCombinerenMetAndereRollen, StudentadministratieAlleenLezenMetAccorderenToelatingseisenEnKenmerken, StudentadministratieNietReguliereInschrijvingen, StudentWijzigPersonalia, StudentVerwijderen, OpleidingenBekijken, InschrijvingBasisTabellenBekijken, OrganisatieFinancieelTabblad, StudentIdentificatieToevoegenVerwijderenWijzigen, StudentIdentificatieVerifieeren, StudentoverledenToevoegenVerwijderenWijzigen, StudentPasfotoToevoegenVerwijderen, StudentFinancieelTabblad, StudentOnderwijsTabblad, StudentKenemerkenTabblad, VooropleidingenToevoegenVerwijderenWijzigen, VooropleidingenVerifieeren, AanmeldingenToevoegenWijzigen, AanmeldingDefinitiefInschrijven, AanmeldingInschrijvingIntrekken, InschrijvingUitschrijven, InschrijvingExamenToevoegenVerwijderenWijzigen, AanmeldingInschrijvingMededelingVersturen, BlokkeerInschrijvingBekijken, SMMutatiesBekijken, FinancieelBetalingsinformatieBekijken, FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen, NietReguliereInschrijvingenWijzigenVerwijderen, StudentAdresWijzigenVerwijderen, StudentKenmerkToevoegenVerwijderenWijzigen)]
		Studentadministratie = -18,

		[MpLabel("Studentadministratie beheerder")]
		[RolSoort.OudAttribute, Implies(FinanciëleAdministratie, StudentadministratieExtra, OrganisatieToevoegenVerwijderenWijzigen, OpleidingenToevoegenVerwijderenWijzigen, OrganisatieBeheerTabblad, SMMutatiesToevoegenWijzigenVerwijderen, StudentEisenTabblad, OrganisatieFinancieelCollegegeldTabelBeheer, DocumentTemplates, TakenBekijkenWijzigen)]
		StudentadministratieBeheerder = -5,

		[MpLabel("Studentadministratie extra")]
		[RolSoort.OudAttribute, Implies(Studentadministratie, RapportenKengetallen, OrganisatieFinancieelTabblad, StudentIdentificatieVerifieeren, StudentOverledenVerifieeren, VooropleidingenVerifieeren, NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen, BlokkeerInschrijvingToevoegenWijzigenVerwijderen, VoorlopigeToelatingInvoerenWijziginVerwijderen, StudentCommunicatieToevoegenVerwijderenWijzigen, InschrijvingNotitiesToevoegenWijzigenVerwijderen)]
		StudentadministratieExtra = -4,

		[MpLabel("Bekijken wijzigen verwijderen alle tabellen")]
		[RolSoort.RechtAttribute]
		BekijkenWijzigenVerwijderenAlleTabellen = 1,

		[MpLabel("Student bekijk personalia")]
		[RolSoort.RechtAttribute]
		StudentBekijkPersonalia = 2,

		[MpLabel("Organisatie toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		OrganisatieToevoegenVerwijderenWijzigen = 4,

		[MpLabel("Organisatieboom bekijken selecteren")]
		[RolSoort.RechtAttribute]
		OrganisatieboomBekijkenSelecteren = 5,

		[MpLabel("Rapporten kengetallen")]
		[RolSoort.RechtAttribute]
		RapportenKengetallen = 30,

		[MpLabel("Student wijzig personalia")]
		[RolSoort.RechtAttribute]
		StudentWijzigPersonalia = 32,

		[MpLabel("Student verwijderen")]
		[RolSoort.RechtAttribute]
		StudentVerwijderen = 33,

		[MpLabel("Opleidingen bekijken")]
		[RolSoort.RechtAttribute]
		OpleidingenBekijken = 35,

		[MpLabel("Opleidingen toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		OpleidingenToevoegenVerwijderenWijzigen = 36,

		[MpLabel("Accounts toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		AccountsToevoegenVerwijderenWijzigen = 37,

		[MpLabel("Student tabblad")]
		[RolSoort.RechtAttribute]
		StudentTabblad = 38,

		[MpLabel("Bekijken wijzigen verwijderen meta data")]
		[RolSoort.RechtAttribute]
		BekijkenWijzigenVerwijderenMetaData = 42,

		[MpLabel("Inschrijving basis tabellen bekijken")]
		[RolSoort.RechtAttribute]
		InschrijvingBasisTabellenBekijken = 43,

		[MpLabel("Organisatie beheer tabblad")]
		[RolSoort.RechtAttribute]
		OrganisatieBeheerTabblad = 44,

		[MpLabel("Organisatie financieel tabblad")]
		[RolSoort.RechtAttribute]
		OrganisatieFinancieelTabblad = 45,

		[MpLabel("Student identificatie toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		StudentIdentificatieToevoegenVerwijderenWijzigen = 46,

		[MpLabel("Student identificatie verifieeren")]
		[RolSoort.RechtAttribute]
		StudentIdentificatieVerifieeren = 47,

		[MpLabel("Studentoverleden toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		StudentoverledenToevoegenVerwijderenWijzigen = 48,

		[MpLabel("Student overleden verifieeren")]
		[RolSoort.RechtAttribute]
		StudentOverledenVerifieeren = 49,

		[MpLabel("Student pasfoto toevoegen verwijderen")]
		[RolSoort.RechtAttribute]
		StudentPasfotoToevoegenVerwijderen = 50,

		[MpLabel("Student persoon tabblad")]
		[RolSoort.RechtAttribute]
		StudentPersoonTabblad = 51,

		[MpLabel("Student inschrijvingen tabblad")]
		[RolSoort.RechtAttribute]
		StudentInschrijvingenTabblad = 52,

		[MpLabel("Student financieel tabblad")]
		[RolSoort.RechtAttribute]
		StudentFinancieelTabblad = 53,

		[MpLabel("Student onderwijs tabblad")]
		[RolSoort.RechtAttribute]
		StudentOnderwijsTabblad = 54,

		[MpLabel("Student kenemerken tabblad")]
		[RolSoort.RechtAttribute]
		StudentKenemerkenTabblad = 55,

		[MpLabel("Vooropleidingen toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		VooropleidingenToevoegenVerwijderenWijzigen = 58,

		[MpLabel("Vooropleidingen verifieeren")]
		[RolSoort.RechtAttribute]
		VooropleidingenVerifieeren = 59,

		[MpLabel("Vooropleidingen bekijken")]
		[RolSoort.RechtAttribute]
		VooropleidingenBekijken = 60,

		[MpLabel("Aanmeldingen bekijken")]
		[RolSoort.RechtAttribute]
		AanmeldingenBekijken = 61,

		[MpLabel("Aanmeldingen toevoegen wijzigen")]
		[RolSoort.RechtAttribute]
		AanmeldingenToevoegenWijzigen = 62,

		[MpLabel("Aanmelding definitief inschrijven")]
		[RolSoort.RechtAttribute]
		AanmeldingDefinitiefInschrijven = 63,

		[MpLabel("Aanmelding inschrijving intrekken")]
		[RolSoort.RechtAttribute]
		AanmeldingInschrijvingIntrekken = 64,

		[MpLabel("Aanmelding inschrijving mededelingen bekijken")]
		[RolSoort.RechtAttribute]
		AanmeldingInschrijvingMededelingenBekijken = 65,

		[MpLabel("Inschrijving uitschrijven")]
		[RolSoort.RechtAttribute]
		InschrijvingUitschrijven = 66,

		[MpLabel("Inschrijving examen toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		InschrijvingExamenToevoegenVerwijderenWijzigen = 67,

		[MpLabel("Aanmelding inschrijving mededeling versturen")]
		[RolSoort.RechtAttribute]
		AanmeldingInschrijvingMededelingVersturen = 68,

		[MpLabel("Negatief binden studieadvies bekijken")]
		[RolSoort.RechtAttribute]
		NegatiefBindenStudieadviesBekijken = 69,

		[MpLabel("Negatief bindend studieadvies toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen = 70,

		[MpLabel("Blokkeer inschrijving toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		BlokkeerInschrijvingToevoegenWijzigenVerwijderen = 71,

		[MpLabel("Blokkeer inschrijving bekijken")]
		[RolSoort.RechtAttribute]
		BlokkeerInschrijvingBekijken = 72,

		[MpLabel("SM mutaties toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		SMMutatiesToevoegenWijzigenVerwijderen = 73,

		[MpLabel("SM mutaties bekijken")]
		[RolSoort.RechtAttribute]
		SMMutatiesBekijken = 74,

		[MpLabel("Studielinkberichten bekijken")]
		[RolSoort.RechtAttribute]
		StudielinkberichtenBekijken = 75,

		[MpLabel("Studielinkberichten beheer")]
		[RolSoort.RechtAttribute]
		StudielinkberichtenBeheer = 76,

		[MpLabel("Inschrijvingen bekijken")]
		[RolSoort.RechtAttribute]
		InschrijvingenBekijken = 77,

		[MpLabel("Financieel betalingsinformatie bekijken")]
		[RolSoort.RechtAttribute]
		FinancieelBetalingsinformatieBekijken = 79,

		[MpLabel("Financieel betalingsinformatie toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen = 80,

		[MpLabel("Student identificatie bekijken")]
		[RolSoort.RechtAttribute]
		StudentIdentificatieBekijken = 81,

		[MpLabel("Financieel clieop toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		FinancieelClieopToevoegenVerwijderenWijzigen = 82,

		[MpLabel("Financieel clieop bekijken")]
		[RolSoort.RechtAttribute]
		FinancieelClieopBekijken = 83,

		[MpLabel("Niet reguliere inschrijvingen bekijken")]
		[RolSoort.RechtAttribute]
		NietReguliereInschrijvingenBekijken = 84,

		[MpLabel("Niet reguliere inschrijvingen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		NietReguliereInschrijvingenWijzigenVerwijderen = 85,

		[MpLabel("Financieel betalingen toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		FinancieelBetalingenToevoegenVerwijderenWijzigen = 86,

		[MpLabel("Financieel collegegeld toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		FinancieelCollegegeldToevoegenVerwijderenWijzigen = 87,

		[MpLabel("Financieel collegegeld bekijken")]
		[RolSoort.RechtAttribute]
		FinancieelCollegegeldBekijken = 88,

		[MpLabel("Financieel machting toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		FinancieelMachtingToevoegenVerwijderenWijzigen = 89,

		[MpLabel("Financieel machtiging bekijken")]
		[RolSoort.RechtAttribute]
		FinancieelMachtigingBekijken = 90,

		[MpLabel("Niet reguliere inschrijvingen toevoegen")]
		[RolSoort.RechtAttribute]
		NietReguliereInschrijvingenToevoegen = 91,

		[MpLabel("Student toevoegen")]
		[RolSoort.RechtAttribute]
		StudentToevoegen = 92,

		[MpLabel("Student adres bekijken")]
		[RolSoort.RechtAttribute]
		StudentAdresBekijken = 93,

		[MpLabel("Student adres toevoegen")]
		[RolSoort.RechtAttribute]
		StudentAdresWijzigenBeperkt = 94,

		[MpLabel("Student adres wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		StudentAdresWijzigenVerwijderen = 95,

		[MpLabel("Aanmelding toelatingseisen accorderen")]
		[RolSoort.RechtAttribute]
		AanmeldingToelatingseisenAccorderen = 96,

		[MpLabel("Student eisen tabblad")]
		[RolSoort.RechtAttribute]
		StudentEisenTabblad = 97,

		[MpLabel("Voorlopige toelating invoeren wijzigin verwijderen")]
		[RolSoort.RechtAttribute]
		VoorlopigeToelatingInvoerenWijziginVerwijderen = 98,

		[MpLabel("Student kenmerk toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		StudentKenmerkToevoegenVerwijderenWijzigen = 99,

		[MpLabel("Organisatie financieel collegegeld tabel beheer")]
		[RolSoort.RechtAttribute]
		OrganisatieFinancieelCollegegeldTabelBeheer = 100,

		[MpLabel("Organisatie onderwijs tab")]
		[RolSoort.RechtAttribute]
		OrganisatieOnderwijsTab = 101,

		[MpLabel("Examenstabblad")]
		[RolSoort.RechtAttribute]
		Examenstabblad = 102,

		[MpLabel("Uitschrijvingentabblad")]
		[RolSoort.RechtAttribute]
		Uitschrijvingentabblad = 103,

		[MpLabel("Student taaltoetstabblad")]
		[RolSoort.RechtAttribute]
		StudentTaaltoetstabblad = 104,

		[MpLabel("Webservice alle tabellen lezen")]
		[RolSoort.RechtAttribute]
		WebserviceAlleTabellenLezen = 105,

		[MpLabel("Document templates")]
		[RolSoort.RechtAttribute]
		DocumentTemplates = 106,

		[MpLabel("Document generatie")]
		[RolSoort.RechtAttribute]
		DocumentGeneratie = 107,

		[MpLabel("BBCs bekijken")]
		[RolSoort.RechtAttribute]
		BbcBekijken = 108,

		[MpLabel("BBCs toevoegen wijzigen")]
		[RolSoort.RechtAttribute]
		BbcToevoegenWijzigen = 109,

		[MpLabel("BBCs afdrukken")]
		[RolSoort.RechtAttribute]
		BbcAfdrukken = 110,

		[MpLabel("Student communicatie tabblad")]
		[RolSoort.RechtAttribute]
		StudentCommunicatieTabblad = 111,

		[MpLabel("Student communicatie toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		StudentCommunicatieToevoegenVerwijderenWijzigen = 112,

		[MpLabel("Communicatie template toevoegen verwijderen wijzigen")]
		[RolSoort.RechtAttribute]
		CommunicatieTemplateToevoegenVerwijderenWijzigen = 113,

		[MpLabel("Volg onderwijs beheer alles")]
		[RolSoort.RechtAttribute]
		VolgOnderwijsBeheerAlles = 114,

		[MpLabel("Volg onderwijs beheer beperkt")]
		[RolSoort.RechtAttribute]
		VolgOnderwijsBeheerBeperkt = 115,

		[MpLabel("Volg onderwijs inzien")]
		[RolSoort.RechtAttribute]
		VolgOnderwijsInzien = 116,

		[MpLabel("Bekijken kengetallen")]
		[RolSoort.NieuwAttribute, Implies(RapportenKengetallen)]
		BekijkenKengetallen = 119,

		[MpLabel("Organisatietabblad en boom")]
		[RolSoort.RechtAttribute]
		OrganisatietabbladEnBoom = 120,

		[MpLabel("Bekijken studievolg")]
		[RolSoort.NieuwAttribute, Implies(StudentOnderwijsTabblad, OrganisatieOnderwijsTab, VolgOnderwijsInzien, BekijkenInschrijvingen, BsaStatusBekijken)]
		BekijkenStudievolg = 121,

		[MpLabel("Bekijken studielinkberichten")]
		[RolSoort.NieuwAttribute, Implies(StudielinkberichtenBekijken, BekijkenInschrijvingen)]
		BekijkenStudielinkberichten = 123,

		[MpLabel("Bekijken uitwisseling DUO")]
		[RolSoort.NieuwAttribute, Implies(SMMutatiesBekijken, BekijkenInschrijvingen)]
		BekijkenUitwisselingDuo = 124,

		[MpLabel("Wijzigen inschrijvingen en aanmeldingen")]
		[RolSoort.NieuwAttribute, Implies(StudentWijzigPersonalia, StudentIdentificatieToevoegenVerwijderenWijzigen, StudentIdentificatieVerifieeren, StudentoverledenToevoegenVerwijderenWijzigen, StudentOverledenVerifieeren, VooropleidingenToevoegenVerwijderenWijzigen, VooropleidingenVerifieeren, AanmeldingenToevoegenWijzigen, AanmeldingDefinitiefInschrijven, AanmeldingInschrijvingIntrekken, AanmeldingInschrijvingMededelingVersturen, NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen, BlokkeerInschrijvingToevoegenWijzigenVerwijderen, StudentAdresWijzigenVerwijderen, StudentKenmerkToevoegenVerwijderenWijzigen, DocumentGeneratie, StudentCommunicatieToevoegenVerwijderenWijzigen, WijzigenCorrespondentieadressen, InvoerNietReguliereInschrijvingen, WijzigenAccorderenToelatingseisen, WijzigenVoorlopigeToelating, BekijkenInschrijvingenUitgebreid, StudentCommunicatieInschrijvingToevoegen, InschrijvingNotitiesToevoegenWijzigenVerwijderen, WijzigenVerblijfsvergunning, WijzigenPasfoto, StudentRichtingToevoegenWijzigenVerwijderen)]
		WijzigenInschrijvingenEnAanmeldingen = 125,

		[MpLabel("Wijzigen uitschrijvingen")]
		[RolSoort.NieuwAttribute, Implies(InschrijvingUitschrijven, BekijkenInschrijvingenUitgebreid, InschrijvingNotitiesToevoegenWijzigenVerwijderen)]
		WijzigenUitschrijvingen = 126,

		[MpLabel("Wijzigen examens")]
		[RolSoort.NieuwAttribute, Implies(InschrijvingExamenToevoegenVerwijderenWijzigen, BekijkenInschrijvingenUitgebreid, InschrijvingNotitiesToevoegenWijzigenVerwijderen, ExamenWaardepapierGeneratie, ExamencommissiesBeheer, DiplomaSupplementBeheer)]
		WijzigenExamens = 127,

		[MpLabel("Wijzigen correspondentieadressen")]
		[RolSoort.NieuwAttribute, Implies(StudentToevoegen, StudentAdresWijzigenBeperkt, BekijkenInschrijvingen)]
		WijzigenCorrespondentieadressen = 128,

		[MpLabel("Invoer niet-reguliere inschrijvingen")]
		[RolSoort.NieuwAttribute, Implies(NietReguliereInschrijvingenWijzigenVerwijderen, NietReguliereInschrijvingenToevoegen, StudentToevoegen, StudentAdresWijzigenBeperkt, BekijkenInschrijvingen, StudentWijzigPersonaliaBeperkt)]
		InvoerNietReguliereInschrijvingen = 129,

		[MpLabel("Wijzigen financieel")]
		[RolSoort.NieuwAttribute, Implies(FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen, FinancieelBetalingenToevoegenVerwijderenWijzigen, FinancieelCollegegeldToevoegenVerwijderenWijzigen, FinancieelMachtingToevoegenVerwijderenWijzigen, WijzigenToevoegenAfdrukkenBbc, BekijkenFinancieelUitgebreid, FinancieelNotitiesToevoegenWijzigenVerwijderen)]
		WijzigenFinancieel = 132,

		[MpLabel("Wijzigen/ toevoegen/ afdrukken BBC's")]
		[RolSoort.NieuwAttribute, Implies(DocumentGeneratie, BbcToevoegenWijzigen, BbcAfdrukken, BekijkenFinancieel)]
		WijzigenToevoegenAfdrukkenBbc = 133,

		[MpLabel("Beheer financieel")]
		[RolSoort.NieuwAttribute, Implies(OrganisatieBeheerTabblad, FinancieelClieopToevoegenVerwijderenWijzigen, FinancieelClieopBekijken, OrganisatieFinancieelCollegegeldTabelBeheer, WijzigenFinancieel)]
		BeheerFinancieel = 134,

		[MpLabel("Beheer inschrijvingen")]
		[RolSoort.NieuwAttribute, Implies(OrganisatieToevoegenVerwijderenWijzigen, StudentVerwijderen, OpleidingenBekijken, OpleidingenToevoegenVerwijderenWijzigen, InschrijvingBasisTabellenBekijken, OrganisatieBeheerTabblad, DocumentTemplates, BekijkenKengetallen, WijzigenInschrijvingenEnAanmeldingen, WijzigenUitschrijvingen, WijzigenExamens, WijzigenCrm, TakenBekijkenWijzigen, StatischeGroepenAanmakenWijzigenVerwijderen, StatischeGroepenGebruiken, KenmerkenToevoegenWijzigenVerwijderen, BatchesToevoegenWijzigenVerwijderen, StudentenSamenvoegen)]
		BeheerInschrijvingen = 135,

		[MpLabel("Wijzigen accorderen toelatingseisen")]
		[RolSoort.NieuwAttribute, Implies(AanmeldingToelatingseisenAccorderen, BekijkenInschrijvingen)]
		WijzigenAccorderenToelatingseisen = 136,

		[MpLabel("Wijzigen voorlopige toelating")]
		[RolSoort.NieuwAttribute, Implies(VoorlopigeToelatingInvoerenWijziginVerwijderen, BekijkenInschrijvingen)]
		WijzigenVoorlopigeToelating = 137,

		[MpLabel("Superuser")]
		[RolSoort.NieuwAttribute, Implies(BekijkenWijzigenVerwijderenAlleTabellen, BekijkenWijzigenVerwijderenMetaData, StudielinkberichtenBeheer, StudentCommunicatieTabblad, CommunicatieTemplateToevoegenVerwijderenWijzigen, BeheerFinancieel, BeheerInschrijvingen, BeheerStudievolg, BeheerAccountsEnRollen, BeheerStudielink, BeheerUitwisselingDuo, BeheerBsaStudiebegeleiding, WijzigenStudentdecaan, WijzigenAlumniNetwerk, BeheerWaardepapierenSjablonen, BeheerCursusaanbodCursusdeelnames, WijzigenCursusdeelnames, BeheerGetuigschriften)]
		Superuser = 138,

		[MpLabel("Wijzigen studievolg")]
		[RolSoort.NieuwAttribute, Implies(VolgOnderwijsBeheerBeperkt, BekijkenStudievolg, WijzigenCrm, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding, StudievoortgangNotitiesBekijken, StudievoortgangNotitiesToevoegenWijzigenVerwijderen)]
		WijzigenStudievolg = 139,

		[MpLabel("Beheer studievolg")]
		[RolSoort.NieuwAttribute, Implies(StudentKenmerkToevoegenVerwijderenWijzigen, VolgOnderwijsBeheerAlles, WijzigenStudievolg)]
		BeheerStudievolg = 140,

		[MpLabel("Bekijken inschrijvingen")]
		[RolSoort.NieuwAttribute, Implies(StudentBekijkPersonalia, StudentTabblad, StudentPersoonTabblad, StudentInschrijvingenTabblad, VooropleidingenBekijken, AanmeldingenBekijken, InschrijvingenBekijken, NietReguliereInschrijvingenBekijken, StudentAdresBekijken)]
		BekijkenInschrijvingen = 160,

		[MpLabel("Bekijken inschrijvingen uitgebreid")]
		[RolSoort.NieuwAttribute, Implies(StudentKenemerkenTabblad, AanmeldingInschrijvingMededelingenBekijken, NegatiefBindenStudieadviesBekijken, BlokkeerInschrijvingBekijken, StudentIdentificatieBekijken, StudentEisenTabblad, Examenstabblad, Uitschrijvingentabblad, StudentTaaltoetstabblad, StudentCommunicatieTabblad, BekijkenInschrijvingen, InschrijvingNotitiesBekijken, CommunicatieBekijken, ExamenWaardepapierBekijken)]
		BekijkenInschrijvingenUitgebreid = 161,

		[MpLabel("Bekijken financieel")]
		[RolSoort.NieuwAttribute, Implies(StudentFinancieelTabblad, FinancieelBetalingsinformatieBekijken, FinancieelCollegegeldBekijken, FinancieelMachtigingBekijken, BbcBekijken, BekijkenInschrijvingen)]
		BekijkenFinancieel = 162,

		[MpLabel("Bekijken financieel uitgebreid")]
		[RolSoort.NieuwAttribute, Implies(OrganisatieFinancieelTabblad, BekijkenFinancieel, FinancieelNotitiesBekijken)]
		BekijkenFinancieelUitgebreid = 163,

		[MpLabel("Beheer accounts en rollen")]
		[RolSoort.NieuwAttribute, Implies(AccountsToevoegenVerwijderenWijzigen, OrganisatieBeheerTabblad)]
		BeheerAccountsEnRollen = 165,

		[MpLabel("Beheer studielink")]
		[RolSoort.NieuwAttribute, Implies(StudielinkberichtenBeheer, BekijkenStudielinkberichten)]
		BeheerStudielink = 166,

		[MpLabel("Beheer uitwisseling DUO")]
		[RolSoort.NieuwAttribute, Implies(SMMutatiesToevoegenWijzigenVerwijderen, BekijkenUitwisselingDuo)]
		BeheerUitwisselingDuo = 167,

		[MpLabel("Wijzigen CRM")]
		[RolSoort.NieuwAttribute, Implies(DocumentGeneratie, StudentCommunicatieTabblad, StudentCommunicatieToevoegenVerwijderenWijzigen)]
		WijzigenCrm = 168,

		[MpLabel("Taken bekijken wijzigen")]
		[RolSoort.RechtAttribute]
		TakenBekijkenWijzigen = 169,

		[MpLabel("Wijzigen BSA bijzondere omstandigheden/ studiebegeleiding")]
		[RolSoort.NieuwAttribute, Implies(BekijkenStudievolg, WijzigenCrm, StudiebegeleidingGespreksnotitiesBekijken, StudiebegeleidingGespreksnotitiesToevoegenWijzigenVerwijderen, StudievoortgangNotitiesBekijken, BsaBijzondereOmstandighedenToevoegenWijzigenVerwijderen)]
		WijzigenBsaBijzondereOmstandighedenStudiebegeleiding = 172,

		[MpLabel("Student communicatie inschrijving toevoegen")]
		[RolSoort.RechtAttribute]
		StudentCommunicatieInschrijvingToevoegen = 173,

		[MpLabel("Inschrijving notities bekijken")]
		[RolSoort.RechtAttribute]
		InschrijvingNotitiesBekijken = 174,

		[MpLabel("Inschrijving notities toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		InschrijvingNotitiesToevoegenWijzigenVerwijderen = 175,

		[MpLabel("Financieel notities bekijken")]
		[RolSoort.RechtAttribute]
		FinancieelNotitiesBekijken = 176,

		[MpLabel("Financieel notities toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		FinancieelNotitiesToevoegenWijzigenVerwijderen = 177,

		[MpLabel("Studiebegeleiding gespreksnotities bekijken")]
		[RolSoort.RechtAttribute]
		StudiebegeleidingGespreksnotitiesBekijken = 178,

		[MpLabel("Studiebegeleiding gespreksnotities toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		StudiebegeleidingGespreksnotitiesToevoegenWijzigenVerwijderen = 179,

		[MpLabel("Studievoortgang notities bekijken")]
		[RolSoort.RechtAttribute]
		StudievoortgangNotitiesBekijken = 180,

		[MpLabel("Studievoortgang notities toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		StudievoortgangNotitiesToevoegenWijzigenVerwijderen = 181,

		[MpLabel("BSA status bekijken")]
		[RolSoort.RechtAttribute]
		BsaStatusBekijken = 182,

		[MpLabel("BSA bijzondere omstandigheden toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		BsaBijzondereOmstandighedenToevoegenWijzigenVerwijderen = 183,

		[MpLabel("BSA ontheffing toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		BsaOntheffingToevoegenWijzigenVerwijderen = 184,

		[MpLabel("BSA definitief toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		BsaDefinitiefToevoegenWijzigenVerwijderen = 185,

		[MpLabel("Wijzigen BSA status")]
		[RolSoort.NieuwAttribute, Implies(BekijkenStudievolg, BsaOntheffingToevoegenWijzigenVerwijderen, BsaDefinitiefToevoegenWijzigenVerwijderen)]
		WijzigenBsaStatus = 186,

		[MpLabel("Communicatie bekijken")]
		[RolSoort.RechtAttribute]
		CommunicatieBekijken = 187,

		[MpLabel("Beheer BSA/ studiebegeleiding")]
		[RolSoort.NieuwAttribute, Implies(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding, WijzigenBsaStatus)]
		BeheerBsaStudiebegeleiding = 188,

		[MpLabel("Decaan notities toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		DecaanNotitiesToevoegenWijzigenVerwijderen = 189,

		[MpLabel("Decaan notities bekijken")]
		[RolSoort.RechtAttribute]
		DecaanNotitiesBekijken = 190,

		[MpLabel("Wijzigen studentdecaan")]
		[RolSoort.NieuwAttribute, Implies(BekijkenStudievolg, BekijkenInschrijvingenUitgebreid, BekijkenFinancieelUitgebreid, DecaanNotitiesToevoegenWijzigenVerwijderen, DecaanNotitiesBekijken)]
		WijzigenStudentdecaan = 191,

		[MpLabel("Statische groepen aanmaken wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		StatischeGroepenAanmakenWijzigenVerwijderen = 192,

		[MpLabel("Statische groepen gebruiken")]
		[RolSoort.RechtAttribute]
		StatischeGroepenGebruiken = 193,

		[MpLabel("Alumni netwerk toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		AlumniNetwerkToevoegenWijzigenVerwijderen = 194,

		[MpLabel("Wijzigen alumni-netwerk")]
		[RolSoort.NieuwAttribute, Implies(BekijkenInschrijvingen, AlumniNetwerkToevoegenWijzigenVerwijderen)]
		WijzigenAlumniNetwerk = 195,

		[MpLabel("Student wijzig personalia beperkt")]
		[RolSoort.RechtAttribute]
		StudentWijzigPersonaliaBeperkt = 196,

		[MpLabel("Wijzigen verblijfsvergunning")]
		[RolSoort.NieuwAttribute, Implies(BekijkenInschrijvingenUitgebreid, StudentWijzigVerblijfsvergunning)]
		WijzigenVerblijfsvergunning = 197,

		[MpLabel("Student wijzig verblijfsvergunning")]
		[RolSoort.RechtAttribute]
		StudentWijzigVerblijfsvergunning = 198,

		[MpLabel("Wijzigen pasfoto")]
		[RolSoort.NieuwAttribute, Implies(StudentPasfotoToevoegenVerwijderen, BekijkenInschrijvingenUitgebreid)]
		WijzigenPasfoto = 199,

		[MpLabel("Waardepapier templates")]
		[RolSoort.RechtAttribute]
		WaardepapierTemplates = 201,

		[MpLabel("Examen waardepapier generatie")]
		[RolSoort.RechtAttribute]
		ExamenWaardepapierGeneratie = 202,

		[MpLabel("Examen waardepapier bekijken")]
		[RolSoort.RechtAttribute]
		ExamenWaardepapierBekijken = 203,

		[MpLabel("Examencommissies beheer")]
		[RolSoort.RechtAttribute]
		ExamencommissiesBeheer = 204,

		[MpLabel("Beheer waardepapieren sjablonen")]
		[RolSoort.NieuwAttribute, Implies(WijzigenExamens, WaardepapierTemplates)]
		BeheerWaardepapierenSjablonen = 205,

		[MpLabel("Cursusaanbod toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		CursusaanbodToevoegenWijzigenVerwijderen = 206,

		[MpLabel("Cursusaanbod bekijken")]
		[RolSoort.RechtAttribute]
		CursusaanbodBekijken = 207,

		[MpLabel("Beheer cursusaanbod/ cursusdeelnames")]
		[RolSoort.NieuwAttribute, Implies(WijzigenCursusaanbod, CursusStamTabellenToevoegenWijzigenVerwijderen)]
		BeheerCursusaanbodCursusdeelnames = 208,

		[MpLabel("Diploma supplement beheer")]
		[RolSoort.RechtAttribute]
		DiplomaSupplementBeheer = 209,

		[MpLabel("Cursusdeelnames bekijken")]
		[RolSoort.RechtAttribute]
		CursusdeelnamesBekijken = 210,

		[MpLabel("Cursusdeelname toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		CursusdeelnameToevoegenWijzigenVerwijderen = 211,

		[MpLabel("Wijzigen cursusdeelnames")]
		[RolSoort.NieuwAttribute, Implies(CursusdeelnameToevoegenWijzigenVerwijderen, BekijkenCursusdeelnamesEnAanbod)]
		WijzigenCursusdeelnames = 212,

		[MpLabel("Kenmerken toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		KenmerkenToevoegenWijzigenVerwijderen = 213,

		[MpLabel("Batches toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		BatchesToevoegenWijzigenVerwijderen = 214,

		[MpLabel("Studenten samenvoegen")]
		[RolSoort.RechtAttribute]
		StudentenSamenvoegen = 215,

		[MpLabel("Taken van iedereen wijzigen verwijderen annuleren")]
		[RolSoort.RechtAttribute]
		TakenVanIedereenWijzigenVerwijderenAnnuleren = 216,

		[MpLabel("Beheer getuigschriften")]
		[RolSoort.NieuwAttribute, Implies(OpleidingenBekijken, OrganisatieBeheerTabblad, DocumentTemplates, CommunicatieTemplateToevoegenVerwijderenWijzigen, ExamencommissiesBeheer, DiplomaSupplementBeheer)]
		BeheerGetuigschriften = 217,

		[MpLabel("Bekijken cursusdeelnames en aanbod")]
		[RolSoort.NieuwAttribute, Implies(CursusaanbodBekijken, CursusdeelnamesBekijken, CursussenTab)]
		BekijkenCursusdeelnamesEnAanbod = 218,

		[MpLabel("Wijzigen cursusaanbod")]
		[RolSoort.NieuwAttribute, Implies(CursusaanbodToevoegenWijzigenVerwijderen, CursusaanbodBekijken, CursussenTab)]
		WijzigenCursusaanbod = 219,

		[MpLabel("Cursus stam tabellen toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		CursusStamTabellenToevoegenWijzigenVerwijderen = 220,

		[MpLabel("Cursussen tab")]
		[RolSoort.RechtAttribute]
		CursussenTab = 221,

		[MpLabel("Student richting toevoegen wijzigen verwijderen")]
		[RolSoort.RechtAttribute]
		StudentRichtingToevoegenWijzigenVerwijderen = 222,

		[MpLabel("COMBI: bekijk inschrijving/financieel/studielink/cursus")]
		[RolSoort.NieuwAttribute, Implies(BekijkenStudielinkberichten, BekijkenInschrijvingenUitgebreid, BekijkenFinancieelUitgebreid, BekijkenCursusdeelnamesEnAanbod)]
		Combi_BekijkInschrijvingFinancieelStudielinkCursus = 223,

		[MpLabel("COMBI: beheer alles")]
		[RolSoort.NieuwAttribute, Implies(BeheerFinancieel, BeheerInschrijvingen, BeheerAccountsEnRollen, BeheerStudielink, BeheerUitwisselingDuo, BeheerWaardepapierenSjablonen, BeheerCursusaanbodCursusdeelnames, BeheerGetuigschriften)]
		Combi_BeheerAlles = 224,

		[MpLabel("Document generatie vooropleidingen")]
		[RolSoort.RechtAttribute]
		DocumentGeneratieVooropleidingen = 225,

		[MpLabel("COMBI: Fontys Beheer FO")]
		[RolSoort.NieuwAttribute, Implies(BeheerGetuigschriften, WijzigenCursusaanbod, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
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
