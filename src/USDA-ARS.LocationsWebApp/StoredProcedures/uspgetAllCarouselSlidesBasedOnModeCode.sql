USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllCarouselSlidesBasedOnModeCode]    Script Date: 3/18/2016 4:53:38 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO










CREATE procEDURE [dbo].[uspgetAllCarouselSlidesBasedOnModeCode]
@ModeCode nvarchar(max) = NULL
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
	,'Slide-'+Cast(PhotoID as nvarchar(max)) as 'SlideName'
  FROM [redesign].[dbo].[PhotoCarousel]
  where 
       cast (SUBSTRING ([Modecode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([Modecode],7, 2)as varchar(2))=@ModeCode
		--[Modecode]	=@ModeCode
	 order by [Mode Code]		
 
	END











GO


