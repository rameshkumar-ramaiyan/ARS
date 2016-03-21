USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllAreasCarouselSlides]    Script Date: 3/18/2016 4:52:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








CREATE PROCEDURE [dbo].[uspgetAllAreasCarouselSlides]
@ModeCode int = NULL
AS
 
 	BEGIN
	SELECT 
   
    cast (SUBSTRING ([Modecode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],7, 2)as varchar(2)) as 'Mode Code'
	,FileName as 'FileName'
	,URL as 'URL'
	,AlternateText as 'AlternateText'
	,Caption as 'Caption'
  FROM [redesign].[dbo].[PhotoCarousel]
  where 
 SUBSTRING ([Modecode],3, 2)='00'
	 order by [Mode Code]		
 
	END









GO


