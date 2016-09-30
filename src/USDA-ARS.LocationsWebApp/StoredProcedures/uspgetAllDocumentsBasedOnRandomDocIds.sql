USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentsBasedOnRandomDocIds]    Script Date: 9/30/2016 12:51:19 AM ******/
DROP PROCEDURE [dbo].[uspgetAllDocumentsBasedOnRandomDocIds]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDocumentsBasedOnRandomDocIds]    Script Date: 9/30/2016 12:51:19 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[uspgetAllDocumentsBasedOnRandomDocIds]

@RandomDocId nvarchar(max) 
AS
 
BEGIN
	
	
	declare @PageNo int

	DECLARE  
			@DocPage		nvarchar(max)
			


select DocId,docpagenum,Title,CurrentVersion_ID,DocType,OriginSite_ID,oldURL,docpagetitle,
cast(sitepublisherii.dbo.DocPages.DocPage as varchar(max)) as DocPage
,dbo.udfgetDecryptedPagesBasedOnPageNumber(DocId,docpagenum,
cast(sitepublisherii.dbo.DocPages.DocPage as varchar(max))) AS DocPageDecrypted

from sitepublisherii.dbo.Documents 
join sitepublisherii.dbo.DocPages
on sitepublisherii.dbo.Documents.CurrentVersion_ID=sitepublisherii.dbo.DocPages.DocVer_Id
 where 
Cast(DocId as varchar(max) )=  @RandomDocId
 and published =  'p'and sitepublisherii.dbo.Documents.SPSysEndTime is  null
 and sitepublisherii.dbo.DocPages.CurrentVersion=1
order by DocType,DocPageNum
 

 

END

 -- all pages for NP 107
	--0.select * from sitepublisherii.dbo.NPGroups order by NPGroupID
	--1.select * from sitepublisherii.dbo.NPrograms  where npgroup_id=1 order by npgroup_id,NPNumber	
	--2.select * from sitepublisherii.dbo.NPrograms order by npgroup_id,NPNumber	
--3.select * from  sitepublisherii.dbo.Documents where OriginSite_ID =  '107'and published =  'p'and SPSysEndTime is  null
--4.select * from  sitepublisherii.dbo.DocPages where DocVer_ID =(22894)and CurrentVersion = 1

--select * from sitepublisherii.dbo.Documents
-- where DocId =  '8040'and published =  'p'and SPSysEndTime is  null order by CurrentVersion
--and Title  not like '%Strategic Vision%' 
--order by DocType




GO

