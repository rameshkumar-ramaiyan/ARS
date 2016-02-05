USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllAreas]    Script Date: 2/5/2016 4:32:21 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[uspgetAllAreas]
@ModeCode int = NULL
AS
 
 	BEGIN
	select  cast (MODECODE_1 as varchar(2))+'-00-00-00'as 'Mode Code',MODECODE_1_DESC As 'Area' 
	
     
   
	--,STATE_CODE
from aris_public_webNew.dbo.REF_MODECODE 

where 
MODECODE_2=0 and MODECODE_3=0 and MODECODE_4=0 
AND STATUS_CODE = 'A' --status code active
AND  MODECODE_1 between 20 and 80                      
and  STATE_CODE is not null
 order by      MODECODE_1_DESC
		  --,  STATE_CODE                                                             

                    

			
----select  cast (MODECODE_1 as varchar(2))+'-00-00-00'as 'Area Mode Code',MODECODE_1_DESC As 'Area' 
----from aris_public_webNew.dbo.REF_MODECODE 
----where 
----MODECODE_2=0 and MODECODE_3=0 and MODECODE_4=0 
----AND STATUS_CODE = 'A' --status code active
------AND STATUS_CODE = 'A' AND NOT FACILITY_NAME IS NULL AND DEPT_CODE2 > 0 ---facility name and dept code 
----AND MODECODE_1%10=0 --areas ending with zero
----AND MODECODE_1<>0 --headquarters
----order by MODECODE_1_DESC
			
	END








GO

