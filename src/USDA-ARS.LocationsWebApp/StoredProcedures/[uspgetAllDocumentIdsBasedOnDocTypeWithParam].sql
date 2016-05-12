USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithParam]    Script Date: 5/12/2016 3:41:05 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithParam]

--@RandomSiteId nvarchar(max) ,
@SiteType nvarchar(max) 
AS
 
BEGIN
	
	
	 if @SiteType = 'place'
		begin
		select title,CurrentVersion_ID,doctype,Published,OriginSite_Type,OriginSite_ID,oldURL from sitepublisherii.dbo.Documents 
		  
		 where 
			--Cast(OriginSite_ID as varchar(max) )=  @RandomSiteId
			--and 
			--published =  'p'
			--and 
			sitepublisherii.dbo.Documents.SPSysEndTime is  null		
			AND OriginSite_Type='Place' 
			and DocType not in ('Program Reports', 'Program Planning','Program Inputs') 
			order by doctype, CurrentVersion_ID
			          
		end
		
		if @SiteType = 'person'
		begin
		select title,CurrentVersion_ID,doctype,Published,OriginSite_Type,OriginSite_ID,oldURL from sitepublisherii.dbo.Documents 
		where --Cast(OriginSite_ID as varchar(max) )=  @RandomSiteId
			--and 
			published =  'p'
			and sitepublisherii.dbo.Documents.SPSysEndTime is  null		
			AND OriginSite_Type='person' 
			--and DocType not in ('Program Reports', 'Program Planning','Program Inputs') 
			order by doctype, CurrentVersion_ID       
		end
		if @SiteType = 'ad_hoc'
		begin
		select title,CurrentVersion_ID,doctype,Published,OriginSite_Type,OriginSite_ID,oldURL from sitepublisherii.dbo.Documents 
		where --Cast(OriginSite_ID as varchar(max) )=  @RandomSiteId
			--and 
			--published =  'p'
			--and
			 sitepublisherii.dbo.Documents.SPSysEndTime is  null		
			AND OriginSite_Type='ad_hoc' 
			--and DocType not in ('Program Reports', 'Program Planning','Program Inputs')     
			order by doctype, CurrentVersion_ID   
		end



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


