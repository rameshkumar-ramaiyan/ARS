USE [aris_public_web]
GO

/****** Object:  UserDefinedFunction [dbo].[udfgetDecryptedPagesBasedOnPageNumber]    Script Date: 9/30/2016 12:52:17 AM ******/
DROP FUNCTION [dbo].[udfgetDecryptedPagesBasedOnPageNumber]
GO

/****** Object:  UserDefinedFunction [dbo].[udfgetDecryptedPagesBasedOnPageNumber]    Script Date: 9/30/2016 12:52:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE FUNCTION [dbo].[udfgetDecryptedPagesBasedOnPageNumber] (@DocId int,@PageNumber int,@DocPageEncrypted varchar(max))
RETURNS VARCHAR(max)
AS BEGIN
 declare @DocPageDecrypted varchar(max)
 select @DocPageDecrypted= CAST(CAST(N'' AS XML).value('(sql:variable("@DocPageEncrypted"))',
           'VARBINARY(MAX)') AS VARCHAR(MAX))   
		  
			return @DocPageDecrypted
END




GO

