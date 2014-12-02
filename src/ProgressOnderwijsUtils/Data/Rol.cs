﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using ProgressOnderwijsUtils.ToegangsRolInternal;

namespace ProgressOnderwijsUtils
{
    [OdsTable]
    public enum Rol
    {
#pragma warning disable 612
        [Obsolete, MpLabel("Studentadministratie alleen lezen, met accorderen toelatingseisen en kenmerken"),
         Implies(StudentadministratieAlleenInkijken, AanmeldingToelatingseisenAccorderen, WijzigenStudentKenmerken)]
        StudentadministratieAlleenLezenMetAccorderenToelatingseisenEnKenmerken = -24,

        [Obsolete, MpLabel("Financiële administratie alleen inkijken"), Implies(StudentFinancieelTabblad, FinancieelBetalingsinformatieBekijken,
            FinancieelCollegegeldBekijken, FinancieelMachtigingBekijken)]
        FinanciëleAdministratieAlleenInkijken = -23,

        [Obsolete, MpLabel("Financiële administratie"), Implies(FinanciëleAdministratieAlleenInkijken, FinancieelClieopToevoegenVerwijderenWijzigen,
            FinancieelClieopBekijken, FinancieelBetalingenToevoegenVerwijderenWijzigen, FinancieelCollegegeldToevoegenVerwijderenWijzigen,
            FinancieelMachtingToevoegenVerwijderenWijzigen)]
        FinanciëleAdministratie = -22,

        [Obsolete, MpLabel("Studentadministratie alleen inkijken"), Implies(StudentCommunicatieToevoegenVerwijderenWijzigen)]
        StudentadministratieAlleenInkijken = -21,

        [Obsolete, MpLabel("Studentadministratie"),
         Implies(StudentadministratieAlleenLezenMetAccorderenToelatingseisenEnKenmerken,
             StudentWijzigPersonalia, OpleidingenBekijken, BekijkenFinancieelUitgebreid,
             StudentIdentificatieToevoegenVerwijderenWijzigen,
             StudentIdentificatieVerifieeren, StudentoverledenToevoegenVerwijderenWijzigen,
             StudentPasfotoToevoegenVerwijderen,
             StudentOnderwijsTabblad,
             VooropleidingenToevoegenVerwijderenWijzigen, VooropleidingenVerifieeren,
             AanmeldingenToevoegenWijzigen,
             AanmeldingDefinitiefInschrijven, AanmeldingInschrijvingIntrekken,
             InschrijvingUitschrijven, InschrijvingExamenToevoegenVerwijderenWijzigen,
             BlokkeerInschrijvingBekijken,
             SMMutatiesBekijken,
             FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen,
             NietReguliereInschrijvingenWijzigenVerwijderen)]
        Studentadministratie = -18,

        [Obsolete, MpLabel("Studentadministratie beheerder"),
         Implies(FinanciëleAdministratie, StudentadministratieExtra,
             OrganisatieToevoegenVerwijderenWijzigen,
             OpleidingenToevoegenVerwijderenWijzigen,
             SMMutatiesToevoegenWijzigenVerwijderen, StudentEisenTabblad,
             OrganisatieFinancieelCollegegeldTabelBeheer,
             DocumentTemplates,
             TakenBekijkenWijzigen)]
        StudentadministratieBeheerder = -5,

        [Obsolete, MpLabel("Studentadministratie extra"),
         Implies(Studentadministratie, BekijkenKengetallen, StudentOverledenVerifieeren,
             NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen,
             BlokkeerInschrijvingToevoegenWijzigenVerwijderen)]
        StudentadministratieExtra = -4,
#pragma warning restore 612
        [MpLabel("Student bekijk personalia")]
        StudentBekijkPersonalia = 2,

        [MpLabel("Organisatie toevoegen verwijderen wijzigen")]
        OrganisatieToevoegenVerwijderenWijzigen = 4,

        [MpLabel("Student wijzig personalia")]
        StudentWijzigPersonalia = 32,

        [MpLabel("Opleidingen bekijken"), Implies(NietReguliereOpleidingenBekijken)]
        OpleidingenBekijken = 35,

        [MpLabel("Opleidingen toevoegen verwijderen wijzigen"), Implies(Rol.NietReguliereOpleidingenWijzigen)]
        OpleidingenToevoegenVerwijderenWijzigen = 36,

        [MpLabel("Student tabblad")]
        StudentTabblad = 38,

        [MpLabel("Student identificatie toevoegen verwijderen wijzigen")]
        StudentIdentificatieToevoegenVerwijderenWijzigen = 46,

        [MpLabel("Student identificatie verifieeren")]
        StudentIdentificatieVerifieeren = 47,

        [MpLabel("Studentoverleden toevoegen verwijderen wijzigen")]
        StudentoverledenToevoegenVerwijderenWijzigen = 48,

        [MpLabel("Student overleden verifieeren")]
        StudentOverledenVerifieeren = 49,

        [MpLabel("Student pasfoto toevoegen verwijderen")]
        StudentPasfotoToevoegenVerwijderen = 50,

        [MpLabel("Student persoon tabblad")]
        StudentPersoonTabblad = 51,

        [MpLabel("Student inschrijvingen tabblad")]
        StudentInschrijvingenTabblad = 52,

        [MpLabel("Student financieel tabblad")]
        StudentFinancieelTabblad = 53,

        [MpLabel("Student onderwijs tabblad")]
        StudentOnderwijsTabblad = 54,

        [MpLabel("Student kenemerken tabblad")]
        StudentKenmerkenTabblad = 55,

        [MpLabel("Vooropleidingen toevoegen verwijderen wijzigen")]
        VooropleidingenToevoegenVerwijderenWijzigen = 58,

        [MpLabel("Vooropleidingen verifieeren")]
        VooropleidingenVerifieeren = 59,

        [MpLabel("Vooropleidingen bekijken")]
        VooropleidingenBekijken = 60,

        [MpLabel("Aanmeldingen bekijken")]
        AanmeldingenBekijken = 61,

        [MpLabel("Aanmeldingen toevoegen wijzigen")]
        AanmeldingenToevoegenWijzigen = 62,

        [MpLabel("Aanmelding definitief inschrijven")]
        AanmeldingDefinitiefInschrijven = 63,

        [MpLabel("Aanmelding inschrijving intrekken")]
        AanmeldingInschrijvingIntrekken = 64,

        [MpLabel("Inschrijving uitschrijven")]
        InschrijvingUitschrijven = 66,

        [MpLabel("Inschrijving examen toevoegen verwijderen wijzigen")]
        InschrijvingExamenToevoegenVerwijderenWijzigen = 67,

        [MpLabel("Negatief binden studieadvies bekijken")]
        NegatiefBindenStudieadviesBekijken = 69,

        [MpLabel("Negatief bindend studieadvies toevoegen wijzigen verwijderen")]
        NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen = 70,

        [MpLabel("Blokkeer inschrijving toevoegen wijzigen verwijderen")]
        BlokkeerInschrijvingToevoegenWijzigenVerwijderen = 71,

        [MpLabel("Blokkeer inschrijving bekijken")]
        BlokkeerInschrijvingBekijken = 72,

        [MpLabel("SM mutaties toevoegen wijzigen verwijderen")]
        SMMutatiesToevoegenWijzigenVerwijderen = 73,

        [MpLabel("SM mutaties bekijken")]
        SMMutatiesBekijken = 74,

        [MpLabel("Studielinkberichten bekijken")]
        StudielinkberichtenBekijken = 75,

        [MpLabel("Studielinkberichten beheer")]
        StudielinkberichtenBeheer = 76,

        [MpLabel("Inschrijvingen bekijken")]
        InschrijvingenBekijken = 77,

        [MpLabel("Financieel betalingsinformatie bekijken")]
        FinancieelBetalingsinformatieBekijken = 79,

        [MpLabel("Financieel betalingsinformatie toevoegen wijzigen verwijderen")]
        FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen = 80,

        [MpLabel("Student identificatie bekijken")]
        StudentIdentificatieBekijken = 81,

        [MpLabel("Financieel clieop toevoegen verwijderen wijzigen")]
        FinancieelClieopToevoegenVerwijderenWijzigen = 82,

        [MpLabel("Financieel clieop bekijken")]
        FinancieelClieopBekijken = 83,

        [MpLabel("Niet reguliere inschrijvingen bekijken")]
        NietReguliereInschrijvingenBekijken = 84,

        [MpLabel("Niet reguliere inschrijvingen wijzigen verwijderen")]
        NietReguliereInschrijvingenWijzigenVerwijderen = 85,

        [MpLabel("Financieel betalingen toevoegen verwijderen wijzigen")]
        FinancieelBetalingenToevoegenVerwijderenWijzigen = 86,

        [MpLabel("Financieel collegegeld toevoegen verwijderen wijzigen")]
        FinancieelCollegegeldToevoegenVerwijderenWijzigen = 87,

        [MpLabel("Financieel collegegeld bekijken")]
        FinancieelCollegegeldBekijken = 88,

        [MpLabel("Financieel machting toevoegen verwijderen wijzigen")]
        FinancieelMachtingToevoegenVerwijderenWijzigen = 89,

        [MpLabel("Financieel machtiging bekijken")]
        FinancieelMachtigingBekijken = 90,

        [MpLabel("Niet reguliere inschrijvingen toevoegen")]
        NietReguliereInschrijvingenToevoegen = 91,

        [MpLabel("Student toevoegen")]
        StudentToevoegen = 92,

        [MpLabel("Student adres bekijken")]
        StudentAdresBekijken = 93,

        [MpLabel("Student adres toevoegen")]
        StudentAdresWijzigenBeperkt = 94,

        [MpLabel("Student adres wijzigen verwijderen")]
        StudentAdresWijzigenVerwijderen = 95,

        [MpLabel("Aanmelding toelatingseisen accorderen")]
        AanmeldingToelatingseisenAccorderen = 96,

        [MpLabel("Student eisen tabblad")]
        StudentEisenTabblad = 97,

        [MpLabel("Wijzigen Kenmerken"), Toekenbaar]
        WijzigenStudentKenmerken = 99,

        [MpLabel("Organisatie financieel collegegeld tabel beheer")]
        OrganisatieFinancieelCollegegeldTabelBeheer = 100,

        [MpLabel("Examenstabblad")]
        Examenstabblad = 102,

        [MpLabel("Uitschrijvingentabblad")]
        Uitschrijvingentabblad = 103,

        [MpLabel("Document templates")]
        DocumentTemplates = 106,

        [MpLabel("Document generatie")]
        DocumentGeneratie = 107,

        [MpLabel("BBCs bekijken")]
        BbcBekijken = 108,

        [MpLabel("BBCs toevoegen wijzigen")]
        BbcToevoegenWijzigen = 109,

        [MpLabel("BBCs afdrukken")]
        BbcAfdrukken = 110,

        [MpLabel("Student communicatie tabblad")]
        StudentCommunicatieTabblad = 111,

        [MpLabel("Student communicatie toevoegen verwijderen wijzigen")]
        StudentCommunicatieToevoegenVerwijderenWijzigen = 112,

        [MpLabel("Communicatie template toevoegen verwijderen wijzigen")]
        CommunicatieTemplateToevoegenVerwijderenWijzigen = 113,

        [MpLabel("Volg onderwijs beheer alles")]
        VolgOnderwijsBeheerAlles = 114,

        [MpLabel("Volg onderwijs beheer beperkt"),
         Implies(VolgOnderwijsCijferlijst)]
        VolgOnderwijsBeheerBeperkt = 115,

        [MpLabel("Genereren cijferlijsten voor bullen")]
        VolgOnderwijsCijferlijst = 244,

        [MpLabel("Volg onderwijs inzien")]
        VolgOnderwijsInzien = 116,

        [MpLabel("Bekijken kengetallen"), Toekenbaar]
        BekijkenKengetallen = 119,

        [MpLabel("Bekijken studievolg"), Toekenbaar,
         Implies(StudentOnderwijsTabblad, VolgOnderwijsInzien, BsaStatusBekijken, BekijkenInschrijvingen)]
        BekijkenStudievolg = 121,

        [MpLabel("Bekijken studielinkberichten"), Toekenbaar,
         Implies(StudielinkberichtenBekijken, BekijkenInschrijvingen)]
        BekijkenStudielinkberichten = 123,

        [MpLabel("Bekijken uitwisseling DUO"), Toekenbaar,
         Implies(SMMutatiesBekijken, BekijkenInschrijvingen)]
        BekijkenUitwisselingDuo = 124,

        [MpLabel("Wijzigen inschrijvingen en aanmeldingen"), Toekenbaar,
         Implies(StudentWijzigPersonalia, StudentIdentificatieToevoegenVerwijderenWijzigen,
             StudentIdentificatieVerifieeren, StudentoverledenToevoegenVerwijderenWijzigen,
             StudentOverledenVerifieeren, VooropleidingenToevoegenVerwijderenWijzigen, VooropleidingenVerifieeren,
             AanmeldingenToevoegenWijzigen, AanmeldingDefinitiefInschrijven, AanmeldingInschrijvingIntrekken,
             NegatiefBindendStudieadviesToevoegenWijzigenVerwijderen,
             BlokkeerInschrijvingToevoegenWijzigenVerwijderen, StudentAdresWijzigenVerwijderen,
             WijzigenStudentKenmerken, DocumentGeneratie,
             StudentCommunicatieToevoegenVerwijderenWijzigen, WijzigenCorrespondentieadressen,
             InvoerNietReguliereInschrijvingen, WijzigenAccorderenToelatingseisen,
             WijzigenVerblijfsvergunningVnummer, WijzigenPasfoto,
             StudentRichtingToevoegenWijzigenVerwijderen, WijzigenVoorlopigeToelating, WijzigenStudentBatchRegel,
             WijzigenCommunicatieAanmeldingInschrijvingUitschrijvingExamen, WijzigenMatching)]
        WijzigenInschrijvingenEnAanmeldingen = 125,

        [MpLabel("Wijzigen uitschrijvingen"), Toekenbaar,
         Implies(InschrijvingUitschrijven, BekijkenInschrijvingenUitgebreid
             )]
        WijzigenUitschrijvingen = 126,

        [MpLabel("Wijzigen examens"), Toekenbaar,
         Implies(InschrijvingExamenToevoegenVerwijderenWijzigen, BekijkenInschrijvingenUitgebreid,
             ExamenWaardepapierGeneratie,
             ExamencommissiesBeheer, DiplomaSupplementBeheer)]
        WijzigenExamens = 127,

        [MpLabel("Wijzigen correspondentieadressen"), Toekenbaar,
         Implies(StudentToevoegen, StudentAdresWijzigenBeperkt, BekijkenInschrijvingen)]
        WijzigenCorrespondentieadressen = 128,

        [MpLabel("Invoer niet-reguliere inschrijvingen"), Toekenbaar,
         Implies(NietReguliereInschrijvingenWijzigenVerwijderen, NietReguliereInschrijvingenToevoegen, StudentToevoegen,
             StudentAdresWijzigenBeperkt, StudentWijzigPersonaliaBeperkt, BekijkenInschrijvingen)]
        InvoerNietReguliereInschrijvingen = 129,

        [MpLabel("Wijzigen financieel"), Toekenbaar,
         Implies(FinancieelBetalingsinformatieToevoegenWijzigenVerwijderen,
             FinancieelBetalingenToevoegenVerwijderenWijzigen, FinancieelCollegegeldToevoegenVerwijderenWijzigen,
             FinancieelMachtingToevoegenVerwijderenWijzigen, WijzigenToevoegenAfdrukkenBbc,
             BekijkenFinancieelUitgebreid,
             WijzigenCommunicatieFinancieel)]
        WijzigenFinancieel = 132,

        [MpLabel("Wijzigen/ toevoegen/ afdrukken BBC's"), Toekenbaar,
         Implies(DocumentGeneratie, BbcToevoegenWijzigen, BbcAfdrukken, BekijkenFinancieel)]
        WijzigenToevoegenAfdrukkenBbc = 133,

        [MpLabel("Beheer financieel"), Toekenbaar,
         Implies(FinancieelClieopToevoegenVerwijderenWijzigen, FinancieelClieopBekijken, OrganisatieFinancieelCollegegeldTabelBeheer, WijzigenFinancieel)]
        BeheerFinancieel = 134,

        [MpLabel("Beheer inschrijvingen"), Toekenbaar,
         Implies(OrganisatieToevoegenVerwijderenWijzigen, OpleidingenBekijken, OpleidingenToevoegenVerwijderenWijzigen, DocumentTemplates,
             BekijkenKengetallen, WijzigenInschrijvingenEnAanmeldingen, WijzigenUitschrijvingen, WijzigenExamens, WijzigenCrm, TakenBekijkenWijzigen,
             StatischeGroepenAanmakenWijzigenVerwijderen,
             StatischeGroepenGebruiken, KenmerkenToevoegenWijzigenVerwijderen, BatchesToevoegenWijzigenVerwijderen, StudentenSamenvoegen)]
        BeheerInschrijvingen = 135,

        [MpLabel("Wijzigen accorderen toelatingseisen"), Toekenbaar, Implies(AanmeldingToelatingseisenAccorderen, BekijkenInschrijvingen)]
        WijzigenAccorderenToelatingseisen = 136,

        [MpLabel("Superuser"), Implies(Combi_BeheerAlles, BeheerStudievolg, BeheerBsaStudiebegeleiding, WijzigenStudentdecaan,
            WijzigenAlumniNetwerk, WijzigenDocumentenVooropleidingen, RapportenTabblad, CommunicatieTemplateToevoegenVerwijderenWijzigen, Student, WijzigenGroepen,
            Combi_FontysBeheerFO, Combi_FontysMuteerFO
            , StudiebegeleidingNotities, StudievoortgangNotities, DecaanNotities, FinancieelNotities, InschrijvingNotities, StudentDecanaatNotities, PsycholoogNotities
            )]
        Superuser = 138,

        [MpLabel("Wijzigen studievolg"), Toekenbaar,
         Implies(VolgOnderwijsBeheerBeperkt, WijzigenBsaBijzondereOmstandighedenStudiebegeleiding,
             WijzigenCommunicatieStudievolg)]
        WijzigenStudievolg = 139,

        [MpLabel("Beheer studievolg"), Toekenbaar, Implies(WijzigenStudentKenmerken, VolgOnderwijsBeheerAlles, WijzigenStudievolg)]
        BeheerStudievolg = 140,

        [MpLabel("Bekijken inschrijvingen"), Toekenbaar,
         Implies(BekijkenStudentBasis, StudentInschrijvingenTabblad, VooropleidingenBekijken,
             AanmeldingenBekijken, InschrijvingenBekijken, Examenstabblad, NietReguliereInschrijvingenBekijken)]
        BekijkenInschrijvingen = 160,

        [MpLabel("Bekijken inschrijvingen uitgebreid"), Toekenbaar,
         Implies(StudentKenmerkenTabblad, NegatiefBindenStudieadviesBekijken, BlokkeerInschrijvingBekijken, StudentIdentificatieBekijken, StudentEisenTabblad,
             Uitschrijvingentabblad, StudentCommunicatieTabblad, BekijkenInschrijvingen, ExamenWaardepapierBekijken)]
        BekijkenInschrijvingenUitgebreid = 161,

        [MpLabel("Bekijken financieel"), Toekenbaar, Implies(StudentFinancieelTabblad, FinancieelBetalingsinformatieBekijken, FinancieelCollegegeldBekijken,
            FinancieelMachtigingBekijken, BbcBekijken, BekijkenInschrijvingen)]
        BekijkenFinancieel = 162,

        [MpLabel("Bekijken financieel uitgebreid"), Toekenbaar, Implies(BekijkenFinancieel)]
        BekijkenFinancieelUitgebreid = 163,

        [MpLabel("Beheer accounts en rollen"), Toekenbaar]
        BeheerAccountsEnRollen = 165,

        [MpLabel("Beheer studielink"), Toekenbaar, Implies(StudielinkberichtenBeheer, BekijkenStudielinkberichten)]
        BeheerStudielink = 166,

        [MpLabel("Beheer uitwisseling DUO"), Toekenbaar, Implies(SMMutatiesToevoegenWijzigenVerwijderen, BekijkenUitwisselingDuo)]
        BeheerUitwisselingDuo = 167,

        [MpLabel("Wijzigen CRM"), Toekenbaar, Implies(DocumentGeneratie, StudentCommunicatieTabblad, StudentCommunicatieToevoegenVerwijderenWijzigen)]
        WijzigenCrm = 168,

        [MpLabel("Taken bekijken wijzigen")]
        TakenBekijkenWijzigen = 169,

        [MpLabel("Wijzigen BSA bijzondere omstandigheden/ studiebegeleiding"), Toekenbaar, Implies(BekijkenStudievolg, WijzigenCrm,
            BsaBijzondereOmstandighedenToevoegenWijzigenVerwijderen)]
        WijzigenBsaBijzondereOmstandighedenStudiebegeleiding = 172,

        [MpLabel("Inschrijving notities bekijken")]
        InschrijvingNotitiesAlleenBekijken = 174,

        [MpLabel("Inschrijving notities toevoegen wijzigen verwijderen"), Toekenbaar, Implies(InschrijvingNotitiesAlleenBekijken)]
        InschrijvingNotities = 175,

        [MpLabel("Financieel notities bekijken")]
        FinancieelNotitiesAlleenBekijken = 176,

        [MpLabel("Financieel notities toevoegen wijzigen verwijderen"), Toekenbaar, Implies(FinancieelNotitiesAlleenBekijken)]
        FinancieelNotities = 177,

        [MpLabel("Studiebegeleiding gespreksnotities bekijken")]
        StudiebegeleidingNotitiesAlleenBekijken = 178,

        [MpLabel("Studiebegeleiding gespreksnotities toevoegen wijzigen verwijderen"), Toekenbaar, Implies(StudiebegeleidingNotitiesAlleenBekijken)]
        StudiebegeleidingNotities = 179,

        [MpLabel("Studievoortgang notities toevoegen wijzigen verwijderen"), Toekenbaar, Implies(StudievoortgangNotitiesAlleenBekijken)]
        StudievoortgangNotities = 181,

        [MpLabel("Studentdecanaatnotities bekijken")]
        StudentDecanaatNotitiesAlleenBekijken = 245,

        [MpLabel("Studentdecanaatnotities wijzigen"), Toekenbaar, Implies(StudentDecanaatNotitiesAlleenBekijken)]
        StudentDecanaatNotities = 246,

        [MpLabel("Psycholoognotities bekijken")]
        PsycholoogNotitiesAlleenBekijken = 247,

        [MpLabel("Psycholoognotities wijzigen"), Toekenbaar, Implies(PsycholoogNotitiesAlleenBekijken)]
        PsycholoogNotities = 248,

        [MpLabel("BSA status bekijken")]
        BsaStatusBekijken = 182,

        [MpLabel("BSA bijzondere omstandigheden toevoegen wijzigen verwijderen")]
        BsaBijzondereOmstandighedenToevoegenWijzigenVerwijderen = 183,

        [MpLabel("BSA ontheffing toevoegen wijzigen verwijderen")]
        BsaOntheffingToevoegenWijzigenVerwijderen = 184,

        [MpLabel("Wijzigen BSA status"), Toekenbaar, Implies(BekijkenStudievolg, BsaOntheffingToevoegenWijzigenVerwijderen)]
        WijzigenBsaStatus = 186,

        [MpLabel("Beheer BSA/ studiebegeleiding"), Toekenbaar, Implies(WijzigenBsaBijzondereOmstandighedenStudiebegeleiding, WijzigenBsaStatus)]
        BeheerBsaStudiebegeleiding = 188,

        [MpLabel("Decaan notities toevoegen wijzigen verwijderen"), Toekenbaar, Implies(DecaanNotitiesAlleenBekijken)]
        DecaanNotities = 189,

        [MpLabel("Decaan notities bekijken")]
        DecaanNotitiesAlleenBekijken = 190,

        [MpLabel("Wijzigen studentdecaan"), Toekenbaar, Implies(BekijkenStudievolg, BekijkenInschrijvingenUitgebreid, BekijkenFinancieelUitgebreid)]
        WijzigenStudentdecaan = 191,

        [MpLabel("Statische groepen aanmaken wijzigen verwijderen")]
        StatischeGroepenAanmakenWijzigenVerwijderen = 192,

        [MpLabel("Statische groepen gebruiken")]
        StatischeGroepenGebruiken = 193,

        [MpLabel("Alumni netwerk toevoegen wijzigen verwijderen")]
        AlumniNetwerkToevoegenWijzigenVerwijderen = 194,

        [MpLabel("Wijzigen alumni-netwerk"), Toekenbaar,
         Implies(BekijkenInschrijvingen, AlumniNetwerkToevoegenWijzigenVerwijderen)]
        WijzigenAlumniNetwerk = 195,

        [MpLabel("Student wijzig personalia beperkt")]
        StudentWijzigPersonaliaBeperkt = 196,

        [MpLabel("Wijzigen verblijfsvergunning/Vnummer"), Toekenbaar,
         Implies(BekijkenInschrijvingenUitgebreid, StudentWijzigVerblijfsvergunning, StudentWijzigPersonaliaBeperkt)]
        WijzigenVerblijfsvergunningVnummer = 197,

        [MpLabel("Student wijzig verblijfsvergunning")]
        StudentWijzigVerblijfsvergunning = 198,

        [MpLabel("Wijzigen pasfoto"), Toekenbaar,
         Implies(StudentPasfotoToevoegenVerwijderen, BekijkenInschrijvingenUitgebreid)]
        WijzigenPasfoto = 199,

        [MpLabel("Waardepapier templates")]
        WaardepapierTemplates = 201,

        [MpLabel("Examen waardepapier generatie")]
        ExamenWaardepapierGeneratie = 202,

        [MpLabel("Examen waardepapier bekijken")]
        ExamenWaardepapierBekijken = 203,

        [MpLabel("Examencommissies beheer")]
        ExamencommissiesBeheer = 204,

        [MpLabel("Beheer waardepapieren sjablonen"), Toekenbaar, Implies(WijzigenExamens, WaardepapierTemplates)]
        BeheerWaardepapierenSjablonen = 205,

        [MpLabel("Cursusaanbod toevoegen wijzigen verwijderen")]
        CursusaanbodToevoegenWijzigenVerwijderen = 206,

        [MpLabel("Cursusaanbod bekijken")]
        CursusaanbodBekijken = 207,

        [MpLabel("Beheer cursusaanbod/ cursusdeelnames"), Toekenbaar, Implies(WijzigenCursusaanbod, CursusStamTabellenToevoegenWijzigenVerwijderen)]
        BeheerCursusaanbodCursusdeelnames = 208,

        [MpLabel("Diploma supplement beheer")]
        DiplomaSupplementBeheer = 209,

        [MpLabel("Cursusdeelnames bekijken")]
        CursusdeelnamesBekijken = 210,

        [MpLabel("Cursusdeelname toevoegen wijzigen verwijderen")]
        CursusdeelnameToevoegenWijzigenVerwijderen = 211,

        [MpLabel("Wijzigen cursusdeelnames"), Toekenbaar, Implies(CursusdeelnameToevoegenWijzigenVerwijderen, BekijkenCursusdeelnamesEnAanbod)]
        WijzigenCursusdeelnames = 212,

        [MpLabel("Kenmerken toevoegen wijzigen verwijderen")]
        KenmerkenToevoegenWijzigenVerwijderen = 213,

        [MpLabel("Batches toevoegen wijzigen verwijderen")]
        BatchesToevoegenWijzigenVerwijderen = 214,

        [MpLabel("Studenten samenvoegen")]
        StudentenSamenvoegen = 215,

        [MpLabel("Beheer getuigschriften"), Toekenbaar, Implies(OpleidingenBekijken, ExamencommissiesBeheer, DiplomaSupplementBeheer)]
        BeheerGetuigschriften = 217,

        [MpLabel("Bekijken cursusdeelnames en aanbod"), Toekenbaar, Implies(CursusaanbodBekijken, CursusdeelnamesBekijken, CursussenTab)]
        BekijkenCursusdeelnamesEnAanbod = 218,

        [MpLabel("Wijzigen cursusaanbod"), Toekenbaar, Implies(CursusaanbodToevoegenWijzigenVerwijderen, CursusaanbodBekijken, CursussenTab)]
        WijzigenCursusaanbod = 219,

        [MpLabel("Cursus stam tabellen toevoegen wijzigen verwijderen")]
        CursusStamTabellenToevoegenWijzigenVerwijderen = 220,

        [MpLabel("Cursussen tab")]
        CursussenTab = 221,

        [MpLabel("Student richting toevoegen wijzigen verwijderen")]
        StudentRichtingToevoegenWijzigenVerwijderen = 222,

        [MpLabel("COMBI: bekijk inschrijving/financieel/studielink/cursus"), Toekenbaar, Implies(BekijkenStudielinkberichten, BekijkenInschrijvingenUitgebreid,
            BekijkenFinancieelUitgebreid, BekijkenCursusdeelnamesEnAanbod)]
        Combi_BekijkInschrijvingFinancieelStudielinkCursus = 223,

        [MpLabel("COMBI: beheer alles"), Toekenbaar, Implies(BeheerFinancieel, BeheerInschrijvingen, BeheerAccountsEnRollen, BeheerStudielink,
            BeheerUitwisselingDuo, BeheerWaardepapierenSjablonen, BeheerCursusaanbodCursusdeelnames, BeheerGetuigschriften)]
        Combi_BeheerAlles = 224,

        [MpLabel("Wijzigen documenten vooropleidingen"), Toekenbaar, Implies(Rol.BekijkenInschrijvingen, Rol.StudentCommunicatieTabblad)]
        WijzigenDocumentenVooropleidingen = 225,

        [MpLabel("COMBI: Fontys Beheer FO"), Toekenbaar, Implies(Combi_BekijkInschrijvingFinancieelStudielinkCursus, BeheerGetuigschriften,
            Rol.StatischeGroepenAanmakenWijzigenVerwijderen, StudentRichtingToevoegenWijzigenVerwijderen)]
        Combi_FontysBeheerFO = 226,

        [MpLabel("COMBI: Fontys Muteer FO"), Toekenbaar, Implies(Combi_BekijkInschrijvingFinancieelStudielinkCursus, WijzigenCursusdeelnames,
            WijzigenExamens, StatischeGroepenAanmakenWijzigenVerwijderen, StudentRichtingToevoegenWijzigenVerwijderen)]
        Combi_FontysMuteerFO = 227,

        [MpLabel("Wijzigen voorlopige toelating"), Toekenbaar, Implies(BekijkenInschrijvingenUitgebreid)]
        WijzigenVoorlopigeToelating = 228,

        [MpLabel("Wijzigen batchregels student"), Toekenbaar, Implies(BekijkenInschrijvingenUitgebreid)]
        WijzigenStudentBatchRegel = 229,

        [MpLabel("Student"), MpTooltip("Virtuele rol voor studenten die extern inloggen")]
        Student = 230,

        [MpLabel("Wijzigen/toevoegen communicatie aanmelding/inschrijving/uitschrijving/examen"), Toekenbaar]
        WijzigenCommunicatieAanmeldingInschrijvingUitschrijvingExamen = 231,

        [MpLabel("Wijzigen/toevoegen communicatie studievolg")]
        WijzigenCommunicatieStudievolg = 232,

        [MpLabel("Wijzigen/toevoegen communicatie financieel")]
        WijzigenCommunicatieFinancieel = 233,

        [MpLabel("Wijzigen/toevoegen groepen"), Toekenbaar, Implies(Rol.StatischeGroepenAanmakenWijzigenVerwijderen)]
        WijzigenGroepen = 234,

        [MpLabel("Wijzigen Matching"), Toekenbaar, Implies(Rol.BekijkenInschrijvingen)]
        WijzigenMatching = 235,

        [MpLabel("Bekijken student basis"),
         Implies(StudentTabblad, StudentBekijkPersonalia, StudentPersoonTabblad, StudentAdresBekijken)]
        BekijkenStudentBasis = 236,

        [MpLabel("Niet-reguliere opleidingen bekijken")]
        NietReguliereOpleidingenBekijken = 240,

        [MpLabel("Niet-reguliere opleidingen wijzigen"), Implies(NietReguliereOpleidingenBekijken)]
        NietReguliereOpleidingenWijzigen = 241,

        [MpLabel("Studievoortgang notities bekijken")]
        StudievoortgangNotitiesAlleenBekijken = 242,

        [MpLabel("Rapporten bekijken"), Toekenbaar]
        RapportenTabblad = 243,

        [Obsolete("Dit id overnemen voor een nieuwe rol, en dan hier een ophogen; niet extern gebruiken", true), UsedImplicitly]
        META_EerstVolgendVrijRolId = 249,
    }
}
