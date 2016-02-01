USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllCities]    Script Date: 1/27/2016 5:01:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [dbo].[uspgetAllCities]
@ParentAreaModeCode int 
AS
 
 	BEGIN
		
select  MODECODE_1 as 'Area Mode Code', 
		cast (MODECODE_1 as varchar(2))+'-'+cast (MODECODE_2 as varchar(2))+'-00-00'as 'Area,City and State Mode Code',
		MODECODE_2_DESC  as 'City and State'
		--substring(MODECODE_2_DESC,0 ,		charindex(',',MODECODE_2_DESC )) as 'City and State'
		 ,STATE_CODE as 'State Code'
from [aris_public_web].dbo.REF_MODECODE 
where 
	MODECODE_1=@ParentAreaModeCode
	and	MODECODE_3=0 
	and	MODECODE_4=0 
	and modecode_2<>0 and modecode_2<>1
	
	AND STATUS_CODE = 'A' --status code active                  
	and  STATE_CODE is not null
	order by MODECODE_1,STATE_CODE,substring(MODECODE_2_DESC,0 ,charindex(',',MODECODE_2_DESC ))
			
	END






GO


