USE [aris_public_webNew]
GO
/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithParam]    Script Date: 5/12/2016 9:13:13 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[uspgetAllDocumentIdsBasedOnDocTypeWithParam]

--@RandomSiteId nvarchar(max) ,
@SiteType nvarchar(max)
AS

BEGIN


  IF @SiteType = 'place'
  BEGIN
    SELECT
      title,
      CurrentVersion_ID,
      doctype,
      Published,
      OriginSite_Type,
      OriginSite_ID,
      oldURL,
      DocId
    FROM sitepublisherii.dbo.Documents

    WHERE
    --Cast(OriginSite_ID as varchar(max) )=  @RandomSiteId
    --and 
    --published =  'p'
    --and 
    sitepublisherii.dbo.Documents.SPSysEndTime IS NULL
    AND OriginSite_Type = 'Place'
    AND DocType NOT IN ('Program Reports', 'Program Planning', 'Program Inputs')
    ORDER BY doctype, CurrentVersion_ID

  END

  IF @SiteType = 'person'
  BEGIN


    --checking condition if index page and is published for person then only fetch that page (could be index or other page as well )
    SELECT
      * INTO #TempDataTable
    FROM (SELECT
      sitepublisherii.dbo.Documents.Title,
      Published


    FROM sitepublisherii.dbo.Documents

    WHERE Documents.Title = 'Index'
    AND published = 'p') AS P

    IF (EXISTS (SELECT
        1
      FROM #TempDataTable)
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
        DocId
      FROM sitepublisherii.dbo.Documents
      JOIN sitepublisherii.dbo.People
        ON CAST(sitepublisherii.dbo.People.PersonID AS varchar(max)) = sitepublisherii.dbo.Documents.OriginSite_ID
      WHERE --Cast(OriginSite_ID as varchar(max) )=  @RandomSiteId
      --and 
      published = 'p'
      AND sitepublisherii.dbo.Documents.SPSysEndTime IS NULL
      AND OriginSite_Type = 'person'
      --and DocType not in ('Program Reports', 'Program Planning','Program Inputs') 
      ORDER BY doctype, CurrentVersion_ID
    END
  END



  IF @SiteType = 'ad_hoc'
  BEGIN
    SELECT
      title,
      CurrentVersion_ID,
      doctype,
      Published,
      OriginSite_Type,
      OriginSite_ID,
      oldURL,
      DocId,
      sitepublisherii.dbo.Sites.siteLabel
    FROM sitepublisherii.dbo.Documents
    JOIN sitepublisherii.dbo.Sites
      ON sitepublisherii.dbo.Sites.site_code = sitepublisherii.dbo.Documents.OriginSite_ID
    WHERE --Cast(OriginSite_ID as varchar(max) )=  @RandomSiteId
    --and 
    --published =  'p'
    --and
    sitepublisherii.dbo.Documents.SPSysEndTime IS NULL
    AND OriginSite_Type = 'ad_hoc'
    --and DocType not in ('Program Reports', 'Program Planning','Program Inputs')     
    ORDER BY doctype, CurrentVersion_ID
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