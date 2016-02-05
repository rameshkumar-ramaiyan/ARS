USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllResearchUnitsWebTrendsProfileIDs]    Script Date: 2/5/2016 4:23:27 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[uspgetAllResearchUnitsWebTrendsProfileIDs]
@ParentAreaModeCode int, 
@ParentCityModeCode int 
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
and SUBSTRING ([modecode],5, 2) is not null and SUBSTRING ([modecode],5, 2)<>'00' and SUBSTRING ([modecode],5, 2)<>'01' and SUBSTRING ([modecode],5, 2)<> '02'

 and  SUBSTRING ([modecode],7, 2)='00'
			
	END








GO

