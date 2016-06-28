USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllNPDocPagesDecrypted]    Script Date: 4/25/2016 8:04:08 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspgetAllRandomDocPagesDecrypted]

@DocPageEncrypted nvarchar(max) 
AS
 
BEGIN
	
	
	


 select CAST(CAST(N'' AS XML).value('(sql:variable("@DocPageEncrypted"))',
           'VARBINARY(MAX)') AS VARCHAR(MAX)) 
		   AS [DocPageContent]

		--

END

 





GO


