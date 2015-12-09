USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllAreas]    Script Date: 12/7/2015 4:03:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[uspgetAllAreas]
@ModeCode int = NULL
AS
 
 	BEGIN
		
select  MODECODE_1 as 'Area Mode Code',MODECODE_1_DESC As 'Area' from aris_public_webNew.dbo.REF_MODECODE where 
MODECODE_2=0 and MODECODE_3=0 and MODECODE_4=0 order by MODECODE_1
			
	END




GO


