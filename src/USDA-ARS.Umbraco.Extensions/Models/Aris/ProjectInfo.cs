﻿using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectInfo
    {
        [Column("ACCN_NO")]
        public int AccountNo { get; set; }

        [Column("START_DATE")]
        public DateTime DateStart { get; set; }

        [Column("TERM_DATE")]
        public DateTime DateTermination { get; set; }

        [Column("PROJECT_START")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectStart { get; set; }

        [Column("PROJECT_END")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectEnd { get; set; }

        [Column("STATUS_CODE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ShortDescription { get; set; }

        [Column("PRJ_TITLE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectTitle { get; set; }

        [Column("AGREEMENT_CODE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string AgreementCode { get; set; }

        [Column("FY")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Year { get; set; }

        [Column("RESEARCH_FACILITIES")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ResearchFacilities { get; set; }

        [Column("DURATION")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Duration { get; set; }

        [Column("modecode_1")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ModeCode1 { get; set; }

        [Column("modecode_2")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ModeCode2 { get; set; }

        [Column("modecode_3")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ModeCode3 { get; set; }

        [Column("modecode_4")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ModeCode4 { get; set; }

        [Column("ProjectNumber")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectNumber { get; set; }

        [Column("PRJ_TYPE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string ProjectType { get; set; }

        [Column("objective")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Objective { get; set; }

        [Column("approach")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Approach { get; set; }

        [Column("country_code")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CountryCode { get; set; }

        [Column("country_desc")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string CountryDescription { get; set; }

        [Column("RECIPIENT_NAME")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string RecipientName { get; set; }

        [Column("CITY_NAME")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string City { get; set; }

        [Column("STATE_NAME")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string State { get; set; }
    }
}

