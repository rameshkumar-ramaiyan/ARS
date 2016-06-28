USE [aris_public_web]
GO

/****** Object:  StoredProcedure [dbo].[uspgetAllOldModeCodesBasedOnNewModeCodes]    Script Date: 6/2/2016 8:05:37 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







CREATE PROCEDURE [dbo].[uspgetAllOldModeCodesBasedOnNewModeCodes]
@ModeCode nvarchar(max) = NULL
AS
 
 	BEGIN
	
SELECT     distinct cast (SUBSTRING ([NewModeCode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([NewModeCode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([NewModeCode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([NewModeCode],7, 2)as varchar(2)),STUFF((    SELECT ',' + cast (SUBSTRING ([OldModeCode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OldModeCode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OldModeCode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([OldModeCode],7, 2)as varchar(2))

                       
                        FROM dbo.NewModecodes SUB
                        WHERE
                        SUB.NewModecode= CAT.NewModecode
                        FOR XML PATH('')
                        ), 1, 1, ''   )
						
                        
            AS [OldModeCodes]
FROM  dbo.NewModecodes CAT

where @ModeCode=cast (SUBSTRING ([NewModeCode],1, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([NewModeCode],3, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([NewModeCode],5, 2)as varchar(2))+'-'
	+ cast (SUBSTRING ([NewModeCode],7, 2)as varchar(2))

 
			
	END





	 


GO


