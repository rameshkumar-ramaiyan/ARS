USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithoutParam]    Script Date: 5/12/2016 3:41:58 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithoutParam]


AS
 
BEGIN
	
	
	 
		select title,CurrentVersion_ID,doctype,Published,OriginSite_Type,OriginSite_ID,oldURL from sitepublisherii.dbo.Documents 
		  
		 where 
			
			-- published =  'p'
			--and 
			sitepublisherii.dbo.Documents.SPSysEndTime is  null		
			AND OriginSite_Type in ('Place' ,'person','ad_hoc')
			and DocType not in ('Program Reports', 'Program Planning','Program Inputs') 
			order by OriginSite_ID, OriginSite_Type , doctype, CurrentVersion_ID
			          
		
		
		



END







	----1.site queires
	--select siteLabel, site_Type ,site_Code,site_status  from sitepublisherii.dbo.Sites where site_Type='Place' and site_status=1
	--select siteLabel,site_Type,site_Code,site_status from sitepublisherii.dbo.Sites where site_Type='Person' and site_status=1
	--select siteLabel,site_Type,site_Code,site_status  from sitepublisherii.dbo.Sites where site_Type='ad_hoc' and site_status=1
	
	
	----1.document queires
	--select title,CurrentVersion_ID,doctype,Published,OriginSite_Type,OriginSite_ID,oldURL from sitepublisherii.dbo.Documents where OriginSite_Type='Place' and Published='p'
	--and DocType not in ('Program Reports', 'Program Planning','Program Inputs')  order by CurrentVersion_ID                                                                                                                                                                       
		
	--select title,CurrentVersion_ID,doctype,Published,OriginSite_Type,OriginSite_ID,oldURL from sitepublisherii.dbo.Documents where OriginSite_Type='person' and Published='p'
	 
	-- select title,CurrentVersion_ID,doctype,Published,OriginSite_Type,OriginSite_ID,oldURL from sitepublisherii.dbo.Documents where OriginSite_Type='ad_hoc' and Published='p'

	-- --1.document pages queires
	-- select * from  sitepublisherii.dbo.DocPages order by  docver_id,docpagenum
	--where DocVer_ID =(4877)  and CurrentVersion = 1







GO


