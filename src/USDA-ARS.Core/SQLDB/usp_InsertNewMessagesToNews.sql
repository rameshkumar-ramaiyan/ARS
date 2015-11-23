USE [Lyris]
GO
/****** Object:  StoredProcedure [dbo].[usp_GetLatestNewsFromLyrisMessages]    Script Date: 11/23/2015 9:12:17 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[usp_GetLatestNewsFromLyrisMessages]
	
AS
BEGIN
declare @maxMessageId INT;
set  @maxMessageId=(	select	max(MessageID) as MessageID	from sitepublisherii.dbo.NewsTemp
	where 	ToField = 'ARS News subscriber <spNewsFeeder@ars.usda.gov>')
CREATE TABLE #TempTable(
 DateField datetime,
 FromField nvarchar(max),
 ToField nvarchar(max),
 ReplytoField nvarchar(max),
 MessageNumberField nvarchar(max),
 SubjectField nvarchar(max),
 BodyField nvarchar(max),
 HeaderField nvarchar(max),
 originsite_type nvarchar(max),
 originsite_id nvarchar(max),
 published nvarchar(max),
 MessageID nvarchar(max),
 ISFileName nvarchar(max),
 )
	insert into #TempTable
		select  
		CreatStamp_,
		HdrFrom_,
		'ARS News subscriber <spNewsFeeder@ars.usda.gov>',
		HdrFrom_,
		'1',
		HdrSubject_,
		Body_,
		HdrAll_,
		'place',
		'01040000',
		'01040000',	
		'p',
		MessageID_,
		convert(varchar(10), CreatStamp_, 120)+'.htm'	
		from Lyris.dbo.messages_
	
	where		MessageID_ > @maxMessageId	

	select * from #TempTable

	INSERT INTO sitepublisherii.dbo.NewsTemp 
					(
						DateField, 
						FromField, 
						ToField, 
						ReplytoField, 
						MessageNumberField,
						SubjectField, 
						BodyField, 
						HeaderField,
						originsite_type,
						originsite_id,
						published,
						MessageID,								-- chg-01
						ISFileName								-- chg-01
					)
			select DateField, 
						FromField, 
						ToField, 
						ReplytoField, 
						MessageNumberField,
						SubjectField, 
						BodyField, 
						HeaderField,
						originsite_type,
						originsite_id,
						published,
						MessageID,								-- chg-01
						ISFileName		
						from #TempTable						-- chg-01

	truncate table #temptable
END
