USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllSoftwaresBasedOnModeCode]    Script Date: 3/18/2016 4:51:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO











CREATE procEDURE [dbo].[uspgetAllSoftwaresBasedOnModeCode]
@ModeCode nvarchar(max) = NULL
AS
 
 	BEGIN
	SELECT 
   
    cast (SUBSTRING ([OriginSite_ID],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OriginSite_ID],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OriginSite_ID],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OriginSite_ID],7, 2)as varchar(2)) as 'Mode Code'	
	,SoftwareID as 'SoftwareID'
	,Title as 'Title'
	,recipients as 'recipients'
	,blurb as 'blurb'
	,info as 'info'
	
	
  FROM [sitepublisherii].dbo.[Software]
  where 
       cast (SUBSTRING ([OriginSite_ID],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OriginSite_ID],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OriginSite_ID],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OriginSite_ID],7, 2)as varchar(2))=@ModeCode

	and originsite_type='Place'
		--[Modecode]	=@ModeCode
	 order by [OriginSite_ID]		
 
	END

	--select * from sitepublisherii.dbo.Software










GO


