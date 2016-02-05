USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllAreasWebTrendsProfileIDs]    Script Date: 2/5/2016 4:20:34 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE PROCEDURE [dbo].[uspgetAllAreasWebTrendsProfileIDs]
@ModeCode int = NULL
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
 SUBSTRING ([Modecode],3, 2)='00'
	 order by [Mode Code]		
	END







GO

