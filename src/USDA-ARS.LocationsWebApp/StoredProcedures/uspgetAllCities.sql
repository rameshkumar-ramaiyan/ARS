USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllCities]    Script Date: 12/7/2015 4:04:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[uspgetAllCities]
@ParentAreaModeCode int 
AS
 
 	BEGIN
		
select MODECODE_1 as 'Area Mode Code', MODECODE_2 as 'City and State Mode Code',MODECODE_2_DESC as 'City and State' 
from aris_public_webNew.dbo.REF_MODECODE 
where 
MODECODE_1=@ParentAreaModeCode and
 MODECODE_3=0 and MODECODE_4=0 
 order by MODECODE_1,MODECODE_2
			
	END





GO


