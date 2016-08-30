USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllDistinctDocIdsWithMultiplePages]    Script Date: 8/29/2016 6:21:40 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[uspgetAllDistinctDocIdsWithMultiplePages]


AS
 
 	BEGIN

DECLARE @DocId int
declare @CountOfRows int 
DECLARE @DistinctDocIdsTable TABLE( 
docId int 
);
DECLARE @DocIdResultTable TABLE( 
docId int,
title   varchar(max) ,
doctype   varchar(max) ,
originsite_id   varchar(max) ,
docpagenum int,
docpagetitle varchar(max) 

);
DECLARE cur1 CURSOR FOR SELECT cast(DocId as varchar(max))  from sitepublisherii.dbo.Documents where SPSysEndTime is null 
--and DocId in(19413,6566,4115,6607,18031)
and DocId in(4162,2999,18151)
OPEN cur1

FETCH NEXT FROM cur1 INTO @DocId

WHILE @@FETCH_STATUS = 0 BEGIN


select  @CountOfRows=
				count(*) from 
				(  select   DocId,Title,DocType,OriginSite_ID,DocPageNum

from sitepublisherii.dbo.Documents 
join sitepublisherii.dbo.DocPages
on sitepublisherii.dbo.Documents.CurrentVersion_ID=sitepublisherii.dbo.DocPages.DocVer_Id
 where 
Cast(DocId as varchar(max) )=  @DocId
 and published =  'p'and sitepublisherii.dbo.Documents.SPSysEndTime is  null
 and sitepublisherii.dbo.DocPages.CurrentVersion=1
) as x

if (@CountOfRows>0)
begin
insert into @DocIdResultTable select   DocId,Title,DocType,OriginSite_ID,DocPageNum,docpagetitle

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


insert  into @DistinctDocIdsTable select distinct docid  from @DocIdResultTable ;
--select count(*) as 'count of distinct Docs with multiple pages' from @DistinctDocIdsTable
select * from @DistinctDocIdsTable order by DocId;
--select * from @DocIdResultTable order by doctype,DocId,docpagenum;


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


