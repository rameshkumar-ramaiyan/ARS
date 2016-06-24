USE [aris_public_web]
GO
/****** Object:  StoredProcedure [dbo].[uspgetAllNPDocumentsBasedOnNPNumber]    Script Date: 4/22/2016 7:01:05 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[uspgetAllNPDocumentsBasedOnNPNumber]

@NPNumber nvarchar(max) 
AS
 
BEGIN
	
	
	
--	DECLARE  
--			@DocPage		nvarchar(max)
			
--select  @DocPage= cast( DocPage as nvarchar(max)) from  sitepublisherii.dbo.DocPages where DocVer_ID =(select currentversion_id from sitepublisherii.dbo.Documents
-- where 
-- --Title not like '%Strategic Vision%' and 
-- rtrim(ltrim(OriginSite_ID))=rtrim(ltrim(@NPNumber) )and 
-- published =  'p'and SPSysEndTime is  null
-- )
----179
--and CurrentVersion = 1

-- select CAST(CAST(N'' AS XML).value('(sql:variable("@DocPage"))',
--           'VARBINARY(MAX)') AS VARCHAR(MAX)) 
--		   AS [DocPageContent]


select DocId,Title,CurrentVersion_ID,DocType,OriginSite_ID,oldURL,
cast(sitepublisherii.dbo.DocPages.DocPage as varchar(max)) as DocPage
--CAST(CAST(N'' AS XML).value('(sql:variable("@DocPage"))',
--           'VARBINARY(MAX)') AS VARCHAR(MAX)) 
--		   AS [DocPageContent]
from sitepublisherii.dbo.Documents 
join sitepublisherii.dbo.DocPages
on sitepublisherii.dbo.Documents.CurrentVersion_ID=sitepublisherii.dbo.DocPages.DocVer_Id
 where 
Cast(OriginSite_ID as varchar(max) )=  @NPNumber
 and published =  'p'and sitepublisherii.dbo.Documents.SPSysEndTime is  null
 and sitepublisherii.dbo.DocPages.CurrentVersion=1
-- and DocType='Program Planning'                                                                                    
--and Title   like '%Action Plans%' 
order by DocType
 

END

 -- all pages for NP 107
	--0.select * from sitepublisherii.dbo.NPGroups order by NPGroupID
	--1.select * from sitepublisherii.dbo.NPrograms  where npgroup_id=1 order by npgroup_id,NPNumber	
	--2.select * from sitepublisherii.dbo.NPrograms order by npgroup_id,NPNumber	
--3.select * from  sitepublisherii.dbo.Documents where OriginSite_ID =  '107'and published =  'p'and SPSysEndTime is  null
--4.select * from  sitepublisherii.dbo.DocPages where DocVer_ID =(22894)and CurrentVersion = 1

--select * from sitepublisherii.dbo.Documents
-- where OriginSite_ID =  '107'and published =  'p'and SPSysEndTime is  null order by CurrentVersion
--and Title  not like '%Strategic Vision%' 
--order by DocType


--BEGIN
	
--	DECLARE  
--			@DocPage		nvarchar(max)
			

--select  @DocPage= cast( DocPage as nvarchar(max)) from  sitepublisherii.dbo.DocPages where DocVer_ID =(select currentversion_id from sitepublisherii.dbo.Documents
-- where 
-- Title not like '%Strategic Vision%' and 
-- rtrim(ltrim(OriginSite_ID))=rtrim(ltrim(@NPNumber) )and 
-- published =  'p'and SPSysEndTime is  null
-- )
----179
--and CurrentVersion = 1
 
-- select CAST(CAST(N'' AS XML).value('(sql:variable("@DocPage"))',
--           'VARBINARY(MAX)') AS VARCHAR(MAX)) 
--		   AS [DocPageContent]
--END




