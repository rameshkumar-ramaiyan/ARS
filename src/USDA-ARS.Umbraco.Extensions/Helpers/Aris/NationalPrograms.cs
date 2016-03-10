using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Persistence;
using USDA_ARS.Umbraco.Extensions.Models.Aris;

namespace USDA_ARS.Umbraco.Extensions.Helpers.Aris
{
    public class NationalPrograms
    {
        public static NationalProgramLocation GetProgramLocationByModeCode(string modeCode)
        {
            modeCode = Helpers.ModeCodes.ModeCodeNoDashes(modeCode);

            NationalProgramLocation programLocation = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"select	modecode_1,
			                modecode_2,
			                modecode_2,
			                modecode_3,
			                modecode_4,
			                Web_Label,
			                MODECODE_1_DESC, 
			                MODECODE_2_DESC, 
			                MODECODE_3_DESC, 
			                MODECODE_4_DESC, 
			                RL_EMP_ID,
			                MISSION_STATEMENT,
			                HOMEPAGEURL,
			                ADD_LINE_1,
			                ADD_LINE_2,
			                city,
			                STATE_CODE,
			                POSTAL_CODE,
			                COUNTRY_CODE, 
			                state_code,
			                rl_email,	
			                rl_phone	
	                from 	V_LOCATIONS  
	                WHERE 	MODECODEconc = @modeCode
	                AND 	STATUS_CODE = 'a'";

            programLocation = db.Query<NationalProgramLocation>(sql, new { modeCode = modeCode }).FirstOrDefault();

            return programLocation;
        }


        public static List<NationalProgramLocation> GetProgramsByNpCodeForMap(string npCode)
        {
            List<NationalProgramLocation> programLocationList = null;

            var db = new Database("arisPublicWebDbDSN");

            string sql = @"SELECT 	distinct modecodes.state_code,  
				            modecodes.MODECODE_1, 
				            MODECODES.web_label, 
				            modecodes.MODECODE_2, 
				            modecodes.MODECODE_3,  
				            MODECODE_1_DESC, 
				            MODECODE_2_DESC, 
				            MODECODE_3_DESC, 
				            MODECODES.city, 
				            rtrim(state_CODE) stateabbr 
		            FROM 	V_CLEAN_PROJECTS		as PROJECTS, 
				            v_locations				as MODECODES
		            WHERE 	1=1
		            and		PROJECTS.ACCN_NO in 
							            (	select	distinct accn_no
								            from	A416_NATIONAL_PROGRAM A4NP
								            WHERE 	A4NP.NP_CODE = @npCode
								            and 	A4NP.np_type = 'n'
							            )
		            AND 	projects.MODECODE_1 = modecodes.MODECODE_1
		            AND 	projects.MODECODE_2 = modecodes.MODECODE_2
		            AND 	projects.MODECODE_3 = modecodes.MODECODE_3
		            AND 	projects.MODECODE_4 = modecodes.MODECODE_4
		            AND 	projects.MODECODE_2 <> '01'
		            AND 	MODECODES.STATUS_CODE = 'a'
		            AND 	left(modecodes.MODECODE_1, 2) > 05
		            ORDER BY modecodes.modecode_1, modecodes.modecode_2";

            programLocationList = db.Query<NationalProgramLocation>(sql, new { npCode = npCode }).ToList();

            return programLocationList;
        }
    }

    
}
