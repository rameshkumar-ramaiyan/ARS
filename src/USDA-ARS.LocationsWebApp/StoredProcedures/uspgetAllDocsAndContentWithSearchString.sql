USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDocsAndContentWithSearchString]    Script Date: 8/29/2016 6:19:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE procEDURE [dbo].[uspgetAllDocsAndContentWithSearchString]
@SearchString nvarchar(max) = NULL
AS
 
 	BEGIN

DECLARE @DocId int
declare @CountOfRows int 
DECLARE @DistinctDocIdsTable TABLE( 
docId int,
title   varchar(max) ,
doctype   varchar(max) ,
originsite_id   varchar(max) 
 
);
DECLARE @DocIdResultTable TABLE( 
docId int,
title   varchar(max) ,
doctype   varchar(max) ,
originsite_id   varchar(max) ,
docpagenum int,
docpagetitle varchar(max) ,
docpagecontent   varchar(max) 
);
DECLARE cur1 CURSOR FOR SELECT cast(DocId as varchar(max))  from sitepublisherii.dbo.Documents where SPSysEndTime is null 
--and DocId in(19413,6566,4115,6607,18031)
and DocId in(4162,2999,18151)
OPEN cur1

FETCH NEXT FROM cur1 INTO @DocId

WHILE @@FETCH_STATUS = 0 BEGIN
DECLARE  
			@DocPage		nvarchar(max)
			select  @DocPage=cast(sitepublisherii.dbo.DocPages.DocPage as varchar(max)) from sitepublisherii.dbo.Documents 
join sitepublisherii.dbo.DocPages
on sitepublisherii.dbo.Documents.CurrentVersion_ID=sitepublisherii.dbo.DocPages.DocVer_Id
 where 
Cast(DocId as varchar(max) )=  @DocId
 and published =  'p'and sitepublisherii.dbo.Documents.SPSysEndTime is  null
 and sitepublisherii.dbo.DocPages.CurrentVersion=1
 --and sitepublisherii.dbo.DocPages.DocPageNum not in (1)
 --order by OriginSite_ID,DocId
 order by DocId,DocPageNum

select  @CountOfRows=
				count(*) from 
				(  select   DocId,Title,DocType,OriginSite_ID,DocPageNum,
CAST(CAST(N'' AS XML).value('(sql:variable("@DocPage"))',
           'VARBINARY(MAX)') AS VARCHAR(MAX)) 
		  AS DocPageContent 
from sitepublisherii.dbo.Documents 
join sitepublisherii.dbo.DocPages
on sitepublisherii.dbo.Documents.CurrentVersion_ID=sitepublisherii.dbo.DocPages.DocVer_Id
 where 
Cast(DocId as varchar(max) )=  @DocId
 and published =  'p'and sitepublisherii.dbo.Documents.SPSysEndTime is  null
 and sitepublisherii.dbo.DocPages.CurrentVersion=1
 --and sitepublisherii.dbo.DocPages.DocPageNum not in (1)
-- and DocType='Program Planning'                                                                                    

  and CAST(CAST(N'' AS XML).value('(sql:variable("@DocPage"))',
           'VARBINARY(MAX)') AS VARCHAR(MAX)) like '%'+@SearchString+'%') as x

if (@CountOfRows>0)
begin
insert into @DocIdResultTable select   DocId,Title,DocType,OriginSite_ID,DocPageNum,docpagetitle,

dbo.udfgetDecryptedPagesBasedOnPageNumber(DocId,docpagenum,cast(sitepublisherii.dbo.DocPages.DocPage as varchar(max))) AS DocPageDecrypted
from sitepublisherii.dbo.Documents 
join sitepublisherii.dbo.DocPages
on sitepublisherii.dbo.Documents.CurrentVersion_ID=sitepublisherii.dbo.DocPages.DocVer_Id


 where 
Cast(DocId as varchar(max) )=  LTRIM(RTRIM(@DocId))
 and published =  'p'and sitepublisherii.dbo.Documents.SPSysEndTime is  null
 and sitepublisherii.dbo.DocPages.CurrentVersion=1


 order by DocType, DocId ,DocPageNum 
 print 'DocId='+cast(@DocId as varchar(max)) 
end
   
    FETCH NEXT FROM cur1 INTO @DocId
END

CLOSE cur1    
DEALLOCATE cur1

select count(*) as 'count of Docs with search string provided' from @DocIdResultTable
insert  into @DistinctDocIdsTable select distinct docid,title,doctype,originsite_id  from @DocIdResultTable ;
select * from @DistinctDocIdsTable order by doctype,DocId;
select * from @DocIdResultTable order by doctype,DocId,docpagenum;


end

--select * from sitepublisherii.dbo.documents where docid=18031

--select * from  sitepublisherii.dbo.DocPages where DocVer_ID =(18225)and CurrentVersion = 1

--select * from sitepublisherii.dbo.Documents
-- where OriginSite_ID =  '107'and published =  'p'and SPSysEndTime is  null order by CurrentVersion
--and Title  not like '%Strategic Vision%' 
--order by DocType

--select * from sitepublisherii.dbo.documents where docid=19413

--select * from  sitepublisherii.dbo.DocPages where DocVer_ID =(19608)and CurrentVersion = 1

--select * from sitepublisherii.dbo.documents where docid=4162

--select * from  sitepublisherii.dbo.DocPages where DocVer_ID =(4346)and CurrentVersion = 1 order by docpagenum

GO


