USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentIdsBasedOnPersonId]    Script Date: 3/18/2016 4:54:26 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Batch submitted through debugger: SQLQuery27.sql|7|0|C:\Users\Phani\AppData\Local\Temp\~vsF164.sql











CREATE PROCEDURE [dbo].[uspgetAllDocumentIdsBasedOnPersonId]
@PersonId varchar(max) =null
AS
 
 	BEGIN
--	declare @ModeCode nvarchar(max)='60-00-00-00'
	
   -- Declare the return variable here
   DECLARE  @SiteType varchar(50) ='person',
			@DocID		int,
			@Title		varchar(50),
			@DocType	varchar(50),
			@DocPage		nvarchar(max)
	-- Add the T-SQL statements to compute the return value here
	
		
	 if @SiteType = 'person'
		begin
			set @Title = 'index'
			set @DocType = 'pandp'
		end
		
	
			

	select	@DocID = DocID
	from	[sitepublisherii].dbo.documents
	where	1 = 1
	and		published		= 'p'
	and		spsysendtime	IS NULL
	and		originsite_id	= 	@PersonId

	and		title			= COALESCE(@Title, title)
	and		DocType			= COALESCE(@DocType, DocType)
--RETURN @DocID
	 --Return the result of the function
	SELECT @DocPage= DocPage
FROM [sitepublisherii].dbo.documents, [sitepublisherii].dbo.docversions, [sitepublisherii].dbo.docpages, [sitepublisherii].dbo.contentgroups c
WHERE currentversion_id = docverid
AND docver_id = docverid
AND documents.spsysendtime is null
AND c.cgid =[sitepublisherii].dbo.documents.cg_id
AND docpagenum = 1
AND docid = @DocID
--AND docid = 22878
and CurrentVersion = 1        
	
	RETURN @DocPage


	END

	--select * from sitepublisherii.dbo.documents where originsite_id











GO


