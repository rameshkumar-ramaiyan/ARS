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
	set  @maxMessageId=(	select	max(MessageID) as MessageID	from sitepublisherii.dbo.News
		where 	ToField = 'ARS News subscriber <spNewsFeeder@ars.usda.gov>')
	CREATE TABLE #TempTable(
							 --1
							 DateField datetime,
							 --2
							 FromField nvarchar(max),
							 --3
							 ToField nvarchar(max),
							 --4
							 ReplytoField nvarchar(max),
							 --5
							 MessageNumberField nvarchar(max),
							 --6
							 SubjectField nvarchar(max),
							 --7
							 BodyField nvarchar(max),
							 --8
							 HeaderField nvarchar(max),
							 --9
							 originsite_type nvarchar(max),
							 --10
							 originsite_id nvarchar(max),
							 --11
							 published nvarchar(max),
							 --12
							 MessageID nvarchar(max),
							 --13
							 ISFileName nvarchar(max),
	 )
		insert into #TempTable select			  
							--1
							CreatStamp_,
							--2
							HdrFrom_,
							--3
							'ARS News subscriber <spNewsFeeder@ars.usda.gov>',
							--4
							HdrFrom_,
							--5
							'1',
							--6
							HdrSubject_,
							--7
							Body_,
							--8
							HdrAll_,
							--9
							'place',
							--10
							'01040000',
								
							--11
							'p',
							--12
							MessageID_,
							--13
							convert(varchar(10), CreatStamp_, 120)+'.htm'	
		from Lyris.dbo.messages_		
		where	MessageID_ > @maxMessageId	
		--and HdrTo_  = 'ARS News subscriber <spNewsFeeder@ars.usda.gov>'

		select * from #TempTable
		--truncate table #TempTable
		--end
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
		select 				
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
							ISFileName		
							from #TempTable						-- chg-01
							
	 select * from sitepublisherii.dbo.NewsTemp 
	 --truncate table #temptable
	END
	--exec [dbo].[usp_GetLatestNewsFromLyrisMessages]
	--select * from sitepublisherii.dbo.NewsTemp 