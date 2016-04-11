-- Batch submitted through debugger: SQLQuery27.sql|7|0|C:\Users\Phani\AppData\Local\Temp\~vsF164.sql











CREATE PROCEDURE [dbo].[uspgetAllDocumentIdsBasedOnPersonId]
@PersonId varchar(max) =null
AS
 
 	BEGIN
--	declare @ModeCode nvarchar(max)='60-00-00-00'
	
   -- Declare the return variable here
   DECLARE  @SiteType varchar(50) ='person',
			@CurrentVersion_ID	int,
			@Title		varchar(50),
			@DocType	varchar(50)
			--,
			--@DocPage		nvarchar(max)
	-- Add the T-SQL statements to compute the return value here
	
		
	 --if @SiteType = 'person'
		--begin
		set	@SiteType = 'person'
			set @Title = 'index'
			set @DocType = 'pandp'
		--end
		
	
			

	select	@CurrentVersion_ID= CurrentVersion_ID
	from	[sitepublisherii].dbo.documents
	where	1 = 1
	and		published		= 'p'
	and		spsysendtime	IS NULL
	and		originsite_id	= 	@PersonId

	and		title			= COALESCE(@Title, title)
	and		DocType			= COALESCE(@DocType, DocType)
	
--RETURN @CurrentVersion_ID
	 --Return the result of the function
	
	DECLARE  
			@DocPage		nvarchar(max)
			
	
	select @DocPage=cast(DocPage as nvarchar(max))
	FROM 
	[sitepublisherii].dbo.docpages docpages

WHERE @CurrentVersion_ID=docpages.DocVer_ID 
--and docpages.CurrentVersion=1
		

 select CAST(CAST(N'' AS XML).value('(sql:variable("@DocPage"))',
           'VARBINARY(MAX)') AS VARCHAR(MAX)) 
		   AS [DocPageContent]
	


	END


		--select * from sitepublisherii.dbo.DocPages where DocVer_ID=23104 
		--select * from sitepublisherii.dbo.Sites where site_type='person' and SPSysEndTime is null and site_Code='198'