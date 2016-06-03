USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllJobLocationIdsBasedOnCityName]    Script Date: 6/2/2016 8:06:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[uspgetAllJobLocationIdsBasedOnCityName]
@CityNameUpper nvarchar(max) = NULL
AS
 
 	BEGIN
	
SELECT     distinct LocationId,City
                        FROM sitepublisherii.dbo.USAJobsLocationId
                        WHERE
                       UPPER(LTRIM(RTRIM(City)))=UPPER(LTRIM(RTRIM(@CityNameUpper)))
                        
                        
            


 
			
	END





	 


GO


