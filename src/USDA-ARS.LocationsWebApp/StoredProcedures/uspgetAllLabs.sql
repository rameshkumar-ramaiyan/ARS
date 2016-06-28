USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllLabs]    Script Date: 2/5/2016 4:21:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE PROCEDURE [dbo].[uspgetAllLabs]
@ParentAreaModeCode int, 
@ParentCityModeCode int ,
@ParentResearchUnitModeCode int 
AS
 
 	
	BEGIN
		
select MODECODE_1 as 'Area Mode Code'
, MODECODE_2 as 'City and State Mode Code',
MODECODE_3 as 'Research Unit Mode Code',

cast (MODECODE_1 as varchar(2))+'-'+cast (MODECODE_2 as varchar(2))+'-'+
CASE 
	WHEN MODECODE_3  >=10 THEN cast (MODECODE_3 as varchar(2))
	ELSE '0'+ cast (MODECODE_3 as varchar(2)) 
	END 


+'-'+
CASE 
	WHEN MODECODE_4  >=10 THEN cast (MODECODE_4 as varchar(2))
	ELSE '0'+ cast (MODECODE_4 as varchar(2)) 
	END 
	as 'Mode Code'


,MODECODE_4_DESC as 'Lab' 
--,MODECODE_4 as 'Lab Mode Code',MODECODE_4_DESC as 'Lab'
from aris_public_web.dbo.REF_MODECODE 
where 
MODECODE_1=@ParentAreaModeCode and MODECODE_2 <> '1'
and MODECODE_2=@ParentCityModeCode
and MODECODE_3=@ParentResearchUnitModeCode and MODECODE_3 <> '2'
and MODECODE_4 is not null and MODECODE_4<>0 and MODECODE_4 <> '1'  and MODECODE_4 <>  '2'
AND STATUS_CODE = 'A' --status code active                  
	and  STATE_CODE is not null
	order by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_4,MODECODE_4_DESC
 
			
	END





GO

