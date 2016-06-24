-- Batch submitted through debugger: SQLQuery27.sql|7|0|C:\Users\Phani\AppData\Local\Temp\~vsF164.sql











CREATE PROCEDURE [dbo].[uspgetAllDocumentPagesBasedOnCurrentVersionId]
@CurrentVersionId int =0
AS
 
 	BEGIN
--	declare @ModeCode nvarchar(max)='60-00-00-00'
	
   -- Declare the return variable here
   DECLARE  
			@DocPage		nvarchar(max)
	
	
	 SELECT cast( DocPage as nvarchar(max))
	FROM 
	[sitepublisherii].dbo.docpages docpages

WHERE @CurrentVersionId=docpages.DocVer_ID and docpages.CurrentVersion=1

	RETURN @DocPage


	END


		--select * from sitepublisherii.dbo.DocPages where DocVer_ID=23104 