USE [aris_public_webNew]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllAreasQuickLinks]    Script Date: 2/5/2016 4:20:02 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





create PROCEDURE [dbo].[uspgetAllAreasQuickLinks]
@ModeCode int = NULL
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
 SUBSTRING ([Modecode],3, 2)='00'
	 order by [Mode Code]		
	END






GO

