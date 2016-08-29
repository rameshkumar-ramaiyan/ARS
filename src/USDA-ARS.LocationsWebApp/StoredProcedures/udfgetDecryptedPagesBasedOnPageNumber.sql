USE [aris_public_webNew]
GO

/****** Object:  UserDefinedFunction [dbo].[udfgetDecryptedPagesBasedOnPageNumber]    Script Date: 8/29/2016 6:22:12 PM ******/
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


