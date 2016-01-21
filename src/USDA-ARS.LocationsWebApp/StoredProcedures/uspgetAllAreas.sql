USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllAreas]    Script Date: 1/21/2016 1:29:05 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[uspgetAllAreas]
@ModeCode int = NULL
AS
 
 	BEGIN
	
select  cast (MODECODE_1 as varchar(2))+'-00-00-00'as 'Area Mode Code',MODECODE_1_DESC As 'Area' 
from aris_public_webNew.dbo.REF_MODECODE 
where 
MODECODE_2=0 and MODECODE_3=0 and MODECODE_4=0 
AND STATUS_CODE = 'A' --status code active
--AND STATUS_CODE = 'A' AND NOT FACILITY_NAME IS NULL AND DEPT_CODE2 > 0 ---facility name and dept code 
AND MODECODE_1%10=0 --areas ending with zero
AND MODECODE_1<>0 --headquarters
order by MODECODE_1
			
	END





GO


