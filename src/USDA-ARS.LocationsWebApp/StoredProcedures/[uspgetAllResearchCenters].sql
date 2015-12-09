USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllResearchCenters]    Script Date: 12/7/2015 4:05:35 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE PROCEDURE [dbo].[uspgetAllResearchCenters]
@ParentAreaModeCode int, 
@ParentCityModeCode int ,
@ParentResearchCenterModeCode int
AS
 
 	if(@ParentResearchCenterModeCode=0)
	BEGIN
		
	select MODECODE_1 as 'Area Mode Code', MODECODE_2 as 'City and State Mode Code', MODECODE_3 as 'Research Center Mode Code',MODECODE_3_DESC as 'Research Center' 
	from aris_public_webNew.dbo.REF_MODECODE 
	where 
	MODECODE_1=@ParentAreaModeCode and MODECODE_2=@ParentCityModeCode
	and MODECODE_3=0 and MODECODE_4=0 
	 order by MODECODE_1,MODECODE_2,MODECODE_3
			
	END



	else
	BEGIN
		
select MODECODE_1 as 'Area Mode Code', MODECODE_2 as 'City and State Mode Code', MODECODE_3 as 'Research Center Mode Code',MODECODE_3_DESC as 'Research Center' ,MODECODE_4 as 'Lab Mode Code',MODECODE_4_DESC as 'Lab'
from aris_public_webNew.dbo.REF_MODECODE 
where 
MODECODE_1=@ParentAreaModeCode and MODECODE_2=@ParentCityModeCode
and MODECODE_3=@ParentResearchCenterModeCode and MODECODE_4=0 
 order by MODECODE_1,MODECODE_2,MODECODE_3,MODECODE_4
			
	END


GO


