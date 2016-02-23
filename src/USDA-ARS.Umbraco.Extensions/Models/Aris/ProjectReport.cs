using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace USDA_ARS.Umbraco.Extensions.Models.Aris
{
    public class ProjectReport
    {
        [Column("QUESTION_NO")]
        public int QuestionNo { get; set; }

        [Column("RESPONSE")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Response { get; set; }

        [Column("QUESTION")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Question { get; set; }

        [Column("VISUAL_QUESTION_NO")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string VisualQuestionNo { get; set; }

        [Column("Q3_HEADER_TEXT")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Q3HeaderText { get; set; }
    }
}

