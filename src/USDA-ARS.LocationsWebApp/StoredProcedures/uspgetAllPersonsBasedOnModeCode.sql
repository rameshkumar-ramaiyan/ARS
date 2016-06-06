USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllPersonsBasedOnModeCode]    Script Date: 6/2/2016 7:23:57 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO












CREATE PROCEDURE [dbo].[uspgetAllPersonsBasedOnModeCode]
@ModeCode nvarchar(max) = NULL
AS
 
 	BEGIN
--	declare @ModeCode nvarchar(max)='60-00-00-00'
	SELECT 
   
    cast (SUBSTRING ([parent_Site_Code],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([parent_Site_Code],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([parent_Site_Code],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([parent_Site_Code],7, 2)as varchar(2)) as 'Mode Code'
	 ,[site_Code] as 'PersonId'
      ,[siteLabel] as 'Person Name'
	
	
   FROM [sitepublisherii].[dbo].[Sites]
  where 
       cast (SUBSTRING ([parent_Site_Code],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([parent_Site_Code],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([parent_Site_Code],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([parent_Site_Code],7, 2)as varchar(2))=@ModeCode
	and site_Type='person'
	and site_status=1

	and spsysendtime is NULL
and site_code in (select originsite_id from sitepublisherii.dbo.Documents where Title = 'index' and spsysendtime is NULL)
and parent_Site_Code  = [parent_Site_Code]


	
		--[Modecode]	=@ModeCode
	-- order by [OriginSite_ID]		
 
	END
	--select * from sitepublisherii.dbo.Sites where site_Code ='50572'
	--select * from sitepublisherii.dbo.Sites where site_Code ='45434'
	--select * from sitepublisherii.dbo.documents where OriginSite_ID='45434'

	
	--select * from sitepublisherii.dbo.Sites where site_Code='49130'
	--select * from sitepublisherii.dbo.documents where OriginSite_ID='49130'

	








GO


