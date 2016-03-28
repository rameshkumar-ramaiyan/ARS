USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllAreasPopularTopics]    Script Date: 3/18/2016 4:53:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[uspgetAllAreasPopularTopics]
@ModeCode int = NULL
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
 SUBSTRING ([Modecode],3, 2)='00'
	 order by [Mode Code]		
	END








GO


