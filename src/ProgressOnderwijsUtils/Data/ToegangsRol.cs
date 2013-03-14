// ReSharper disable UnusedMember.Global
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils.DoNotUseYet
{
	internal sealed class ImpliedByAttribute : Attribute
	{
		public readonly IReadOnlyList<ToegangsRol> Ouders;
		public ImpliedByAttribute(params ToegangsRol[] ouders)
		{
			Ouders = ouders;
		}
	}

	public enum ToegangsRol
	{
		[MpLabel("Bekijken wijzigen verwijderen alle tabellen")]
		[ImpliedBy(Superuser)]
		BekijkenWijzigenVerwijderenAlleTabellen = 1,

		[MpLabel("Student bekijk personalia")]
		[ImpliedBy(BekijkenInschrijvingen)]
		StudentBekijkPersonalia = 2,

		[MpLabel("Organisatie toevoegen verwijderen wijzigen")]
		[ImpliedBy(BeheerInschrijvingen)]
		OrganisatieToevoegenVerwijderenWijzigen = 4,

		[MpLabel("Organisatieboom bekijken selecteren")]
		OrganisatieboomBekijkenSelecteren = 5,

		[MpLabel("Rapporten kengetallen")]
		[ImpliedBy(BekijkenKengetallen)]
		RapportenKengetallen = 30,

		[MpLabel("Student wijzig personalia")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentWijzigPersonalia = 32,

		[MpLabel("Student verwijderen")]
		[ImpliedBy(BeheerInschrijvingen)]
		StudentVerwijderen = 33,

		[MpLabel("Opleidingen bekijken")]
		[ImpliedBy(BeheerInschrijvingen, BeheerGetuigschriften)]
		OpleidingenBekijken = 35,

		[MpLabel("Opleidingen toevoegen verwijderen wijzigen")]
		[ImpliedBy(BeheerInschrijvingen)]
		OpleidingenToevoegenVerwijderenWijzigen = 36,

		[MpLabel("Accounts toevoegen verwijderen wijzigen")]
		[ImpliedBy(BeheerAccountsEnRollen)]
		AccountsToevoegenVerwijderenWijzigen = 37,

		[MpLabel("Student tabblad")]
		[ImpliedBy(BekijkenInschrijvingen)]
		StudentTabblad = 38,

		[MpLabel("Bekijken wijzigen verwijderen meta data")]
		[ImpliedBy(Superuser)]
		BekijkenWijzigenVerwijderenMetaData = 42,

		[MpLabel("Inschrijving basis tabellen bekijken")]
		[ImpliedBy(BeheerInschrijvingen)]
		InschrijvingBasisTabellenBekijken = 43,

		[MpLabel("Organisatie beheer tabblad")]
		[ImpliedBy(BeheerFinancieel, BeheerInschrijvingen, BeheerAccountsEnRollen, BeheerGetuigschriften)]
		OrganisatieBeheerTabblad = 44,

		[MpLabel("Organisatie financieel tabblad")]
		[ImpliedBy(BekijkenFinancieelUitgebreid)]
		OrganisatieFinancieelTabblad = 45,

		[MpLabel("Student identificatie toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentIdentificatieToevoegenVerwijderenWijzigen = 46,

		[MpLabel("Student identificatie verifieeren")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentIdentificatieVerifieeren = 47,

		[MpLabel("Studentoverleden toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentoverledenToevoegenVerwijderenWijzigen = 48,

		[MpLabel("Student overleden verifieeren")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentOverledenVerifieeren = 49,

		[MpLabel("Student pasfoto toevoegen verwijderen")]
		[ImpliedBy(WijzigenPasfoto)]
		StudentPasfotoToevoegenVerwijderen = 50,

		[MpLabel("Student persoon tabblad")]
		[ImpliedBy(BekijkenInschrijvingen)]
		StudentPersoonTabblad = 51,

		[MpLabel("Inschrijvingen tabblad")]
		[ImpliedBy(BekijkenInschrijvingen)]
		InschrijvingenTabblad = 52,

		[MpLabel("Student financieel tabblad")]
		[ImpliedBy(BekijkenFinancieel)]
		StudentFinancieelTabblad = 53,

		[MpLabel("Student onderwijs tabblad")]
		[ImpliedBy(BekijkenStudievolg)]
		StudentOnderwijsTabblad = 54,

		[MpLabel("Student kenemerken tabblad")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		StudentKenemerkenTabblad = 55,

		[MpLabel("Vooropleidingen toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		VooropleidingenToevoegenVerwijderenWijzigen = 58,

		[MpLabel("Vooropleidingen verifieeren")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		VooropleidingenVerifieeren = 59,

		[MpLabel("Vooropleidingen bekijken")]
		[ImpliedBy(BekijkenInschrijvingen)]
		VooropleidingenBekijken = 60,

		[MpLabel("Aanmeldingen bekijken")]
		[ImpliedBy(BekijkenInschrijvingen)]
		AanmeldingenBekijken = 61,

		[MpLabel("Aanmeldingen toevoegen wijzigen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingenToevoegenWijzigen = 62,

		[MpLabel("Aanmelding definitief inschrijven")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingDefinitiefInschrijven = 63,

		[MpLabel("Aanmelding inschrijving intrekken")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingInschrijvingIntrekken = 64,

		[MpLabel("Aanmelding inschrijving mededelingen bekijken")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		AanmeldingInschrijvingMededelingenBekijken = 65,

		[MpLabel("Inschrijving uitschrijven")]
		[ImpliedBy(WijzigenUitschrijvingen)]
		InschrijvingUitschrijven = 66,

		[MpLabel("Inschrijving examen toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenExamens)]
		InschrijvingExamenToevoegenVerwijderenWijzigen = 67,

		[MpLabel("Aanmelding inschrijving mededeling versturen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		AanmeldingInschrijvingMededelingVersturen = 68,

		[MpLabel("Negatief binden studieadvies bekijken")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		NegatiefBindenStudieadviesBekijken = 69,

		[MpLabel("Negatief bindend studieadvies toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen = 70,

		[MpLabel("Blokkeer inschrijving toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		BlokkeerInschrijvingToevoegenWijzigenVerwijderen = 71,

		[MpLabel("Blokkeer inschrijving bekijken")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		BlokkeerInschrijvingBekijken = 72,

		[MpLabel("SM mutaties toevoegen wijzigen verwijderen")]
		[ImpliedBy(BeheerUitwisselingDuo)]
		SMMutatiesToevoegenWijzigenVerwijderen = 73,

		[MpLabel("SM mutaties bekijken")]
		[ImpliedBy(BekijkenUitwisselingDuo)]
		SMMutatiesBekijken = 74,

		[MpLabel("Studielinkberichten bekijken")]
		[ImpliedBy(BekijkenStudielinkberichten)]
		StudielinkberichtenBekijken = 75,

		[MpLabel("Studielinkberichten beheer")]
		[ImpliedBy(Superuser, BeheerStudielink)]
		StudielinkberichtenBeheer = 76,

		[MpLabel("Inschrijvingen bekijken")]
		[ImpliedBy(BekijkenInschrijvingen)]
		InschrijvingenBekijken = 77,

		[MpLabel("Financieel betalingsinformatie bekijken")]
		[ImpliedBy(BekijkenFinancieel)]
		FinancieelBetalingsinformatieBekijken = 79,

		[MpLabel("Financieel betalingsinformatie toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenFinancieel)]
		FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen = 80,

		[MpLabel("Student identificatie bekijken")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		StudentIdentificatieBekijken = 81,

		[MpLabel("Financieel clieop toevoegen verwijderen wijzigen")]
		[ImpliedBy(BeheerFinancieel)]
		FinancieelClieopToevoegenVerwijderenWijzigen = 82,

		[MpLabel("Financieel clieop bekijken")]
		[ImpliedBy(BeheerFinancieel)]
		FinancieelClieopBekijken = 83,

		[MpLabel("Niet reguliere inschrijvingen bekijken")]
		[ImpliedBy(BekijkenInschrijvingen)]
		NietReguliereInschrijvingenBekijken = 84,

		[MpLabel("Niet reguliere inschrijvingen wijzigen verwijderen")]
		[ImpliedBy(InvoerNietReguliereInschrijvingen)]
		NietReguliereInschrijvingenWijzigenVerwijderen = 85,

		[MpLabel("Financieel betalingen toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenFinancieel)]
		FinancieelBetalingenToevoegenVerwijderenWijzigen = 86,

		[MpLabel("Financieel collegegeld toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenFinancieel)]
		FinancieelCollegegeldToevoegenVerwijderenWijzigen = 87,

		[MpLabel("Financieel collegegeld bekijken")]
		[ImpliedBy(BekijkenFinancieel)]
		FinancieelCollegegeldBekijken = 88,

		[MpLabel("Financieel machting toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenFinancieel)]
		FinancieelMachtingToevoegenVerwijderenWijzigen = 89,

		[MpLabel("Financieel machtiging bekijken")]
		[ImpliedBy(BekijkenFinancieel)]
		FinancieelMachtigingBekijken = 90,

		[MpLabel("Niet reguliere inschrijvingen toevoegen")]
		[ImpliedBy(InvoerNietReguliereInschrijvingen)]
		NietReguliereInschrijvingenToevoegen = 91,

		[MpLabel("Student toevoegen")]
		[ImpliedBy(WijzigenCorrespondentieadressen, InvoerNietReguliereInschrijvingen)]
		StudentToevoegen = 92,

		[MpLabel("Student adres bekijken")]
		[ImpliedBy(BekijkenInschrijvingen)]
		StudentAdresBekijken = 93,

		[MpLabel("Student adres toevoegen")]
		[ImpliedBy(WijzigenCorrespondentieadressen, InvoerNietReguliereInschrijvingen)]
		StudentAdresToevoegen = 94,

		[MpLabel("Student adres wijzigen verwijderen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentAdresWijzigenVerwijderen = 95,

		[MpLabel("Aanmelding toelatingseisen accorderen")]
		[ImpliedBy(WijzigenAccorderenToelatingseisen)]
		AanmeldingToelatingseisenAccorderen = 96,

		[MpLabel("Student eisen tabblad")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		StudentEisenTabblad = 97,

		[MpLabel("Voorlopige toelating invoeren wijzigin verwijderen")]
		[ImpliedBy(WijzigenVoorlopigeToelating)]
		VoorlopigeToelatingInvoerenWijziginVerwijderen = 98,

		[MpLabel("Student kenmerk toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen, BeheerStudievolg)]
		StudentKenmerkToevoegenVerwijderenWijzigen = 99,

		[MpLabel("Organisatie financieel collegegeld tabel beheer")]
		[ImpliedBy(BeheerFinancieel)]
		OrganisatieFinancieelCollegegeldTabelBeheer = 100,

		[MpLabel("Organisatie onderwijs tab")]
		[ImpliedBy(BekijkenStudievolg)]
		OrganisatieOnderwijsTab = 101,

		[MpLabel("Examenstabblad")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		Examenstabblad = 102,

		[MpLabel("Uitschrijvingentabblad")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		Uitschrijvingentabblad = 103,

		[MpLabel("Student taaltoetstabblad")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		StudentTaaltoetstabblad = 104,

		[MpLabel("Webservice alle tabellen lezen")]
		WebserviceAlleTabellenLezen = 105,

		[MpLabel("Document templates")]
		[ImpliedBy(BeheerInschrijvingen, BeheerGetuigschriften)]
		DocumentTemplates = 106,

		[MpLabel("Document generatie")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen, WijzigenToevoegenAfdrukkenBbc, WijzigenCrm)]
		DocumentGeneratie = 107,

		[MpLabel("BBCs bekijken")]
		[ImpliedBy(BekijkenFinancieel)]
		BbcBekijken = 108,

		[MpLabel("BBCs toevoegen wijzigen")]
		[ImpliedBy(WijzigenToevoegenAfdrukkenBbc)]
		BbcToevoegenWijzigen = 109,

		[MpLabel("BBCs afdrukken")]
		[ImpliedBy(WijzigenToevoegenAfdrukkenBbc)]
		BbcAfdrukken = 110,

		[MpLabel("Student communicatie tabblad")]
		[ImpliedBy(Superuser, BekijkenInschrijvingenUitgebreid, WijzigenCrm)]
		StudentCommunicatieTabblad = 111,

		[MpLabel("Student communicatie toevoegen verwijderen wijzigen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen, WijzigenCrm)]
		StudentCommunicatieToevoegenVerwijderenWijzigen = 112,

		[MpLabel("Communicatie template toevoegen verwijderen wijzigen")]
		[ImpliedBy(Superuser, BeheerGetuigschriften)]
		CommunicatieTemplateToevoegenVerwijderenWijzigen = 113,

		[MpLabel("Volg onderwijs beheer alles")]
		[ImpliedBy(BeheerStudievolg)]
		VolgOnderwijsBeheerAlles = 114,

		[MpLabel("Volg onderwijs beheer beperkt")]
		[ImpliedBy(WijzigenStudievolg)]
		VolgOnderwijsBeheerBeperkt = 115,

		[MpLabel("Volg onderwijs inzien")]
		[ImpliedBy(BekijkenStudievolg)]
		VolgOnderwijsInzien = 116,

		[MpLabel("Bekijken kengetallen")]
		[ImpliedBy(BeheerInschrijvingen)]
		BekijkenKengetallen = 119,

		[MpLabel("Organisatietabblad en boom")]
		OrganisatietabbladEnBoom = 120,

		[MpLabel("Bekijken studievolg")]
		[ImpliedBy(WijzigenStudievolg, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding, WijzigenBsaStatus, WijzigenStudentdecaan)]
		BekijkenStudievolg = 121,

		[MpLabel("Bekijken studielinkberichten")]
		[ImpliedBy(BeheerStudielink, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenStudielinkberichten = 123,

		[MpLabel("Bekijken uitwisseling DUO")]
		[ImpliedBy(BeheerUitwisselingDuo)]
		BekijkenUitwisselingDuo = 124,

		[MpLabel("Wijzigen inschrijvingen en aanmeldingen")]
		[ImpliedBy(BeheerInschrijvingen)]
		WijzigenInschrijvingenEnAanmeldingen = 125,

		[MpLabel("Wijzigen uitschrijvingen")]
		[ImpliedBy(BeheerInschrijvingen)]
		WijzigenUitschrijvingen = 126,

		[MpLabel("Wijzigen examens")]
		[ImpliedBy(BeheerInschrijvingen, BeheerWaardepapierenSjablonen)]
		WijzigenExamens = 127,

		[MpLabel("Wijzigen correspondentieadressen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenCorrespondentieadressen = 128,

		[MpLabel("Invoer niet-reguliere inschrijvingen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		InvoerNietReguliereInschrijvingen = 129,

		[MpLabel("Wijzigen financieel")]
		[ImpliedBy(BeheerFinancieel)]
		WijzigenFinancieel = 132,

		[MpLabel("Wijzigen/ toevoegen/ afdrukken BBC's")]
		[ImpliedBy(WijzigenFinancieel)]
		WijzigenToevoegenAfdrukkenBbc = 133,

		[MpLabel("Beheer financieel")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerFinancieel = 134,

		[MpLabel("Beheer inschrijvingen")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerInschrijvingen = 135,

		[MpLabel("Wijzigen accorderen toelatingseisen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenAccorderenToelatingseisen = 136,

		[MpLabel("Wijzigen voorlopige toelating")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenVoorlopigeToelating = 137,

		[MpLabel("Superuser")]
		Superuser = 138,

		[MpLabel("Wijzigen studievolg")]
		[ImpliedBy(BeheerStudievolg)]
		WijzigenStudievolg = 139,

		[MpLabel("Beheer studievolg")]
		[ImpliedBy(Superuser)]
		BeheerStudievolg = 140,

		[MpLabel("Bekijken inschrijvingen")]
		[ImpliedBy(BekijkenStudievolg, BekijkenStudielinkberichten, BekijkenUitwisselingDuo, WijzigenCorrespondentieadressen, InvoerNietReguliereInschrijvingen, WijzigenAccorderenToelatingseisen, WijzigenVoorlopigeToelating, BekijkenInschrijvingenUitgebreid, BekijkenFinancieel, WijzigenAlumniNetwerk)]
		BekijkenInschrijvingen = 160,

		[MpLabel("Bekijken inschrijvingen uitgebreid")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen, WijzigenUitschrijvingen, WijzigenExamens, WijzigenStudentdecaan, WijzigenVerblijfsvergunning, WijzigenPasfoto, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenInschrijvingenUitgebreid = 161,

		[MpLabel("Bekijken financieel")]
		[ImpliedBy(WijzigenToevoegenAfdrukkenBbc, BekijkenFinancieelUitgebreid)]
		BekijkenFinancieel = 162,

		[MpLabel("Bekijken financieel uitgebreid")]
		[ImpliedBy(WijzigenFinancieel, WijzigenStudentdecaan, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenFinancieelUitgebreid = 163,

		[MpLabel("Beheer accounts en rollen")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerAccountsEnRollen = 165,

		[MpLabel("Beheer studielink")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerStudielink = 166,

		[MpLabel("Beheer uitwisseling DUO")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerUitwisselingDuo = 167,

		[MpLabel("Wijzigen CRM")]
		[ImpliedBy(BeheerInschrijvingen, WijzigenStudievolg, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		WijzigenCrm = 168,

		[MpLabel("Taken bekijken wijzigen")]
		[ImpliedBy(BeheerInschrijvingen)]
		TakenBekijkenWijzigen = 169,

		[MpLabel("Wijzigen BSA bijzondere omstandigheden/ studiebegeleiding")]
		[ImpliedBy(WijzigenStudievolg, BeheerBsaStudiebegeleiding)]
		WijzigenBsaBijzondereOmstandighedenStudiebegeleiding = 172,

		[MpLabel("Student communicatie inschrijving toevoegen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentCommunicatieInschrijvingToevoegen = 173,

		[MpLabel("Inschrijving notities bekijken")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		InschrijvingNotitiesBekijken = 174,

		[MpLabel("Inschrijving notities toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen, WijzigenUitschrijvingen, WijzigenExamens)]
		InschrijvingNotitiesToevoegenWijzigenVerwijderen = 175,

		[MpLabel("Financieel notities bekijken")]
		[ImpliedBy(BekijkenFinancieelUitgebreid)]
		FinancieelNotitiesBekijken = 176,

		[MpLabel("Financieel notities toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenFinancieel)]
		FinancieelNotitiesToevoegenWijzigenVerwijderen = 177,

		[MpLabel("Studiebegeleiding gespreksnotities bekijken")]
		[ImpliedBy(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		StudiebegeleidingGespreksnotitiesBekijken = 178,

		[MpLabel("Studiebegeleiding gespreksnotities toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		StudiebegeleidingGespreksnotitiesToevoegenWijzigenVerwijderen = 179,

		[MpLabel("Studievoortgang notities bekijken")]
		[ImpliedBy(WijzigenStudievolg, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		StudievoortgangNotitiesBekijken = 180,

		[MpLabel("Studievoortgang notities toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenStudievolg)]
		StudievoortgangNotitiesToevoegenWijzigenVerwijderen = 181,

		[MpLabel("BSA status bekijken")]
		[ImpliedBy(BekijkenStudievolg)]
		BsaStatusBekijken = 182,

		[MpLabel("BSA bijzondere omstandigheden toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding)]
		BsaBijzondereOmstandighedenToevoegenWijzigenVerwijderen = 183,

		[MpLabel("BSA ontheffing toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenBsaStatus)]
		BsaOntheffingToevoegenWijzigenVerwijderen = 184,

		[MpLabel("BSA definitief toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenBsaStatus)]
		BsaDefinitiefToevoegenWijzigenVerwijderen = 185,

		[MpLabel("Wijzigen BSA status")]
		[ImpliedBy(BeheerBsaStudiebegeleiding)]
		WijzigenBsaStatus = 186,

		[MpLabel("Communicatie bekijken")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		CommunicatieBekijken = 187,

		[MpLabel("Beheer BSA/ studiebegeleiding")]
		[ImpliedBy(Superuser)]
		BeheerBsaStudiebegeleiding = 188,

		[MpLabel("Decaan notities toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenStudentdecaan)]
		DecaanNotitiesToevoegenWijzigenVerwijderen = 189,

		[MpLabel("Decaan notities bekijken")]
		[ImpliedBy(WijzigenStudentdecaan)]
		DecaanNotitiesBekijken = 190,

		[MpLabel("Wijzigen studentdecaan")]
		[ImpliedBy(Superuser)]
		WijzigenStudentdecaan = 191,

		[MpLabel("Statische groepen aanmaken wijzigen verwijderen")]
		[ImpliedBy(BeheerInschrijvingen)]
		StatischeGroepenAanmakenWijzigenVerwijderen = 192,

		[MpLabel("Statische groepen gebruiken")]
		[ImpliedBy(BeheerInschrijvingen)]
		StatischeGroepenGebruiken = 193,

		[MpLabel("Alumni netwerk toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenAlumniNetwerk)]
		AlumniNetwerkToevoegenWijzigenVerwijderen = 194,

		[MpLabel("Wijzigen alumni-netwerk")]
		[ImpliedBy(Superuser)]
		WijzigenAlumniNetwerk = 195,

		[MpLabel("Student wijzig personalia beperkt")]
		[ImpliedBy(InvoerNietReguliereInschrijvingen)]
		StudentWijzigPersonaliaBeperkt = 196,

		[MpLabel("Wijzigen verblijfsvergunning")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenVerblijfsvergunning = 197,

		[MpLabel("Student wijzig verblijfsvergunning")]
		[ImpliedBy(WijzigenVerblijfsvergunning)]
		StudentWijzigVerblijfsvergunning = 198,

		[MpLabel("Wijzigen pasfoto")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		WijzigenPasfoto = 199,

		[MpLabel("Waardepapier templates")]
		[ImpliedBy(BeheerWaardepapierenSjablonen)]
		WaardepapierTemplates = 201,

		[MpLabel("Examen waardepapier generatie")]
		[ImpliedBy(WijzigenExamens)]
		ExamenWaardepapierGeneratie = 202,

		[MpLabel("Examen waardepapier bekijken")]
		[ImpliedBy(BekijkenInschrijvingenUitgebreid)]
		ExamenWaardepapierBekijken = 203,

		[MpLabel("Examencommissies beheer")]
		[ImpliedBy(WijzigenExamens, BeheerGetuigschriften)]
		ExamencommissiesBeheer = 204,

		[MpLabel("Beheer waardepapieren sjablonen")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerWaardepapierenSjablonen = 205,

		[MpLabel("Cursusaanbod toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenCursusaanbod)]
		CursusaanbodToevoegenWijzigenVerwijderen = 206,

		[MpLabel("Cursusaanbod bekijken")]
		[ImpliedBy(BekijkenCursusdeelnamesEnAanbod, WijzigenCursusaanbod)]
		CursusaanbodBekijken = 207,

		[MpLabel("Beheer cursusaanbod/ cursusdeelnames")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerCursusaanbodCursusdeelnames = 208,

		[MpLabel("Diploma supplement beheer")]
		[ImpliedBy(WijzigenExamens, BeheerGetuigschriften)]
		DiplomaSupplementBeheer = 209,

		[MpLabel("Cursusdeelnames bekijken")]
		[ImpliedBy(BekijkenCursusdeelnamesEnAanbod)]
		CursusdeelnamesBekijken = 210,

		[MpLabel("Cursusdeelname toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenCursusdeelnames)]
		CursusdeelnameToevoegenWijzigenVerwijderen = 211,

		[MpLabel("Wijzigen cursusdeelnames")]
		[ImpliedBy(Superuser)]
		WijzigenCursusdeelnames = 212,

		[MpLabel("Kenmerken toevoegen wijzigen verwijderen")]
		[ImpliedBy(BeheerInschrijvingen)]
		KenmerkenToevoegenWijzigenVerwijderen = 213,

		[MpLabel("Batches toevoegen wijzigen verwijderen")]
		[ImpliedBy(BeheerInschrijvingen)]
		BatchesToevoegenWijzigenVerwijderen = 214,

		[MpLabel("Studenten samenvoegen")]
		[ImpliedBy(BeheerInschrijvingen)]
		StudentenSamenvoegen = 215,

		[MpLabel("Taken van iedereen wijzigen verwijderen annuleren")]
		TakenVanIedereenWijzigenVerwijderenAnnuleren = 216,

		[MpLabel("Beheer getuigschriften")]
		[ImpliedBy(Superuser, Combi_BeheerAlles)]
		BeheerGetuigschriften = 217,

		[MpLabel("Bekijken cursusdeelnames en aanbod")]
		[ImpliedBy(WijzigenCursusdeelnames, Combi_BekijkInschrijvingFinancieelStudielinkCursus)]
		BekijkenCursusdeelnamesEnAanbod = 218,

		[MpLabel("Wijzigen cursusaanbod")]
		[ImpliedBy(BeheerCursusaanbodCursusdeelnames)]
		WijzigenCursusaanbod = 219,

		[MpLabel("Cursus stam tabellen toevoegen wijzigen verwijderen")]
		[ImpliedBy(BeheerCursusaanbodCursusdeelnames)]
		CursusStamTabellenToevoegenWijzigenVerwijderen = 220,

		[MpLabel("Cursussen tab")]
		[ImpliedBy(BekijkenCursusdeelnamesEnAanbod, WijzigenCursusaanbod)]
		CursussenTab = 221,

		[MpLabel("Student richting toevoegen wijzigen verwijderen")]
		[ImpliedBy(WijzigenInschrijvingenEnAanmeldingen)]
		StudentRichtingToevoegenWijzigenVerwijderen = 222,

		[MpLabel("COMBI: bekijk inschrijving/financieel/studielink/cursus")]
		Combi_BekijkInschrijvingFinancieelStudielinkCursus = 223,

		[MpLabel("COMBI: beheer alles")]
		Combi_BeheerAlles = 224,

		[MpLabel("Document generatie vooropleidingen")]
		DocumentGeneratieVooropleidingen = 225,
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
