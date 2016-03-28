USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllResearchUnitsPopularTopics]    Script Date: 3/18/2016 4:56:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[uspgetAllResearchUnitsPopularTopics]
@ParentAreaModeCode int, 
@ParentCityModeCode int 
AS
 
 	BEGIN
	SELECT 
    cast (SUBSTRING ([Modecode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],7, 2)as varchar(2)) as 'Mode Code'
      ,[URL]       as 'Popular Topic Links'
	  ,Label as 'Label'
  FROM [redesign].[dbo].[PopularLink]
  where 
  SUBSTRING ([Modecode],1, 2)=  @ParentAreaModeCode 

 and SUBSTRING ([Modecode],3, 2)=@ParentCityModeCode and SUBSTRING ([Modecode],3, 2)<>'01'
and SUBSTRING ([Modecode],5, 2) is not null and SUBSTRING ([Modecode],5, 2)<>'00' and SUBSTRING ([Modecode],5, 2)<>'01' and SUBSTRING ([Modecode],5, 2)<> '02'

 and  SUBSTRING ([Modecode],7, 2)='00'
			
	END








GO


