USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllNPDocumentsBasedOnNPNumber]    Script Date: 4/11/2016 7:13:24 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[uspgetAllNPDocumentsBasedOnNPNumber]

@NPNumber nvarchar(max) 
AS
 
BEGIN
	
	DECLARE  
			@DocPage		nvarchar(max)
			

select  @DocPage= cast( DocPage as nvarchar(max)) from  sitepublisherii.dbo.DocPages where DocVer_ID =(select currentversion_id from sitepublisherii.dbo.Documents
 where Title not like '%Strategic Vision%' and rtrim(ltrim(OriginSite_ID))=rtrim(ltrim(@NPNumber) )and published =  'p'and SPSysEndTime is  null)
--179
and CurrentVersion = 1
 
 --select CAST(CAST(N'' AS XML).value('(sql:variable("@DocPage"))',
 --          'VARBINARY(MAX)') AS VARCHAR(MAX)) 
	--	   AS [DocPageContent]
END

 -- all pages for NP 107
	--0.select * from sitepublisherii.dbo.NPGroups order by NPGroupID
	--1.select * from sitepublisherii.dbo.NPrograms  where npgroup_id=1 order by npgroup_id,NPNumber	
	--2.select * from sitepublisherii.dbo.NPrograms order by npgroup_id,NPNumber	
--3.select * from  sitepublisherii.dbo.Documents where OriginSite_ID =  '107'and published =  'p'and SPSysEndTime is  nul
--4.select * from  sitepublisherii.dbo.DocPages where DocVer_ID !=(179)and CurrentVersion = 1

--select * from sitepublisherii.dbo.Documents
-- where OriginSite_ID =  '107'and published =  'p'and SPSysEndTime is  null
--and Title  not like '%Strategic Vision%' 
--order by DocType
GO


