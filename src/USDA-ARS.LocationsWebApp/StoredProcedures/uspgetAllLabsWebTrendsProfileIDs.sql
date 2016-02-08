USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllLabsWebTrendsProfileIDs]    Script Date: 2/5/2016 4:22:32 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








create PROCEDURE [dbo].[uspgetAllLabsWebTrendsProfileIDs]
@ParentAreaModeCode int, 
@ParentCityModeCode int ,
@ParentResearchUnitModeCode int 
AS
 
 	BEGIN
	SELECT 
    cast (SUBSTRING ([modecode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([modecode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([modecode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([modecode],7, 2)as varchar(2)) as 'Mode Code'
      ,ProfileID      as 'WebTrendsProfileID'
  FROM [sitepublisherii].[dbo].[WebTrendsProfiles]
  where 
  SUBSTRING ([modecode],1, 2)=  @ParentAreaModeCode 

 and SUBSTRING ([modecode],3, 2)=@ParentCityModeCode and SUBSTRING ([modecode],3, 2)<>'01'
and SUBSTRING ([modecode],5, 2)=@ParentResearchUnitModeCode and SUBSTRING ([modecode],5, 2) is not null and SUBSTRING ([modecode],5, 2)<>'00' and SUBSTRING ([modecode],5, 2)<>'01' and SUBSTRING ([modecode],5, 2)<> '02'

 and SUBSTRING ([modecode],7, 2)is not null and SUBSTRING ([modecode],7, 2)<>'01' and SUBSTRING ([modecode],7, 2)<>'02'

  order by modecode
---condition to be removed	--

--select * from [redesign].[dbo].[ReplacePeople] where Modecode='60400525'
	--select * from [redesign].[dbo].[ReplacePeople] where 

	--select *  from aris_public_webNew.dbo.REF_MODECODE 
	--where MODECODE_1=60 and MODECODE_2=40 and MODECODE_3=5


			
	END









GO

