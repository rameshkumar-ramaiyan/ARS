USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentIdPagesBasedOnCurrentVersion]    Script Date: 5/12/2016 3:41:38 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- Batch submitted through debugger: SQLQuery27.sql|7|0|C:\Users\Phani\AppData\Local\Temp\~vsF164.sql











CREATE PROCEDURE [dbo].[uspgetAllDocumentIdPagesBasedOnCurrentVersion]
@CurrentVersionId int =0
AS
 
 	BEGIN
--	declare @ModeCode nvarchar(max)='60-00-00-00'
	
   -- Declare the return variable here
   DECLARE  
			@DocPage		nvarchar(max)
	
	
	 SELECT  DocPageNum
	 ,cast( DocPage as nvarchar(max)) as 'EncDocPage'
	 ,DocVer_ID as CurrentVersion
	FROM 
	[sitepublisherii].dbo.docpages docpages

WHERE @CurrentVersionId=docpages.DocVer_ID and docpages.CurrentVersion=1

	--RETURN @DocPage


	END


		--select * from sitepublisherii.dbo.DocPages where DocVer_ID=23104 











GO


