USE [aris_public_webNew]
GO
/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithParam]    Script Date: 7/20/2016 3:29:11 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithParam]

@SiteType nvarchar(max)
AS

BEGIN


  IF @SiteType = 'place'
  BEGIN
   --checking condition if index page and is published for place then only fetch that page (could be index or other page as well )
    SELECT
      * INTO #TempDataTable
    FROM (
	select site_code from sitepublisherii.dbo.sites

where site_type = 'place'

and spsysendtime is NULL

and site_code in (select originsite_id from sitepublisherii.dbo.Documents where Title = 'index' and spsysendtime is NULL)

-- finds 306 valid adhoc sites
	) AS P


    IF (EXISTS (SELECT
        1
      FROM #TempDataTable)
      )
	  BEGIN
    SELECT
      title,
      CurrentVersion_ID,
      doctype,
      Published,
      OriginSite_Type,
      OriginSite_ID,
      oldURL,
	  DisplayTitle,
      DocId
	  ,HTMLHeader,keywords,    sitepublisherii.dbo.Sites.parent_Site_Code  ,sitepublisherii.dbo.Sites.siteLabel
    FROM sitepublisherii.dbo.Documents
	JOIN sitepublisherii.dbo.Sites
      ON sitepublisherii.dbo.Sites.site_code = sitepublisherii.dbo.Documents.OriginSite_ID
    WHERE
    --Cast(OriginSite_ID as varchar(max) )=  @RandomSiteId
    --and 
    --published =  'p'
    --and 
    sitepublisherii.dbo.Documents.SPSysEndTime IS NULL
	and sitepublisherii.dbo.Sites.SPSysEndTime IS NULL
    AND OriginSite_Type = 'Place'
    AND DocType NOT IN ('Program Reports', 'Program Planning', 'Program Inputs')
    ORDER BY doctype, CurrentVersion_ID

  END
  END
  IF @SiteType = 'person'
  BEGIN


    --checking condition if index page and is published for person then only fetch that page (could be index or other page as well )
    SELECT
      * INTO #TempDataTable2
    FROM (
	select site_code from sitepublisherii.dbo.sites

where site_type = 'person'

and spsysendtime is NULL

and site_code in (select originsite_id from sitepublisherii.dbo.Documents where Title = 'index' and spsysendtime is NULL)

-- finds 306 valid adhoc sites
	) AS P


    IF (EXISTS (SELECT
        1
      FROM #TempDataTable2)
      )

    BEGIN

      ---getting person pages which are satisfying above condition
      SELECT
        title,
        CurrentVersion_ID,
        doctype,
        Published,
        OriginSite_Type,
        OriginSite_ID,
        oldURL,
		DisplayTitle,
        DocId
		,HTMLHeader,keywords
      FROM sitepublisherii.dbo.Documents
      JOIN sitepublisherii.dbo.People
        ON CAST(sitepublisherii.dbo.People.PersonID AS varchar(max)) = sitepublisherii.dbo.Documents.OriginSite_ID
      WHERE 
      published = 'p'
      AND sitepublisherii.dbo.Documents.SPSysEndTime IS NULL
	 
      AND OriginSite_Type = 'person'
     
      ORDER BY doctype, CurrentVersion_ID
    END
  END



  IF @SiteType = 'ad_hoc'
  BEGIN
     --checking condition if index page and is valid for ad_hoc then only fetch that page (could be index or other page as well )
  SELECT
      * INTO #TempDataTable3
    FROM (
	select site_code from sitepublisherii.dbo.sites

where site_type = 'ad_hoc'

and spsysendtime is NULL

and site_code in (select originsite_id from sitepublisherii.dbo.Documents where Title = 'index' and spsysendtime is NULL)

-- finds 306 valid adhoc sites
	) AS P

    IF (EXISTS (SELECT
        1
      FROM #TempDataTable3)
      )

    BEGIN
    SELECT
      title,
      CurrentVersion_ID,
      doctype,
      Published,
      OriginSite_Type,
      OriginSite_ID,
      oldURL,
	  DisplayTitle,
      DocId
      ,HTMLHeader,keywords,    sitepublisherii.dbo.Sites.parent_Site_Code  ,sitepublisherii.dbo.Sites.siteLabel
    FROM sitepublisherii.dbo.Documents
    JOIN sitepublisherii.dbo.Sites
      ON sitepublisherii.dbo.Sites.site_Code = sitepublisherii.dbo.Documents.OriginSite_ID
    WHERE 
  --  sitepublisherii.dbo.Documents.Published =  'p'
    --and 
	sitepublisherii.dbo.Documents.SPSysEndTime IS NULL	
   -- AND sitepublisherii.dbo.Documents.OriginSite_Type = 'ad_hoc'

   -- and sitepublisherii.dbo.Sites.SPSysEndTime IS NULL
    and OriginSite_ID in (select site_code from sitepublisherii.dbo.sites

where site_type = 'ad_hoc'

and spsysendtime is NULL

and site_code in (select originsite_id from sitepublisherii.dbo.Documents where Title = 'index' and spsysendtime is NULL ))
    ORDER BY doctype, CurrentVersion_ID
  END
END


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

--select * from sitepublisherii.dbo.Documents where originsite_type='ad_hoc'
