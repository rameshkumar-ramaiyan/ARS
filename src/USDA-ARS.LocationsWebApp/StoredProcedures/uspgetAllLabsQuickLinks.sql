USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllLabsQuickLinks]    Script Date: 2/5/2016 4:21:55 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE PROCEDURE [dbo].[uspgetAllLabsQuickLinks]
@ParentAreaModeCode int, 
@ParentCityModeCode int ,
@ParentResearchUnitModeCode int 
AS
 
 	BEGIN
	SELECT 
    cast (SUBSTRING ([Modecode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],7, 2)as varchar(2)) as 'Mode Code'
      ,[Content]       as 'Quick Links'
  FROM [redesign].[dbo].[ReplacePeople]
  where 
  SUBSTRING ([Modecode],1, 2)=  @ParentAreaModeCode 

 and SUBSTRING ([Modecode],3, 2)=@ParentCityModeCode and SUBSTRING ([Modecode],3, 2)<>'01'
and SUBSTRING ([Modecode],5, 2)=@ParentResearchUnitModeCode and SUBSTRING ([Modecode],5, 2) is not null and SUBSTRING ([Modecode],5, 2)<>'00' and SUBSTRING ([Modecode],5, 2)<>'01' and SUBSTRING ([Modecode],5, 2)<> '02'

 and SUBSTRING ([Modecode],7, 2)is not null and SUBSTRING ([Modecode],7, 2)<>'01' and SUBSTRING ([Modecode],7, 2)<>'02'
 and ReplaceId<>73
  order by Modecode
---condition to be removed	--

--select * from [redesign].[dbo].[ReplacePeople] where Modecode='60400525'
	--select * from [redesign].[dbo].[ReplacePeople] where 


			
	END









GO

