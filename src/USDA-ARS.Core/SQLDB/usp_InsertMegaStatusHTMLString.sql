USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[usp_InsertMegaStatusHTMLString]    Script Date: 11/3/2015 1:58:12 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_InsertMegaStatusHTMLString]
	-- Add the parameters for the stored procedure here
	@HtmlString nvarchar(max)
	
AS
BEGIN TRY
  
     TRUNCATE table  dbo.MegaStatusHTMLTable 
	INSERT INTO dbo.MegaStatusHTMLTable(HtmlString)	values(@HtmlString)
 END TRY  
BEGIN CATCH
SELECT ERROR_NUMBER() AS ErrorNumber;
END CATCH 


GO


