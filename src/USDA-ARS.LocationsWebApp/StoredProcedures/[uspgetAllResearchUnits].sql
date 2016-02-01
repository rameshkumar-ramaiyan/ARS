USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllResearchUnits]    Script Date: 1/29/2016 9:44:48 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






create PROCEDURE [dbo].[uspgetAllResearchUnits]
@ParentAreaModeCode int, 
@ParentCityModeCode int 

AS
 
 	
	BEGIN
		
select MODECODE_1 as 'Area Mode Code'
, MODECODE_2 as 'City and State Mode Code',
cast (MODECODE_1 as varchar(2))+'-'+cast (MODECODE_2 as varchar(2))+'-'+
CASE 
	WHEN MODECODE_3  >=10 THEN cast (MODECODE_3 as varchar(2))
	ELSE '0'+ cast (MODECODE_3 as varchar(2)) 
	END 


+'-00'as 'Research Unit Mode Code'

,MODECODE_3_DESC as 'Research Unit' 
--,MODECODE_4 as 'Lab Mode Code',MODECODE_4_DESC as 'Lab'
from aris_public_web.dbo.REF_MODECODE 
where 
MODECODE_1=@ParentAreaModeCode 
and MODECODE_2=@ParentCityModeCode and MODECODE_2 <> '1'
and MODECODE_3 is not null and MODECODE_3<>0 and MODECODE_3 <> '2'
and MODECODE_4=0 




AND STATUS_CODE = 'A' --status code active                  
	and  STATE_CODE is not null
	order by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_3_DESC
 
			
	END



GO


 