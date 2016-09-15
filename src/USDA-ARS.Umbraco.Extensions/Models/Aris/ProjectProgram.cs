using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
			public class ProjectProgram
			{
						[Column("NP_CODE")]
						public int NpCode { get; set; }

						[Column("short_desc")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string ShortDescription { get; set; }

						[Column("accn_no")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public int AccountNo { get; set; }

						[Column("prj_title")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string ProjectTitle { get; set; }

						[Column("prj_type")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string ProjectType { get; set; }

						[Column("Web_Label")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string WebLabel { get; set; }

						[Column("city")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string City { get; set; }

						[Column("stateabbr")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string StateAbbr { get; set; }

						[Column("perfname")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string FirstName { get; set; }

						[Column("perlname")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string LastName { get; set; }

						[Column("location")]
						[NullSetting(NullSetting = NullSettings.Null)]
						public string Location { get; set; }
			}
}

